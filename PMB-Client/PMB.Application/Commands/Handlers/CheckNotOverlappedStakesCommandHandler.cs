using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Interfaces;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;

namespace PMB.Application.Commands.Handlers;

public record CheckNotOverlappedStakesCommand(ForkMain Fork) : IRequest<bool>;

[UsedImplicitly]
internal sealed class CheckNotOverlappedStakesCommandHandler : IRequestHandler<CheckNotOverlappedStakesCommand,bool>
{
    private readonly ForkSettings _forkSettings;

    public CheckNotOverlappedStakesCommandHandler(ISettingsProvider settingsProvider)
    {
        _forkSettings = settingsProvider.GetSettings();
    }

    public Task<bool> Handle(CheckNotOverlappedStakesCommand request, CancellationToken cancellationToken)
    {
        var fork = request.Fork;
        if (fork == null)
            return Task.FromResult(false);
        var firstNameEvent = request.Fork.FirstBet.EventName;
        var secondNameEvent = request.Fork.SecondBet.EventName;
        var (firstNameEventTwin,secondNameEventTwin) = FilterCountStakes.TwinString(firstNameEvent, secondNameEvent);

        //Подсчитывает количество ставок для текущего события
        var countStakes = FilterCountStakes.CountNotOverlappedStakes.GetValueOrDefault(firstNameEventTwin, 0) 
                          + FilterCountStakes.CountNotOverlappedStakes.GetValueOrDefault(secondNameEventTwin, 0);

        return Task.FromResult(countStakes < _forkSettings.CountAdmissibleNonOverlappedInOneEvent);
    }
}