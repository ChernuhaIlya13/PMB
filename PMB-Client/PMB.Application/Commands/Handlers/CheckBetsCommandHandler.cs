using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Models;
using PMB.Application.Queries.Handlers;
using PMB.Browsers.Common;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;

namespace PMB.Application.Commands.Handlers;

public record CheckBetsCommand(ForkMain Fork, List<IBotBrowserDecorator> Decorators,
    int NotOverlappedForksCount,
    decimal FirstBookmakerStavka,
    decimal SecondBookmakerStavka,
    ForkSettings Settings) : IRequest<CheckBetsCommandResult>;

public record CheckBetsCommandResult(bool Ok, decimal FirstBookmakerStavka, decimal SecondBookmakerStavka,
    int NotOverlappedForks);

[UsedImplicitly]
internal sealed class CheckBetsCommandHandler : IRequestHandler<CheckBetsCommand, CheckBetsCommandResult>
{
    private readonly IPanelLogger _panelLogger;
    private readonly IMediator _mediator;
    
    public CheckBetsCommandHandler(IPanelLogger panelLogger, IMediator mediator)
    {
        _panelLogger = panelLogger;
        _mediator = mediator;
    }

    public async Task<CheckBetsCommandResult> Handle(CheckBetsCommand request, CancellationToken token)
    {
        var fork = request.Fork;
        var decorators = request.Decorators;
        var notOverlappedForks = request.NotOverlappedForksCount;
        var settings = request.Settings;
        
        //Проверка BetId в вилке
        var (isValid, betsFromStash) = await _mediator.Send(new CheckBetIdAndGetBetsCommand(fork), token);

        if (!isValid)
        {
            await _panelLogger.AddInfoLog("Не пришёл идентификатор от сканера");
            return new (false, request.FirstBookmakerStavka, request.SecondBookmakerStavka, notOverlappedForks);
        }

        var betDecorators = await _mediator.Send(new BetDecoratorsQuery(betsFromStash, decorators.ToArray()), token);
        
        token.ThrowIfCancellationRequested();

        //Загружает события
        var eventsLoadedStatus = await _mediator.Send(new LoadEventsCommand(betDecorators.BetDecorators));
        
        await Task.Delay(2000, token);
        
        if (!eventsLoadedStatus)
        {
            return new(false, request.FirstBookmakerStavka, request.SecondBookmakerStavka, notOverlappedForks);
        }
        //Проверка на геймы в теннисе
        var checkForkByGamesInTennis = await _mediator.Send(new CheckForkBySportSettingsCommand(betDecorators.BetDecorators));
        if (!checkForkByGamesInTennis)
        {
            return new(false, request.FirstBookmakerStavka, request.SecondBookmakerStavka, notOverlappedForks);
        }

        //Проверка на время в вилке
        var validMinutes = await _mediator.Send(new ValidateTimeForkQuery(betDecorators.BetDecorators), token);

        switch (validMinutes)
        {
            case StatusParseMinutes.TimeNotMatch:
                await _panelLogger.AddInfoLog("Время ставок не сходится,отменяю вилку");
                return new (false, request.FirstBookmakerStavka, request.SecondBookmakerStavka, notOverlappedForks);
            case StatusParseMinutes.TimeMatch:
                await _panelLogger.AddInfoLog("Время ставок сходится");
                break;
            case StatusParseMinutes.None:
                break;
            case StatusParseMinutes.TimeFailedParse:
                break;
            case StatusParseMinutes.UnknownFail:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        //TODO:Разобраться как получать правильно Период,счёты,геймы,сеты в пинакле
        //var validPeriods = await _mediator.Send(new ValidatePeriodsForkQuery(betDecorators.BetDecorators.ToList()));

        token.ThrowIfCancellationRequested();
        var sumOfStakesCommandResult = await _mediator.Send(new SumOfStakesCommand(betDecorators.BetDecorators, request.FirstBookmakerStavka, request.SecondBookmakerStavka), token);
        
        // TODO:Раскоменить когда протещу Fonbet
        // foreach (var statusStake in sumOfStakesCommandResult.StakeStatuses)
        // {
        //     switch (statusStake)
        //     {
        //         case StakeSetStatus.UnhandledException:
        //         case StakeSetStatus.NotFoundStake:
        //         case StakeSetStatus.FailedSetStakeSum:
        //         case StakeSetStatus.BalanceTrouble:
        //         case StakeSetStatus.NotEnoughMoneyForCurrentStake:
        //         case StakeSetStatus.FailedGetMinMaxStake:
        //             return new (false, sumOfStakesCommandResult.FirstBookmakerStavka, sumOfStakesCommandResult.SecondBookmakerStavka, notOverlappedForks);
        //     }
        // }
        //Проверяет коэффициенты пары ставок для уточнения корректности вилки
        if (settings.CheckByCoefficientStakes)
        {
            var isValidConcreteCoefStake = await _mediator.Send(new CheckConcreteCoefStakeCommand(betDecorators.BetDecorators));
        
            if (!isValidConcreteCoefStake)
            {
                await _panelLogger.AddInfoLog("Проверка по коэффициенту ставки не прошла");
                return new(false, sumOfStakesCommandResult.FirstBookmakerStavka,
                    sumOfStakesCommandResult.SecondBookmakerStavka, notOverlappedForks);
            }
        }
        

        //betDecorators.BetDecorators.Reverse();//Меняем местами pinnacle и bet365
        
        token.ThrowIfCancellationRequested();

        var profitForkValid = await _mediator.Send(new CheckBetsCoefficientCommand(betDecorators.BetDecorators), token);
        if (!profitForkValid)
        {
            await _panelLogger.AddInfoLog("Прибыль вилки не подходит для текущих настроек");
            return new (false, sumOfStakesCommandResult.FirstBookmakerStavka, sumOfStakesCommandResult.SecondBookmakerStavka, notOverlappedForks);
        }
        
        var result = await _mediator.Send(new DoStakeCommand(betDecorators.BetDecorators, notOverlappedForks), token);

        return new(result.Ok, sumOfStakesCommandResult.FirstBookmakerStavka,
            sumOfStakesCommandResult.SecondBookmakerStavka, result.NotOverlappedForks);
    }
}