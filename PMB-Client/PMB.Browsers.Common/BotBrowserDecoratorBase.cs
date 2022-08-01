using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PMB.Browsers.Common.Exceptions;
using PMB.Cef.Core;
using PMB.Cef.Core.Extensions;
using PMB.Cef.Core.Handlers;
using PMB.Cef.Core.JsProxy;
using PMB.Domain;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;
using PMB.Utils;
using RequestContextSettings = CefSharp.RequestContextSettings;

namespace PMB.Browsers.Common
{
    public abstract class BotBrowserDecoratorBase: IBotBrowserDecorator
    {
        public Window BrowserWindow { get; set; }
        public BotBrowser Browser { get; set; }
        
        public abstract string BookmakerName { get; }
        
        protected JsLoader Loader { get; }
        
        protected SettingsOptions SettingsOptions { get; }

        protected abstract Func<RequestContextSettings, string, Bookmaker, BotBrowser> Factory { get; }

        protected BotBrowserDecoratorBase(SettingsOptions settingsOptions, JsLoader loader)
        {
            Loader = loader;
            SettingsOptions = settingsOptions;
        }
        
        protected async Task<BotBrowser> CreateBrowserBase(Bookmaker bookmaker, JsLoader loader, IPanelLogger logger, CancellationToken token)
        {
            if (!bookmaker.BookmakerName.Equals(BookmakerName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new BookmakerNotFoundException(bookmaker.BookmakerName);
            }

            var botBrowser = BotBrowserFactory.CreateBrowser(bookmaker, loader, Factory);

            var proxy = bookmaker.BrowserOptions.Proxy;
            
            token.ThrowIfCancellationRequested();
            
            if (proxy.UseProxy)
            {
                botBrowser.RequestHandler = new RequestHandlerCustom(proxy.Login, proxy.Password);
                await botBrowser.SetProxy(bookmaker.BrowserOptions.Proxy.ToProxyString());
            }

            var grid = GetWindowContent(botBrowser);
            
            var window = new Window {Content = grid, WindowState = WindowState.Maximized};
            window.Closing += (sender, args) =>
            {
                Browser.Dispose();
                BrowserWindow = null;
                ViewModelUpdater.BrowserRemove.OnNext(botBrowser.BookmakerName);
            };
            BrowserWindow = window;
            window.Show();

            Browser = botBrowser;
            
            if (logger != null)
            {
                Browser.SubscribeOnLogger(logger);
            }
            
            return botBrowser;
        }

        public Grid GetWindowContent(BotBrowser botBrowser)
        {
            var grid = new Grid()
            {
                RowDefinitions =
                {
                    new RowDefinition()
                    {
                        Height = new GridLength(1,GridUnitType.Star)
                    },
                    new RowDefinition()
                    {
                        Height = new GridLength(25,GridUnitType.Star)
                    }
                },
            };
            var searchPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            var leftBtn = new Button()
            {
                Content = "Назад",
                
            };
            leftBtn.Command = botBrowser.BackCommand;
            var rightBtn = new Button()
            {
                Content = "Вперёд"
            };
            rightBtn.Command = botBrowser.ForwardCommand;
            var searchBox = new TextBox()
            {
                Text = "Поиск",
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxWidth = 1000,
                MinWidth = 800,
                TextAlignment = TextAlignment.Center,
                FontSize = 20
            };
            searchBox.KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Return)
                {
                    botBrowser.LoadUrlAsync(searchBox.Text);
                }
            };
            var btnFind = new Button()
            {
                Content = "Найти"
            };
            var reloadBtn = new Button()
            {
                Content = "Обновить"
            };
            reloadBtn.Command = botBrowser.ReloadCommand;
            searchPanel.Children.Add(leftBtn);
            searchPanel.Children.Add(rightBtn);
            searchPanel.Children.Add(searchBox);
            searchPanel.Children.Add(btnFind);
            searchPanel.Children.Add(reloadBtn);
            btnFind.Click += (sender, args) =>
            {
                botBrowser.LoadUrlAsync(searchBox.Text);
            };
            
            Grid.SetRow(searchPanel, 0);
            Grid.SetRow(botBrowser, 1);
            grid.Children.Add(searchPanel);
            grid.Children.Add(botBrowser);
            return grid;
        }

        protected async Task<BookmakerCommonInfo> GetBookmakerCredentials(
            Bookmaker bookmaker, 
            SettingsOptions settingsOptions)
        {
            var host = string.Empty;
            try
            {
                host = new Uri(bookmaker.Uri).Host;
            }
            catch
            {
                host = bookmaker.Uri;
            }
            var bookmakerInfo = settingsOptions.Bookmakers.First(x => x.Bookmaker.Contains(bookmaker.BookmakerName, StringComparison.OrdinalIgnoreCase));
            var bUrls = await WaitWebPageLoadedEvent(Browser, host, bookmakerInfo.LoadCount);
            
            return new (bookmaker.Uri, bookmaker.BrowserOptions.Auth, bUrls.ToArray(), host);
        }

        private async Task<List<string>> WaitWebPageLoadedEvent(BotBrowser botBrowser, string host, int skipCount)
        {
            //TODO:Почему было 0 секунд
            //var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0));
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(0));

            var tasks = Enumerable.Range(0, skipCount).Select(x => botBrowser.Worker.BrowserLoaded
                .Where(e => e.Contains(host, StringComparison.OrdinalIgnoreCase))
                .Skip(x)
                .FirstAsync().ToTask(cts.Token)).ToArray();
            
            var bUrls = new List<string>();
            try
            {
                foreach (var task in tasks)
                {
                    bUrls.Add(await task);
                }
            }
            catch (TaskCanceledException)
            {
                // ignore
            }

            return bUrls;
        }
        
        public bool BrowserMatchWithBet(BetMain bet) => Browser.BookmakerName.Contains(bet.Bookmaker[1..], StringComparison.OrdinalIgnoreCase);

        public abstract Task<BotBrowser> CreateBrowser(Bookmaker bookmaker, IPanelLogger logger, CancellationToken token);

        public abstract Task<bool> Login(Bookmaker bookmaker, CancellationToken token);
        
        public abstract Task<bool> LoadEvents(BetMain bet);
        
        public abstract Task<bool> FindStake(BetMain bet);

        public abstract Task<bool> SetStakeSum(decimal sum);

        public abstract Task<StakeCoefficientResult> DoStake();
        public abstract Task<StakeCoefficientResult> DoStakeWithLoginCheck();
        
        public virtual async Task<MatchData> GetMatchData(MatchDataParam param)
        {
            //TODO:В javascript нет этого метода для ben365
            return await Browser.Execute<MatchData>("parseMatchData");
        }

        public virtual async Task<BalanceInfo> GetBalanceInfo()
        {
            return await Browser.Execute<BalanceInfo>("parseBalance");
        }

        public virtual async Task<decimal> GetStakeCoefficientFromCoupon(BetMain bet)
        {
            var result = await Browser.Execute<double>("getCoefCurrentStake");
            return Convert.ToDecimal(result);
        }
        public abstract Task<decimal> GetPuttedCoefStake();

        public abstract Task<(decimal MinStake, decimal MaxStake)> GetMinMaxStake(BetMain bet);
        public virtual async Task<double> GetCoefficientWinStake(WinStakeCoefficientParams param)
        {
            var coefStake = await Browser.Execute<string>("getCoefWinStake", param.HomeParam);
            return Convert.ToDouble(coefStake);
        }

        public void CloseBrowser()
        {
            BrowserWindow?.Close();
            BrowserWindow = null;
            Browser?.Dispose();
        }

        public abstract Task<(double firstStakeCoef, double secondStakeCoef)> GetStakesCoefs(BetMain firstBet, BetMain secondBet);
        public abstract Task<bool> CheckGameInTennis(string eventId);
    }
}