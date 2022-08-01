using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Interfaces;
using PMB.Application.Models;
using PMB.Application.Queries.Handlers;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;

namespace PMB.Application.Commands.Handlers;

public record SumOfStakesCommand(BetDecorator[] BetDecorators, decimal FirstBookmakerStavka,
    decimal SecondBookmakerStavka) : IRequest<SumOfStakesCommandResult>;

public record SumOfStakesCommandResult(decimal FirstBookmakerStavka, decimal SecondBookmakerStavka, StakeSetStatus[] StakeStatuses);

[UsedImplicitly]
internal sealed class SumOfStakesCommandHandler : IRequestHandler<SumOfStakesCommand, SumOfStakesCommandResult>
{
    private readonly Random _rnd = new();
    private readonly IPanelLogger _panelLogger;
    private readonly IMediator _mediator;
    private readonly ForkSettings _forkSettings;
    
    public SumOfStakesCommandHandler(IPanelLogger panelLogger,IMediator mediator, ISettingsProvider settingsProvider)
    {
        _panelLogger = panelLogger;
        _mediator = mediator;
        _forkSettings = settingsProvider.GetSettings();
    }

    public async Task<SumOfStakesCommandResult> Handle(SumOfStakesCommand request, CancellationToken cancellationToken)
    {
        //TODO:Проверить,как здесь отрабатывает,не совсем верное условие
        //if (request.BetDecorators.Length > 2 || request.BetDecorators.Length == 0)
        if (request.BetDecorators.Length < 2)
        {
            await _panelLogger.AddInfoLog("Для текущей вилки нет активных букмекеров");
            return new SumOfStakesCommandResult(request.FirstBookmakerStavka, request.SecondBookmakerStavka, new[]{ StakeSetStatus.UnhandledException});
        } 
        
        var commonSumOfStakes = _rnd.Next(_forkSettings.CleverStake.Start, _forkSettings.CleverStake.Finish);

        var firstBet = request.BetDecorators.First().BetMain;
        var secondBet = request.BetDecorators.Skip(1).First().BetMain;
        
        var sumOfStakes = CalculateSumOfStakes(firstBet, secondBet, commonSumOfStakes);
        
        var firstBetDecoratorSumma = new BetDecoratorSumma(sumOfStakes[firstBet.Bookmaker].ToString(CultureInfo.CurrentCulture),request.BetDecorators.First());
        var secondBetDecoratorSumma = new BetDecoratorSumma(sumOfStakes[secondBet.Bookmaker].ToString(CultureInfo.CurrentCulture), request.BetDecorators.Skip(1).First());
        
        var betDecoratorsSumma = new List<BetDecoratorSumma>()
        {
            firstBetDecoratorSumma,
            secondBetDecoratorSumma
        };

        var balanceResult = await _mediator.Send(new ValidateBalanceForkQuery(betDecoratorsSumma.ToArray()));
        
        //TODO:Посмотреть как работает этот кусок кода
        // var firstBookmakerStavka = sumOfStakes.GetValueOrDefault("Pinnacle", 0);
        // var secondBookmakerStavka = sumOfStakes.GetValueOrDefault("Bet365", 0);
        var firstBookmakerStavka = sumOfStakes.GetValueOrDefault(firstBet.Bookmaker, 0);
        var secondBookmakerStavka = sumOfStakes.GetValueOrDefault(secondBet.Bookmaker, 0);

        switch (balanceResult.StatusBalance)
        {
            case StatusParseBalance.FailedParseBalance:
                await _panelLogger.AddInfoLog($"Не смог получить баланс на одной из контор");
                break;
            case StatusParseBalance.NotEnoughMoney:
                await _panelLogger.AddInfoLog("Проверьте баланс на конторах");
                break;
        }
        //TODO:Раскоментить ,так как тестил Fonbet - Bet365
        // if (balanceResult.StatusBalance != StatusParseBalance.Success)
        // {
        //     return new SumOfStakesCommandResult(firstBookmakerStavka, secondBookmakerStavka, new[]{ StakeSetStatus.BalanceTrouble});
        // }

        var browsersFinishedDoStakes = request.BetDecorators.Select(async betDecorator =>
        {
            var bet = betDecorator.BetMain;
            var bookmakerName = betDecorator.Decorator.BookmakerName;
            var founded = false;
            try
            {
                founded = await betDecorator.Decorator.FindStake(bet);
            }
            catch
            {
                // ignored
            }

            if (!founded)
            {
                await _panelLogger.AddInfoLog($"Ставка была не найдена в {bookmakerName}");
                return StakeSetStatus.NotFoundStake;
            }

            (decimal MinStake,decimal MaxStake) minMaxStakeResult = (0m, 0m);
            try
            {
                minMaxStakeResult = await betDecorator.Decorator.GetMinMaxStake(bet);
            }
            catch
            {
                await _panelLogger.AddInfoLog("Не смог получить макс/мин ставку");
                return StakeSetStatus.FailedGetMinMaxStake;
            }

            var sum = balanceResult.BetBalances?.FirstOrDefault(b =>
                b.BookmakerName.Equals(bet.Bookmaker,StringComparison.InvariantCultureIgnoreCase))!.SumOfStake;
            
            if (minMaxStakeResult.MinStake > sum)
            {
                await _panelLogger.AddInfoLog("Проверьте настройки умной проставки");
                return StakeSetStatus.NotEnoughMoneyForCurrentStake;
            }

            var stakeSet = false;
            stakeSet = await betDecorator.Decorator.SetStakeSum(betDecorator.Decorator.BookmakerName == "bet365" ? 1m : 55);
            // try
            // {
            //     if (balanceResult.BetBalances != null)
            //         stakeSet = await betDecorator.Decorator.SetStakeSum(balanceResult.BetBalances.FirstOrDefault(b => b.BookmakerName == betDecorator.Decorator.BookmakerName)!.SumOfStake);
            // }
            // catch
            // {
            //     stakeSet = false;
            // }
            if (!stakeSet)
            {
                await _panelLogger.AddInfoLog($"Не удалось установить сумму ставки в {bookmakerName}");
                return StakeSetStatus.FailedSetStakeSum;
            }
            
            return StakeSetStatus.Success;
        }).ToArray();

        return new SumOfStakesCommandResult(firstBookmakerStavka, secondBookmakerStavka, await Task.WhenAll(browsersFinishedDoStakes));
    }
    
    private static Dictionary<string, decimal> CalculateSumOfStakes(BetMain bet1, BetMain bet2, decimal sumStake)
    {
        var sum1 = 1 / bet1.Coefficient / (1 / bet1.Coefficient + 1 / bet2.Coefficient) * sumStake;
        var sum2 = 1 / bet2.Coefficient / (1 / bet1.Coefficient + 1 / bet2.Coefficient) * sumStake;

        return new Dictionary<string, decimal>
        {
            { bet1.Bookmaker, ToSecondDecimals(sum1) },
            { bet2.Bookmaker, ToSecondDecimals(sum2) }
        };
    }
    
    private static decimal ToSecondDecimals(decimal d) => Math.Round(d, 2);
}