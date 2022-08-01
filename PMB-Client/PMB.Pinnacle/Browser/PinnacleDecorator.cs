using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CefSharp;
using Microsoft.Extensions.Options;
using PMB.Application.Interfaces;
using PMB.Browsers.Common;
using PMB.Browsers.Common.Exceptions;
using PMB.Cef.Core;
using PMB.Cef.Core.Extensions;
using PMB.Cef.Core.Handlers;
using PMB.Cef.Core.JsProxy;
using PMB.Domain;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;
using PMB.Pinnacle.Helpers;
using PMB.Pinnacle.Models;
using PMB.Utils;
using PriceInfo = PMB.Pinnacle.Models.PriceInfo;

namespace PMB.Pinnacle.Browser
{
    public class PinnacleDecorator: BotBrowserDecoratorBase
    {
        private PinnacleClient _client;
        private BetStraightModel _bet;
        private Guid _requestId;
        private const string PuttedStakeSuccess = "unsettled";
        private string _marketKey;
        private string _designation;
        private decimal _coefFromCoupon;
        private ISettingsProvider _settingsProvider; 
            
        public override string BookmakerName => "pinnacle";

        public PinnacleDecorator(IOptions<SettingsOptions> settingsOptions, JsLoader loader,
            ISettingsProvider settingsProvider) : base(settingsOptions.Value, loader)
        {
            _settingsProvider = settingsProvider;
        }
        
        private async Task<StraightMarketsResult[]> GetStraightMarkets(string evId)
        {
            StraightMarketsResult[] straightMarkets = Array.Empty<StraightMarketsResult>();
            try
            {
                straightMarkets = await _client.GetStraightMarkets(evId);//"1472824050"
            }
            catch
            {
                //ignore
            }

            return straightMarkets;
        }

        private void GetMarketKeyDesignation(string betId,string marketAndBetType)
        {
            var (marketKey, designation) = ("","");
            try
            {
                (marketKey, designation) = BetExtensions.ResolvePinnacleMarketKey(betId, marketAndBetType);
            }
            catch
            {
                //ignore
            }

            //_marketKey = marketKey.Replace("-","");
            _marketKey = marketKey;
            _designation = designation;
        }

        private SelectionModel GenerateSelectionModel(MatchupPricesModel model,string marketKey,float marketAndBetTypeParam)
        {
            var selectionModel = new SelectionModel
            {
                Designation = model.Price.Designation,
                Price = model.Price.Price,
                MarketKey = marketKey,
                MatchupId = model.Matchup,
            };
            
            return selectionModel;
        }

        public override async Task<BotBrowser> CreateBrowser(Bookmaker bookmaker, IPanelLogger logger,
            CancellationToken token)
        {
            if (!bookmaker.BookmakerName.Equals(BookmakerName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new BookmakerNotFoundException(bookmaker.BookmakerName);
            }

            var botBrowser = BotBrowserFactory.CreateBrowser(bookmaker, Loader, Factory);

            var proxy = bookmaker.BrowserOptions.Proxy;
            
            token.ThrowIfCancellationRequested();
            
            if (proxy.UseProxy)
            {
                botBrowser.RequestHandler = new RequestHandlerCustom(proxy.Login, proxy.Password);
                await botBrowser.SetProxy(bookmaker.BrowserOptions.Proxy.ToProxyString());
            }
            if(bookmaker.Uri != null && !bookmaker.Uri.Contains("/account/login"))
                botBrowser.Address += "/account/login";
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

        private static bool AuthInfoIsValid(AuthInfo auth)
        {
            return !string.IsNullOrEmpty(auth?.XSession)  && !string.IsNullOrEmpty(auth.XApiKey) && !string.IsNullOrEmpty(auth.XDeviceUuid);
        }


        private async Task<bool> HandleLogin(Bookmaker bookmaker, CancellationToken token)
        {
            var (_, authOptions, _, host) = await GetBookmakerCredentials(bookmaker, SettingsOptions);
            var logined = await Browser.Execute<bool>("isLogined");

            if (!logined)
            {
                await Task.WhenAny(new Task[]
                {
                    Browser.Worker.EasyLogin(authOptions.Login, authOptions.Password), 
                    Browser.Worker.BrowserLoaded
                        .Where(x => x.Contains(host, StringComparison.OrdinalIgnoreCase))
                        .FirstAsync().ToTask(token)
                });
            }
            
            for (var i = 0; i < 3; i++)
            {
                if (!logined)
                {
                    logined = await Browser.Execute<bool>("isLogined");
                    await Task.Delay(1000, token);
                }
                else
                {
                    break;
                }
            }

            return logined;
        }
        
        public override async Task<bool> Login(Bookmaker bookmaker, CancellationToken token)
        {
            var (uri, _, bUrls, _) = await GetBookmakerCredentials(bookmaker, SettingsOptions);
            var logined = false;

            logined = await Browser.Execute<bool>("isLogined");
            if (!logined)
            {
                if (uri.Contains(bUrls.FirstOrDefault() == null ? "" : bUrls.First()))
                {
                    for (var i = 0; i < 3; i++)
                    {
                        logined = await HandleLogin(bookmaker, token);
                        if (logined)
                        {
                            break;
                        }
                    }
                }
            }
            var authInfo = new AuthInfo();

            for (var i = 0; i < 10; i++)
            {
                authInfo = await Browser.Execute<AuthInfo>("getAuthInfo");

                if (!AuthInfoIsValid(authInfo))
                {
                    await Task.Delay(500, token);
                    logined = await HandleLogin(bookmaker, token);
                }
                else
                {
                    break;
                }
            }

            var proxy = bookmaker.BrowserOptions.Proxy;

            _client = new PinnacleClient(authInfo, new Proxy
            {
                IpAdress = proxy.IpAdress,//"109.107.189.113",
                Port = proxy.Port,//39393,
                UseProxy = proxy.UseProxy,
                NeedAuthProxy = proxy.NeedAuthProxy,
                Login = proxy.Login,//"log",
                Password = proxy.Password,//"pas"
            }, new Uri(bookmaker.Uri));
            
      
            return logined;
        }

        public override Task<bool> LoadEvents(BetMain bet) => Task.FromResult(true);

        public override async Task<bool> FindStake(BetMain bet)
        {
            _marketKey = "";
            _designation = "";
            _coefFromCoupon = 0m;
            _requestId = Guid.Empty;
            
            var straightMarkets =  await GetStraightMarkets(bet.EvId);

            GetMarketKeyDesignation(bet.BetId,bet.MarketAndBetType);
            
            var matchUpPrices = BetExtensions.ResolveMathupAndPrice(straightMarkets, _marketKey, _designation);
            if (matchUpPrices == null)
            {
                return false;
            }
            var stakeCoef = await GetStakeCoefficientFromCoupon(bet);

            _bet  = new BetStraightModel
            {
                Class = "Straight",
                Selections = new SelectionModel[]
                {
                    new()
                    {
                        Designation = matchUpPrices.Price.Designation,
                        Price = stakeCoef,
                        MarketKey = _marketKey,
                        MatchupId = matchUpPrices.Matchup
                    }
                },
                OddsFormat = "decimal",
                OriginTag = "1m",
                AcceptBetterPrice = true,
                AcceptBetterPrices = true,
            };
            
            return true;
        }

        public override Task<bool> SetStakeSum(decimal sum)
        {
            if (_bet == null)
                return Task.FromResult(false);

            _bet.Stake = sum;
            return Task.FromResult(true);
        }

        public override async Task<StakeCoefficientResult> DoStake()
        {
            if (_bet == null)
                return new (false, 0);
            var currentBalanceResponse = await _client.GetBalance();
            var currentBalance = currentBalanceResponse.Amount;
            BetStraightResult result = null;
            try
            {
                result = await _client.BetStraight(_bet);
            }
            catch (Exception ex)
            {
                // ignored
            }

            var coefficient = 0d;
            bool isStaked;

            if (result != null && result.RequestId != Guid.Empty)
            {
                _requestId = result.RequestId;
            }
            else
            {
                return new(false, (decimal) coefficient);
            }

            for (var i = 0; i < 20; i++)
            {
                var stakePutted = await _client.GetPuttedStakeInfo(_requestId);
                //rejected
                if (stakePutted.Status.Equals("rejected", StringComparison.InvariantCultureIgnoreCase))
                {
                    break;
                }
                if (stakePutted.Status.Equals("unsettled", StringComparison.InvariantCultureIgnoreCase))
                {
                    coefficient = stakePutted.Price;
                    break;
                }

                await Task.Delay(1000);
            }

            var newBalanceInfo = await _client.GetBalance();
            var newBalance = newBalanceInfo.Amount;
            isStaked = newBalance != currentBalance;

            _bet = null;
            return new (isStaked, (decimal)coefficient);
        }

        public override async  Task<StakeCoefficientResult> DoStakeWithLoginCheck()
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

        protected override Func<RequestContextSettings, string, Bookmaker, BotBrowser> Factory => 
            (settings, recaptcha, bookmaker) => new PinnacleBrowser(bookmaker, recaptcha, settings,_client);
        
        public override async Task<MatchData> GetMatchData(MatchDataParam param)
        {
            var infoWithTime = (await _client.GetRelatedMatchUps(param.eventId)).ToList();
            return new MatchData()
            {
                Time = new()
                {
                    AdditionalTime = "",
                    MainTime = infoWithTime.FirstOrDefault(i => i.State.Minutes != null)?.State.Minutes.ToString()
                },
                AdditionalData = ""
            };
            
        }

        public override async Task<BalanceInfo> GetBalanceInfo()
        {
            return await _client.GetBalance();
        }
        public override async Task<decimal> GetStakeCoefficientFromCoupon(BetMain bet)
        {
            if (_coefFromCoupon != default)
                return _coefFromCoupon;
            
            var straightMarkets =  await GetStraightMarkets(bet.EvId);

            var matchUpPrices = BetExtensions.ResolveMathupAndPrice(straightMarkets, _marketKey, _designation);
            if (matchUpPrices == null)
                return 0;

            var selectionModel = GenerateSelectionModel(matchUpPrices, _marketKey, bet.MarketAndBetTypeParam);
            
            var stakeInfo = await _client.GetStakeInfo(new StraightQuoteModel()
            {
                OddsFormat = "american",
                Selections = new []
                {
                    selectionModel
                }
            });
            decimal priceResult;
            decimal normalCoef;
        
            if (stakeInfo?.Selections.First()?.Price < 0)
            {
                priceResult = stakeInfo.Selections.First().Price * (-1);
                normalCoef = Math.Round(100 / priceResult + 1, 2);
            }
            else
            {
                priceResult = stakeInfo.Selections.First().Price;
                normalCoef = Math.Round(priceResult / 100 + 1, 2);
            }

            _coefFromCoupon = normalCoef;
            return _coefFromCoupon;
        }

        public override async Task<decimal> GetPuttedCoefStake()
        {
            var currentTime = DateTime.Now;
            
            while(true)
            {
                var result = await _client.GetPuttedStakeInfo(_requestId);
                if (result.Status == PuttedStakeSuccess)
                    return Convert.ToDecimal(result.Stake);

                if (DateTime.Now - currentTime > TimeSpan.FromSeconds(1.5))
                    break;
                await Task.Delay(50);
            }

            return 0;
        }

        public override async Task<(decimal MinStake, decimal MaxStake)> GetMinMaxStake(BetMain bet)
        {
            var straightMarkets =  await GetStraightMarkets(bet.EvId);
            
            var matchUpPrices = BetExtensions.ResolveMathupAndPrice(straightMarkets, _marketKey, _designation);
            if (matchUpPrices == null)
                return (0,0);

            var selectionModel = GenerateSelectionModel(matchUpPrices,_marketKey,bet.MarketAndBetTypeParam);
            
            var stakeInfo = await _client.GetStakeInfo(new StraightQuoteModel()
            {
                OddsFormat = "american",
                Selections = new []
                {
                    selectionModel
                }
            });
            var minStake = stakeInfo.Limits.FirstOrDefault(x => x.Type == "minRiskStake")?.Amount;
            var maxStake = stakeInfo.Limits.FirstOrDefault(x => x.Type == "maxRiskStake")?.Amount;

            if (minStake != null && maxStake != null)
            {
                return (minStake.Value, maxStake.Value);
            }

            if (minStake == null && maxStake != null)
            {
                return (0m, maxStake.Value);
            }
            return (0m, 0m);
        }

        public override async Task<double> GetCoefficientWinStake(WinStakeCoefficientParams param)
        {
            var straights = await GetStraightMarkets(param.EventId);
            var stake = straights?.FirstOrDefault(s => s.Type == "moneyline" && s.Key == "s;0;m");
            var designationParam = param.HomeParam ? "home" : "away";
            var price = stake?.Prices?.FirstOrDefault(p => p.Designation == designationParam)?.Price;
            decimal priceResult;
            decimal normalCoef;
        
            if (price != null && price.Value < 0)
            {
                priceResult = price.Value * (-1);
                normalCoef = Math.Round(100 / priceResult + 1, 2);
            }
            else
            {
                priceResult = price.Value;
                normalCoef = Math.Round(priceResult / 100 + 1, 2);
            }

            return Convert.ToDouble(normalCoef);
        }
        public override async Task<(double firstStakeCoef, double secondStakeCoef)> GetStakesCoefs(BetMain firstBet,BetMain secondBet)
        {
            var stakesMatching = new Dictionary<string, string>()
            {
                {
                    "победа",
                    "фора"
                }
            };
            var firstBetValue = firstBet.BetValue;
            var secondBetValue = secondBet.BetValue;

            var needFindStakeLikeInSecondBet = false;

            foreach (var keyValuePair in stakesMatching)
            {
                if ((firstBetValue.Contains(keyValuePair.Key, StringComparison.InvariantCultureIgnoreCase) &&
                    secondBetValue.Contains(keyValuePair.Value, StringComparison.InvariantCultureIgnoreCase)) ||
                    (firstBetValue.Contains(keyValuePair.Value, StringComparison.InvariantCultureIgnoreCase) &&
                    secondBetValue.Contains(keyValuePair.Key, StringComparison.InvariantCultureIgnoreCase)))
                {
                    needFindStakeLikeInSecondBet = true;
                    break;
                }
            }
            

            var straights = await GetStraightMarkets(firstBet.EvId);
            var priceIndex = 0;
            bool foundPriceIndex = false;
            for (var i = 0; i < straights.Length; ++i)
            {
                if(straights[i].Key == _marketKey && straights[i].Prices.FirstOrDefault(pr => pr.Designation.Equals(_designation)) != null)
                {
                    for (var j = 0; j < straights[i].Prices.Count; ++j)
                    {
                        if (straights[i].Prices[j].Designation == _designation)
                        {
                            priceIndex = j;
                            foundPriceIndex = true;
                            break;
                        }
                    }
                    if (foundPriceIndex)
                        break;
                }
            }

            if (!foundPriceIndex)
                return (0, 0);
            
            var straight = straights.FirstOrDefault(
                s => s.Key == _marketKey && s.Prices.Any(pr => pr.Designation.Equals(_designation)));
            if (straight != null)
            {
                var mainPrice = straight.Prices.FirstOrDefault(pr => pr.Designation == _designation);
                PriceInfo oppositeStake;
                decimal oppositeStakeCoef;
                decimal mainStakeCoef;
            
                if (straight.Prices.Count() - 1 > priceIndex)
                {
                    oppositeStake = straight.Prices[straight.Prices.Count() - 1];
                    oppositeStakeCoef = ConvertCoef(oppositeStake.Price);
                    mainStakeCoef = ConvertCoef(mainPrice!.Price);
                }
                else
                {
                    oppositeStake = mainPrice;
                    oppositeStakeCoef = ConvertCoef(oppositeStake!.Price);
                    mainStakeCoef = ConvertCoef(straight.Prices[0].Price);
                }
                return (Math.Round(Convert.ToSingle(mainStakeCoef),2),Math.Round(Convert.ToSingle(oppositeStakeCoef),2));
            }

            return (0, 0);
        }

        public override Task<bool> CheckGameInTennis(string eventId)
        {
            return Task.FromResult(true);
        }

        public static decimal ConvertCoef(int price)
        {
            decimal priceResult;
            decimal normalCoef;
            
            if (price < 0)
            {
                priceResult = price * (-1);
                normalCoef = Math.Round(100 / priceResult + 1, 2);
            }
            else
            {
                normalCoef = Math.Round(Convert.ToDecimal(price) / 100 + 1, 2);
            }

            return normalCoef;
        }
    }
}