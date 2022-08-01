using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PMB.Application.Interfaces;
using PMB.Browsers.Common;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;
using PMB.Wpf.Client.Infra;
using PMB.Wpf.Client.Models;
using PMB.Wpf.Client.Validate;

namespace PMB.Wpf.Client.Operations
{
    public class ForkBetOperations
    {
        private readonly IPanelLogger _customLogger;

        private readonly ISettingsProvider _filterProvider;

        private readonly Random _rnd = new();

        private readonly BrowserOperations _browserOperations;

        public ForkBetOperations(IPanelLogger logger, ISettingsProvider filterProvider,
            BrowserOperations browserOperations)
        {
            _customLogger = logger;
            _filterProvider = filterProvider;
            _browserOperations = browserOperations;
        }

        private async Task<bool> BetIdExists(BetMain first, BetMain second)
        {
            var betIdsExists = !string.IsNullOrWhiteSpace(first.BetId) && !string.IsNullOrWhiteSpace(second.BetId);
            if (!betIdsExists)
            {
                await _customLogger.AddInfoLog("Не нашёл BetId в одной из ставок");
                return false;
            }

            Debug.WriteLine("BetID из первой ставки " + first.BetId);
            Debug.WriteLine("BetID из второй ставки " + second.BetId);
            return true;
        }

        private static void BetUrlsChange(IEnumerable<BetMain> bets)
        {
            foreach (var t in bets.Where(t => t.Bookmaker.Contains("marathon") && t.Url != null))
            {
                t.Url = t.Url.Replace("com", "ru");
            }
        }

        private async Task LoadEventsInBrowsers(List<BetDecorator> betDecorators,
            IReadOnlyCollection<BetMain> bets)
        {
            var browsersLoaded = betDecorators.Select(async betDecorator =>
            {
                var browserLoadTasks = bets.Select(async bet =>
                {
                    var (_, botBrowserDecorator) = betDecorator;
                    if (botBrowserDecorator.BookmakerName.Contains(bet.Bookmaker[1..], StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            await botBrowserDecorator.LoadEvents(bet);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }).ToArray();

                await Task.WhenAll(browserLoadTasks);
            }).ToArray();

            await Task.WhenAll(browsersLoaded);
        }

        private async Task<List<BetDecorator>> GetDecoratorsForCurrentFork(
            List<IBotBrowserDecorator> decorators,
            IReadOnlyCollection<BetMain> bets)
        {
            var betDecoratorPairs = new List<BetDecorator>();

            decorators.ForEach(decorator =>
            {
                var bet = bets.FirstOrDefault(bet =>
                    decorator.BookmakerName.Contains(bet.Bookmaker[1..], StringComparison.OrdinalIgnoreCase));

                if (bet != null)
                {
                    betDecoratorPairs.Add(new (bet, decorator));
                }
            });
            
            if (betDecoratorPairs.Count < 2)
            {
                await _customLogger.AddInfoLog("2 Букмекера минимум должны быть активны.Поменяйте настройки");
                return null;
            }

            return betDecoratorPairs;
        }

        public async Task CheckBets(ForkMain fork, List<IBotBrowserDecorator> decorators)
        {
            var settings = _filterProvider.GetSettings();

            var betsFromStash = new List<BetMain>
            {
                fork.FirstBet,
                fork.SecondBet
            };

            var betIdResult = await BetIdExists(betsFromStash.First(), betsFromStash.Last());
            if (!betIdResult)
                return;

            BetUrlsChange(betsFromStash);

            var betDecorators = await GetDecoratorsForCurrentFork(decorators, betsFromStash);
            if (betDecorators?.Any() != true)
                return;
            
            //betDecorators.Reverse();

            var bookmakerNames = betDecorators.Select(x => x.Decorator.BookmakerName).ToArray();

            var restrictions = settings.SequenceRulesBookmakers.Where(x =>
                bookmakerNames.Contains(x.First) && bookmakerNames.Contains(x.Second));

            foreach (var restriction in restrictions)
            {
                var first = betDecorators.First(x =>
                    x.Decorator.BookmakerName.Equals(restriction.First, StringComparison.InvariantCultureIgnoreCase));

                var second = betDecorators.First(x =>
                    x.Decorator.BookmakerName.Equals(restriction.Second, StringComparison.InvariantCultureIgnoreCase));

                betDecorators = new List<BetDecorator>
                {
                    first,second
                };
            }

            var firstParamsDecorator = new ParamsDecorator(betDecorators.First().Decorator,
                betsFromStash.FirstOrDefault(b => b.Bookmaker.ToLower() == betDecorators.First().Decorator.BookmakerName.ToLower())?.EvId);
            var secondParamsDecorator = new ParamsDecorator(betDecorators.Skip(1).First().Decorator,
                betsFromStash.FirstOrDefault(b => b.Bookmaker.ToLower() == betDecorators.Skip(1).First().Decorator.BookmakerName.ToLower())?.EvId);
            
            await LoadEventsInBrowsers(betDecorators, betsFromStash);

            await Task.Delay(2000);

            var validMinutes = await ValidateFork.ValidateMinutes(firstParamsDecorator,secondParamsDecorator);

            switch (validMinutes)
            {
                case StatusParseMinutes.TimeNotMatch:
                    await _customLogger.AddInfoLog("Время ставок не сходится,илья проверь");
                    break; 
                case StatusParseMinutes.TimeFailedParse:
                    await _customLogger.AddInfoLog("Не удалось распарсить время,илья проверь");
                    break;
                case StatusParseMinutes.UnknownFail:
                    await _customLogger.AddInfoLog("Неизвестная ошибка при парсинге времени,илья проверь");
                    break;
                case StatusParseMinutes.TimeMatch:
                    await _customLogger.AddInfoLog("Вилка прошла проверку на время,илья проверь");
                    break;
            }
            await _customLogger.AddInfoLog("Вилка прошла проверку на время");

            var commonSumOfStakes = _rnd.Next(settings.CleverStake.Start, settings.CleverStake.Finish);
            var sumOfStakes = CalculateSumOfStakes(betDecorators.First().BetMain, betDecorators.Last().BetMain, commonSumOfStakes);

            var browsersFinishedDoStakes = betDecorators.Select(async betDecorator =>
            {
                var betsTasks = betsFromStash
                    .Where(betDecorator.Decorator.BrowserMatchWithBet)
                    .Select(async bet =>
                    {
                        var bookmakerName = betDecorator.Decorator.BookmakerName;
                        var founded = false;
                        try
                        {
                            founded = await betDecorator.Decorator.FindStake(bet);
                        }
                        catch
                        {
                            founded = false;
                        }
                        if (!founded)
                            return new ResponseFromBrowser
                            {
                                StatusStake = ResponseStatusStake.NotFoundStake, BrowserName = bookmakerName
                            };

                        bool stakeSet;
                        try
                        {
                             stakeSet = await betDecorator.Decorator.SetStakeSum(sumOfStakes[bet.Bookmaker]);
                        }
                        catch
                        {
                            stakeSet = false;
                        }
                        
                        if (!stakeSet)
                            return new ResponseFromBrowser
                            {
                                StatusStake = ResponseStatusStake.FailedSetStakeSum,
                                BrowserName = bookmakerName
                            };
                        
                        return new ResponseFromBrowser { StatusStake = ResponseStatusStake.Success };
                    }).ToArray();

                return await betsTasks.First();
            }).ToArray();

            var firstBrowserResult =
                _browserOperations.ProcessBrowserTask(await browsersFinishedDoStakes.First());
            if (!firstBrowserResult)
                return;

            var secondBrowserResult =
                _browserOperations.ProcessBrowserTask(await browsersFinishedDoStakes.Skip(1).First());
            if (!secondBrowserResult)
                return;

            foreach (var (_, botBrowserDecorator) in betDecorators)
            {
                try
                {
                    var isStaked = await botBrowserDecorator.DoStake();
                    if (!isStaked.IsStaked)
                    {
                        await _customLogger.AddInfoLog(
                            $"Не удалось проставить ставку на {botBrowserDecorator.BookmakerName}");
                        break;
                    }

                    await _customLogger.AddInfoLog($"Ставка успешно проставлена на {botBrowserDecorator.BookmakerName}");
                }
                catch (Exception)
                {
                    await _customLogger.AddInfoLog(
                        $"Не удалось проставить ставку на {botBrowserDecorator.BookmakerName}");
                    break;
                }
            }
            await _customLogger.AddInfoLog("Закончил обрабатывать вилку");
        }

        private Dictionary<string, decimal> CalculateSumOfStakes(BetMain bet1, BetMain bet2, decimal sumStake)
        {
            var sum1 = 1 / bet1.Coefficient / (1 / bet1.Coefficient + 1 / bet2.Coefficient) * sumStake;
            var sum2 = 1 / bet2.Coefficient / (1 / bet1.Coefficient + 1 / bet2.Coefficient) * sumStake;

            return new Dictionary<string, decimal>
            {
                {bet1.Bookmaker, ToSecondDecimals(sum1)},
                {bet2.Bookmaker, ToSecondDecimals(sum2)}
            };
        }

        private decimal ToSecondDecimals(decimal d) => Math.Round(d, 2);
    }
}