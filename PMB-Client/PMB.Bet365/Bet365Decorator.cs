using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using Microsoft.Extensions.Options;
using PMB.Application.Interfaces;
using PMB.Bet365.Models;
using PMB.Browsers.Common;
using PMB.Cef.Core;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;

namespace PMB.Bet365
{
    public class Bet365Decorator: BotBrowserDecoratorBase
    {
        public override string BookmakerName => "bet365";
        private ISettingsProvider _settingsProvider;

        public Bet365Decorator(IOptions<SettingsOptions> settingsOptions, JsLoader loader,
            ISettingsProvider settingsProvider)
            : base(settingsOptions.Value, loader)
        {
            _settingsProvider = settingsProvider;
        }

        public override Task<BotBrowser> CreateBrowser(Bookmaker bookmaker, IPanelLogger logger, CancellationToken token) =>
            CreateBrowserBase(bookmaker, Loader, logger, token);

        public override async Task<bool> Login(Bookmaker bookmaker, CancellationToken token)
        {
            var logined = false;
            var (uri, authOptions, bUrls, host) = await GetBookmakerCredentials(bookmaker, SettingsOptions);
            logined = await Browser.Execute<bool>("isLogined");
            if (!logined)
            {
                if (uri.Contains(bUrls.FirstOrDefault() == null ? "" : bUrls.First()) )
                {
                    await Task.WhenAny(new Task[]
                    {
                        Browser.Worker.EasyLogin(authOptions.Login, authOptions.Password), 
                        Browser.Worker.BrowserLoaded
                            .Where(x => x.Contains(host, StringComparison.OrdinalIgnoreCase))
                            .FirstAsync().ToTask(token)
                    });
                }

                await Task.Delay(5000);
                var cnt = 0;
                while (cnt < 5 && !logined)
                {
                    logined = await Browser.Execute<bool>("isLogined");
                    if (logined)
                        break;
                    cnt++;
                    await Task.Delay(1000, token);
                }
            }
            return logined;
        }

        public override async Task<bool> LoadEvents(BetMain bet)
        {
            var teams = bet.Teams.Split("|");
            var firstTeam = teams.First();
            var secondTeam = teams.Skip(1).First();

            var result = await Browser.Worker.OpenUrl(new OpenUrlParam(bet.Url,bet.Sport,bet.SportId.ToString(),firstTeam,secondTeam));

            return result;
        }

        public override async Task<bool> FindStake(BetMain bet)
        {
            return await Browser.Worker.FindStakeAndClick(bet.BetId);
        }

        public override async Task<bool> SetStakeSum(decimal sum)
        {
            return await Browser.Execute<bool>("setValueSum",sum);
        }

        public override async Task<StakeCoefficientResult> DoStake()
        {
            var result = await Browser.Worker.DoStake();
            if (!result)
                return new (false, 0);
            var checkWindowResult = await Browser.Worker.CheckWindowResult();
            return new (checkWindowResult, 0);
        }

        public override async Task<StakeCoefficientResult> DoStakeWithLoginCheck()
        {
            var logined = await Browser.Execute<bool>("isLogined");
            if (!logined)
            {
                var settings = _settingsProvider.GetSettings();
                var bookmakerSettings = settings.Bookmakers.FirstOrDefault(b => b.BookmakerName == BookmakerName);
                logined = await Login(bookmakerSettings, CancellationToken.None);
            }

            if (!logined)
                return new(false,0);
            return await DoStake();
        }

        public override Task<decimal> GetPuttedCoefStake()
        {
            return Task.FromResult(0m);
        }

        public override async Task<(decimal MinStake, decimal MaxStake)> GetMinMaxStake(BetMain bet)
        {
            return await Task.FromResult((0.1m, 0m));
        }

        public override async Task<(double firstStakeCoef, double secondStakeCoef)> GetStakesCoefs(BetMain firstBet, BetMain secondBet)
        {
            var result = await Browser.Execute<StakesResultModel>("getCoupleStakesCoefs");
            return (Convert.ToDouble(result.FirstStakeCoef), Convert.ToDouble(result.SecondStakeCoef));
        }

        public override async Task<bool> CheckGameInTennis(string eventId)
        {
            var isValidGameInTennis = await Browser.Execute<bool>("checkGameInTennis");
            return isValidGameInTennis;
        }

        protected override Func<RequestContextSettings, string, Bookmaker, BotBrowser> Factory => 
            (settings, recaptcha, bookmaker) => new Bet365Browser(bookmaker, recaptcha, settings);
    }
}