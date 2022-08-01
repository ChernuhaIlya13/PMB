using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Interfaces;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;

namespace PMB.Application.Commands.Handlers;

public record CheckCountForksInOneEventCommand(ForkMain Fork) : IRequest<bool>;

[UsedImplicitly]
internal sealed class CheckCountForksInOneEventCommandHandler : IRequestHandler<CheckCountForksInOneEventCommand,bool>
{
    private readonly ForkSettings _forkSettings;

    public CheckCountForksInOneEventCommandHandler(ISettingsProvider settingsProvider)
    {
        _forkSettings = settingsProvider.GetSettings();
    }

    public Task<bool> Handle(CheckCountForksInOneEventCommand request, CancellationToken cancellationToken)
    {
        var fork = request.Fork;
        if (fork == null)
            return Task.FromResult(false);


        var firstNameEvent = fork.FirstBet.EventName;
        var secondNameEvent = fork.SecondBet.EventName;
        var (firstNameEventTwin,secondNameEventTwin) = FilterCountStakes.TwinString(firstNameEvent, secondNameEvent);

        var countForksInOneEvent = FilterCountStakes.CountForksInOneEvent.GetValueOrDefault(firstNameEventTwin, 0) +
                                   FilterCountStakes.CountForksInOneEvent.GetValueOrDefault(secondNameEventTwin, 0);
        
        return Task.FromResult(countForksInOneEvent < _forkSettings.CountAdmissibleNonOverlappedInOneEvent);
    }
}