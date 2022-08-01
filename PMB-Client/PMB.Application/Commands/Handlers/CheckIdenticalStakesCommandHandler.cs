using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Interfaces;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;

namespace PMB.Application.Commands.Handlers;

public record CheckIdenticalStakesCommand(ForkMain Fork) : IRequest<bool>;

[UsedImplicitly]
internal sealed class CheckIdenticalStakesCommandHandler : IRequestHandler<CheckIdenticalStakesCommand,bool>
{
    private readonly ForkSettings _forkSettings;

    public CheckIdenticalStakesCommandHandler(ISettingsProvider settingsProvider)
    {
        _forkSettings = settingsProvider.GetSettings();
    }

    /// <summary>
    /// Возвращает true,когда прошла проверка успешно и вилку можно обрабатывать дальше по конвеееру
    /// Иначе false
    /// </summary>
    public Task<bool> Handle(CheckIdenticalStakesCommand request, CancellationToken cancellationToken)
    {
        var fork = request.Fork;
        if (fork == null)
            return Task.FromResult(false);
        var countIdenticalBets = _forkSettings.CountOfIdenticalSurebetsInOneEvent;
        
        var firstBetValue = fork.FirstBet.BetValue.Trim();
        var secondBetValue = fork.SecondBet.BetValue.Trim();
        
        var firstNameEvent = fork.FirstBet.EventName.Trim();
        var secondNameEvent = fork.SecondBet.EventName.Trim();

        var (firstNameEventTwin, secondNameEventTwin) = FilterCountStakes.TwinString(firstNameEvent, secondNameEvent);
        var (firstBetValueTwin, secondBetValueTwin) = FilterCountStakes.TwinString(firstBetValue, secondBetValue);
        
        var stakesFirst = FilterCountStakes.CountStakesInOneEvent.GetValueOrDefault(firstNameEventTwin,null);
        var stakesSecond = FilterCountStakes.CountStakesInOneEvent.GetValueOrDefault(secondNameEventTwin,null);

        var countStakes = 0;
        
        if (stakesFirst != null)
        {
            countStakes += stakesFirst.GetValueOrDefault(firstBetValueTwin, 0);
        }
        if (stakesSecond != null)
        {
            countStakes += stakesSecond.GetValueOrDefault(secondBetValueTwin, 0);
        }

        return Task.FromResult(countStakes < countIdenticalBets);
    }
}