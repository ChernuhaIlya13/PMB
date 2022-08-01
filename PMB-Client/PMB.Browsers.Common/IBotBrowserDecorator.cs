using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PMB.Cef.Core;
using PMB.Cef.Core.JsProxy;
using PMB.Domain;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;

namespace PMB.Browsers.Common
{
    public interface IBotBrowserDecorator
    {
        Window BrowserWindow { get; set; }
        BotBrowser Browser { get; set; }

        string BookmakerName { get; }

        Task<BotBrowser> CreateBrowser(Bookmaker bookmaker, IPanelLogger logger = null, CancellationToken token = default);

        Task<bool> Login(Bookmaker bookmaker, CancellationToken token);

        bool BrowserMatchWithBet(BetMain bet);
        
        Task<bool> LoadEvents(BetMain bet);

        Task<bool> FindStake(BetMain bet);
        
        Task<bool> SetStakeSum(decimal sum);

        Task<StakeCoefficientResult> DoStake();

        Task<StakeCoefficientResult> DoStakeWithLoginCheck();

        Task<MatchData> GetMatchData(MatchDataParam param);

        Task<BalanceInfo> GetBalanceInfo();

        Task<Decimal> GetStakeCoefficientFromCoupon(BetMain bet);

        Task<decimal> GetPuttedCoefStake();
        
        Task<(decimal MinStake, decimal MaxStake)> GetMinMaxStake(BetMain bet);

        Task<double> GetCoefficientWinStake(WinStakeCoefficientParams param);

        void CloseBrowser();
        
        Task<(double firstStakeCoef, double secondStakeCoef)> GetStakesCoefs(BetMain firstBet, BetMain secondBet);

        Task<bool> CheckGameInTennis(string eventId);
        //todo
    }
}