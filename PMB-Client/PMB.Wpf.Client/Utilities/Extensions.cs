using Microsoft.Extensions.DependencyInjection;
using PMB.Models.Messages;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows;
using CefSharp;
using PMB.Abb.Models.Models;
using PMB.Cef.Core;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Models.PositiveModels;
using PMB.Wpf.Client.Infra;
using Bookmaker = PMB.Domain.BrowserModels.Bookmaker;
using Range = PMB.Domain.ForkModels.Range;

namespace PMB.Wpf.Client.Utilities
{
    public static class Extensions
    {
        public static IServiceProvider Show<T>(this IServiceProvider provider) where T : Window
        {
            provider.GetRequiredService<T>().Show();
            return provider;
        }

        public static IServiceProvider Close<T>(this IServiceProvider provider) where T : Window
        {
            provider.GetRequiredService<T>().Close();
            return provider;
        }

        public static ForksFilterMessage ConvertToForksFilterMessage(this ForkSettings value)
        {
            return new ForksFilterMessage
            {
                Bookmakers = value.Bookmakers.Select(b => b.ConvertToBookmaker()).ToList(),
                CleverStake = new ForksFilterMessage.SmartSpacer(value.CleverStake.Start, value.CleverStake.Finish, value.CleverStake.PercentLimitOnStake),
                Coefficient = new ForksFilterMessage.Range(value.Coefficient.Start, value.Coefficient.Finish),
                CountAdmissibleNonOverlappedInOneEvent = value.CountAdmissibleNonOverlappedInOneEvent,
                CountForksInOneEvent = value.CountForksInOneEvent,
                CountOfIdenticalSurebetsInOneEvent = value.CountOfIdenticalSurebetsInOneEvent,
                MaxMinus = value.MaxMinus,
                MaxSumStakeInBk = value.MaxSumStakeInBk,
                PauseAfterSuccessfulAttemptPutDown = value.PauseAfterSuccessfulAttemptPutDown,
                PauseAfterUnsuccessfulAttemptPutDown = value.PauseAfterUnsuccessfulAttemptPutDown,
                Profit = new ForksFilterMessage.Range(value.Profit.Start, value.Profit.Finish),
                SequenceRulesBookmakers = value.SequenceRulesBookmakers.Select(b => b.ConvertToSequenceBookmaker()).ToList(),
                TimeOfLife = new ForksFilterMessage.Range(value.TimeOfLife.Start,value.TimeOfLife.Finish),
                WaitingShoulderOverlap = value.WaitingShoulderOverlap
            };
        }

        private static ForksFilterMessage.SequenceBookmakers ConvertToSequenceBookmaker(this ForkSettings.SequenceBookmakers value) =>
            new(value.First, value.Second, value.Id);

        private static PMB.Models.BookmakersFilters.Bookmaker ConvertToBookmaker(this Bookmaker value)
        {
            return new PMB.Models.BookmakersFilters.Bookmaker
            {
                BookmakerName = value.BookmakerName,
                BrowserOptions = value.BrowserOptions.ConvertToBrowserOptions(),
                CheckMaxMin = value.CheckMaxMin,
                CleverSlowing = value.CleverSlowing,
                CoefficientFork = value.CoefficientFork.ConvertToCoefficientFork(),
                CountForksInOneEvent = value.CountForksInOneEvent.ConvertToCountForksInOneEvent(),
                CountOfIdenticalSurebetsInOneEvent = value.CountOfIdenticalSurebetsInOneEvent.ConvertToCountOfIdenticalSurebetsInOneEvent(),
                Currency = value.CurrencyInfo.ConvertToCurrency(),
                IsActive = value.IsActive,
                PauseAfterSuccessfulAttemptPutDown = value.PauseAfterSuccessfulAttemptPutDown.ConvertToPauseAfterSuccessfulAttemptPutDown(),
                ProfitFork = value.ProfitFork.ConvertToProfitFork(),
                RestrictInitiator = value.RestrictInitiator,
                RestrictSum = value.RestrictSum.ConvertToRestrictSum(),
                StopBotBalance = value.StopBotBalance.ConvertToStopBalance(),
                TimeOfLifeFork = value.TimeOfLifeFork.ConvertToTimeOfLife(),
                Uri = value.Uri
            };
        }

        private static PMB.Models.BookmakersFilters.BrowserOptions ConvertToBrowserOptions(this BrowserOptions value)
        {
            return new PMB.Models.BookmakersFilters.BrowserOptions()
            {
                Auth = value.Auth.ConvertToAuthBookmaker(),
                Proxy = value.Proxy.ConvertToProxy(),
                SaveCacheInMemory = value.SaveCacheInMemory
            };
        }

        private static PMB.Models.BookmakersFilters.AuthBookmaker ConvertToAuthBookmaker(this AuthBookmaker value)
        {
            return new PMB.Models.BookmakersFilters.AuthBookmaker()
            {
                Login = value.Login,
                Password = value.Password,
                ShowImages = value.ShowImages,
                UseAuthInBk = value.UseAuthInBk
            };
        }

        private static PMB.Models.BookmakersFilters.Proxy ConvertToProxy(this Proxy value)
        {
            return new PMB.Models.BookmakersFilters.Proxy()
            {
                IpAdress = value.IpAdress,
                Login = value.Login,
                NeedAuthProxy = value.NeedAuthProxy,
                Password = value.Password,
                Port = value.Port,
                Schema = value.Schema.ConvertToSchemes(),
                UseProxy = value.UseProxy
            };
        }

        private static PMB.Models.BookmakersFilters.Schemes ConvertToSchemes(this Schemes value) =>
            Enum.TryParse<PMB.Models.BookmakersFilters.Schemes>(value.ToString(),out var result) ? result : PMB.Models.BookmakersFilters.Schemes.Http;

        private static (int Start,int Finish, bool UseCommonRuleRound) ConvertToCoefficientFork(this Bookmaker.ForkInfo value) =>
            (value.Start, value.Finish, value.UseCommonRuleRound);

        private static (int CountForks,bool UseCommonRuleRound) ConvertToCountForksInOneEvent(this Bookmaker.ForksDetails value) =>
            (value.CountForks, value.UseCommonRuleRound);

        private static (int CountForks, bool UseCommonRuleRound) ConvertToCountOfIdenticalSurebetsInOneEvent(this  Bookmaker.ForksDetails value) =>
            (value.CountForks, value.UseCommonRuleRound);

        private static (string currency,string RoundRule,bool UseCommonRuleRound) ConvertToCurrency(this Bookmaker.Currency value) =>
            (value.CurrencyName, value.RoundRule, value.UseCommonRuleRound);

        private static (int Start, int Finish, bool UseCommonRuleRound) ConvertToPauseAfterSuccessfulAttemptPutDown(this Bookmaker.ForkInfo value) =>
            (value.Start, value.Finish, value.UseCommonRuleRound);

        private static (int Start, int Finish, bool UseCommonRuleRound) ConvertToProfitFork(this Bookmaker.ForkInfo value) =>
            (value.Start, value.Finish, value.UseCommonRuleRound);

        private static (int PutStakeLess, int PutStakeMore, bool UseCommonRuleRound) ConvertToRestrictSum(this Bookmaker.ForkInfo value) =>
            (value.Start, value.Finish, value.UseCommonRuleRound);

        private static (int Start, int Finish) ConvertToStopBalance(this Range value) =>
            (value.Start, value.Finish);

        private static (int Start, int Finish, bool UseCommonRuleRound) ConvertToTimeOfLife(this Bookmaker.ForkInfo value) =>
            (value.Start, value.Finish, value.UseCommonRuleRound);

        public static EventHandler<LoadingStateChangedEventArgs> OnBrowserLoading(BotBrowser browser,Subject<string> redirectedOnUrls)
        {
            return delegate(object o, LoadingStateChangedEventArgs args)
            {
                if (!args.IsLoading)
                {
                    redirectedOnUrls.OnNext(browser.BookmakerName);
                }
            }; 
        }

        public static string Convert(this ResponseStatusStake status)
        {
            return status switch
            {
                ResponseStatusStake.NotFoundStake => "Ставка была не найдена в",
                ResponseStatusStake.FailedSetStakeSum => "Не удалось установить сумму ставки в",
                _ => string.Empty
            };
        }

        private static bool BrowserMatchWithBet(this BotBrowser browser, BetMain bet) =>
            browser.BookmakerName.Contains(bet.Bookmaker[1..], StringComparison.OrdinalIgnoreCase);
        
        public static ForkMain Convert(this Fork value)
        {
            return new ForkMain
            {
                Elid = value.Elid,
                Id = value.Id.ToString(),
                K1 = value.K1,
                K2 = value.K2,
                Lifetime = (int)value.Lifetime.TotalSeconds,
                Other = value.Other,
                Profit = value.Profit,
                Sport = value.Sport.ToString(),
                FirstBet = new BetMain()
                {
                    Id = value.FirstBet.Id.ToString(),
                    Bookmaker = value.FirstBet.Bookmaker.ToString(),
                    Coefficient = value.FirstBet.Coefficient,
                    Direction = value.FirstBet.Direction,
                    Parameter = value.FirstBet.Parameter,
                    Sport = value.FirstBet.Sport.ToString(),
                    Teams = value.FirstBet.Teams,
                    Url = value.FirstBet.Url,
                    BetId = value.FirstBet.BetId,
                    BetType = value.FirstBet.BetType.ToString(),
                    BetValue = value.FirstBet.BetValue,
                    EvId = value.FirstBet.EvId,
                    ForksCount = value.FirstBet.ForksCount,
                    IsInitiator = value.FirstBet.IsInitiator,
                    IsReq = value.FirstBet.IsReq,
                    MatchData = new BetMain.MatchDataInfo()
                    {
                        Liga = value.FirstBet.MatchData.Liga,
                        Score = value.FirstBet.MatchData.Score,
                        Time = value.FirstBet.MatchData.Time,
                        AdditionalData = value.FirstBet.MatchData.AdditionalData,
                        PreviousScores = value.FirstBet.MatchData.PreviousScores 
                    },
                    OtherData = value.FirstBet.OtherData,
                    PositiveEvId = value.FirstBet.PositiveEvId
                },
                SecondBet = new BetMain()
                {
                    Id = value.SecondBet.Id.ToString(),
                    Bookmaker = value.SecondBet.Bookmaker.ToString(),
                    Coefficient = value.SecondBet.Coefficient,
                    Direction = value.SecondBet.Direction,
                    Parameter = value.SecondBet.Parameter,
                    Sport = value.SecondBet.Sport.ToString(),
                    Teams = value.SecondBet.Teams,
                    Url = value.SecondBet.Url,
                    BetId = value.SecondBet.BetId,
                    BetType = value.SecondBet.BetType.ToString(),
                    BetValue = value.SecondBet.BetValue,
                    EvId = value.SecondBet.EvId,
                    ForksCount = value.SecondBet.ForksCount,
                    IsInitiator = value.SecondBet.IsInitiator,
                    IsReq = value.SecondBet.IsReq,
                    MatchData = new BetMain.MatchDataInfo()
                    {
                        Liga = value.SecondBet.MatchData.Liga,
                        Time = value.SecondBet.MatchData.Time,
                        AdditionalData = value.SecondBet.MatchData.AdditionalData,
                        PreviousScores = value.SecondBet.MatchData.PreviousScores 
                    },
                    OtherData = value.SecondBet.OtherData,
                    PositiveEvId = value.SecondBet.PositiveEvId
                },
                ForkId = value.ForkId,
                ForkScanner = ForkScannerType.Positivebet,
                UpdateCount = value.UpdateCount,
            };
        }

        private static (string score, string[] previousScores) ParseMainScoreAndPreviousScores(string currentScore)
        {
            if (string.IsNullOrWhiteSpace(currentScore))
            {
                return (string.Empty, Array.Empty<string>());
            }
            
            string mainScoreFirst;
            string[] previousScores;
            if (currentScore.Contains("("))
            {
                mainScoreFirst = currentScore[..currentScore.IndexOf("(",StringComparison.InvariantCultureIgnoreCase)];
                previousScores = currentScore[currentScore.IndexOf("(", StringComparison.InvariantCultureIgnoreCase)..]
                    .Replace(")","").Split(" ");
            }
            else
            {
                mainScoreFirst = currentScore;
                previousScores = Array.Empty<string>();
            }
            return (mainScoreFirst, previousScores);
        }
        
        public static ForkMain Convert(this AbbFork value)
        {
            var sportStr = Enum.TryParse<AbbSport>(value.AbbDto.SportId.ToString(), out var sport) ? sport.ToString() : AbbSport.None.ToString();
            var (scoreFBet, previousScoresFBet) = ParseMainScoreAndPreviousScores(value.Bets[0].CurrentScore);
            var (scoreSBet, previousScoresSBet) = ParseMainScoreAndPreviousScores(value.Bets[1].CurrentScore);
            
            var betValueFirst = AbbBetValue.BetValues[value.Bets[0].MarketAndBetType];
            var betValueSecond = AbbBetValue.BetValues[value.Bets[1].MarketAndBetType];
            return new ForkMain
            {
                Id = value.AbbDto.Id,
                Lifetime = (int)(DateTimeOffset.Now - value.AbbDto.CreatedAt).TotalSeconds,
                Profit = (decimal)value.AbbDto.Percent,
                Sport = sportStr,
                SportId = value.AbbDto.SportId,
                FirstBet = new BetMain()
                {
                    Lifetime = (int)(DateTimeOffset.Now - value.AbbDto.CreatedAt).TotalSeconds,
                    Bookmaker = Enum.TryParse<AbbBookmaker>(value.Bets[0].BookmakerId.ToString(), out var bookmakerFirst) ? bookmakerFirst.ToString() : AbbBookmaker.None.ToString(),
                    Coefficient = (decimal)value.Bets[0].Coefficient,
                    Direction = value.Bets[0].Diff == 0 ? Direction.Freeze : (int)value.Bets[0].Diff == 1 ? Direction.Up : Direction.Down,
                    Id = value.Bets[0].Id,
                    Sport = sportStr,
                    Teams = value.Bets[0].Home + "|" + value.Bets[0].Away,
                    BetId = value.Bets[0].DirectLink,
                    BetValue = betValueFirst,
                    BetType = string.Empty.ConvertToBetType(),
                    Period = AbbPeriod.Periods[value.Bets[0].PeriodId],
                    EvId = value.Bets[0].BookmakerEventDirectLink,
                    MatchData = new BetMain.MatchDataInfo
                    {
                        Liga = value.Bets[0].League,
                        Score = scoreFBet,
                        Time = string.Empty,
                        AdditionalData = string.Empty,
                        PreviousScores = previousScoresFBet 
                    },
                    Url = value.Bets[0].BookmakerEventDirectLink,
                    MarketAndBetType = value.Bets[0].MarketAndBetType.ToString(),
                    SportId = value.AbbDto.SportId,
                    SwapTeams = value.Bets[0].SwapTeams
                },
                SecondBet = new BetMain
                {
                    Lifetime = (int)(DateTimeOffset.Now - value.AbbDto.CreatedAt).TotalSeconds,
                    Bookmaker = Enum.TryParse<AbbBookmaker>(value.Bets[1].BookmakerId.ToString(), out var bookmakerSecond) ? bookmakerSecond.ToString() : AbbBookmaker.None.ToString(),
                    Coefficient = (decimal)value.Bets[1].Coefficient,
                    Direction = value.Bets[1].Diff == 0 ? Direction.Freeze : (int)value.Bets[1].Diff == 1 ? Direction.Up : Direction.Down,
                    Id = value.Bets[1].Id,
                    Sport = sportStr,
                    Teams = value.Bets[1].Home + "|" + value.Bets[1].Away,
                    BetId = value.Bets[1].DirectLink,
                    BetValue = betValueSecond,
                    Period = AbbPeriod.Periods[value.Bets[1].PeriodId],
                    EvId = value.Bets[1].BookmakerEventDirectLink,
                    BetType = string.Empty.ConvertToBetType(),
                    MatchData = new BetMain.MatchDataInfo
                    {
                        Liga = value.Bets[1].League,
                        Score = scoreSBet,
                        Time = string.Empty,
                        AdditionalData = string.Empty,
                        PreviousScores = previousScoresSBet
                    },
                    Url = value.Bets[1].BookmakerEventDirectLink,
                    MarketAndBetType = value.Bets[1].MarketAndBetType.ToString(),
                    SportId = value.AbbDto.SportId,
                    SwapTeams = value.Bets[1].SwapTeams
                },
                ForkScanner = ForkScannerType.AllBestBets,
            };
        }
    }
}
