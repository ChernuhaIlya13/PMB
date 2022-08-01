using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using Microsoft.Extensions.Options;
using PMB.Application.Interfaces;
using PMB.Browsers.Common;
using PMB.Cef.Core;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;

namespace PMB.Fonbet;

public class FonbetDecorator : BotBrowserDecoratorBase
{
    private ISettingsProvider _settingsProvider { get; }

    private readonly TimeSpan WaitForMainBlockEventAndAnyStake = TimeSpan.FromSeconds(20);

    public FonbetDecorator(IOptions<SettingsOptions> settingsOptions, JsLoader loader,ISettingsProvider settingsProvider) : base(settingsOptions.Value, loader)
    {
        _settingsProvider = settingsProvider;
    }

    public override string BookmakerName { get; } = "fonbet";
    public override Task<BotBrowser> CreateBrowser(Bookmaker bookmaker, IPanelLogger logger, CancellationToken token) => CreateBrowserBase(bookmaker, Loader, logger, token);

    public override async Task<bool> Login(Bookmaker bookmaker, CancellationToken token)
    {
        var logined = false;
        var (uri, authOptions, bUrls, host) = await GetBookmakerCredentials(bookmaker, SettingsOptions);
        logined = await Browser.Execute<bool>("isLogined");
        if (!logined)
        {
            await Task.WhenAny(new Task[]
            {
                Browser.Worker.EasyLogin(authOptions.Login, authOptions.Password), 
                Browser.Worker.BrowserLoaded
                .Where(x => x.Contains(host, StringComparison.OrdinalIgnoreCase))
                .FirstAsync().ToTask(token)
            });
            var cnt = 0;
            while (cnt < 5 && !logined)
            {
                logined = await Browser.ExecuteAsync<bool>("isLogined");
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
        var openedEvent = await Browser.ExecuteAsync<bool>("openEvent", bet.EvId);
        if (!openedEvent)
        {
            return false;
        }
        var mainBlockEventLoaded = false;
        var timer = new Stopwatch();
        timer.Start();
        var canExecuteJavascript = false;
        Browser.FrameLoadStart += (sender, args) =>
        {
            if (Browser.CanExecuteJavascriptInMainFrame)
            {
                canExecuteJavascript = true;
            }
        };
        while (!mainBlockEventLoaded && timer.Elapsed < WaitForMainBlockEventAndAnyStake)
        {
            if (canExecuteJavascript)
            {
                mainBlockEventLoaded = await Browser.Execute<bool>("mainBlockEventAndAnyStakeExist");
            }
        }
        timer.Reset();
        return mainBlockEventLoaded;
    }

    public override async Task<bool> FindStake(BetMain bet)
    {
        return await Browser.Worker.FindStakeAndClick(bet.BetId);
    }

    public override async Task<bool> SetStakeSum(decimal sum)
    {
        return await Browser.Worker.SetStakeSum(sum);
    }

    public override async Task<StakeCoefficientResult> DoStake()
    {
        var doStakeResult = await Browser.Worker.DoStake();
        if (doStakeResult)
        {
            return new StakeCoefficientResult(true, 0);
        }

        return new StakeCoefficientResult(false, 0);
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
        var minStake = await Browser.ExecuteAsync<double>("getStakeMin");
        var maxStake = await Browser.ExecuteAsync<double>("getStakeMax");
        return ((decimal)minStake, (decimal)maxStake);
    }

    public override Task<(double firstStakeCoef, double secondStakeCoef)> GetStakesCoefs(BetMain firstBet, BetMain secondBet)
    {
        return Task.FromResult((0d, 0d));
    }

    public override async Task<bool> CheckGameInTennis(string eventId)
    {
        var validGames = await Browser.ExecuteAsync<bool>("checkGameInTennis",eventId);
        if (!validGames)
        {
            return false;
        }
        return true;
    }

    protected override Func<RequestContextSettings, string, Bookmaker, BotBrowser> Factory => 
        (settings, recaptcha, bookmaker) => new FonbetBrowser(bookmaker, recaptcha, settings);
}