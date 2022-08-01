using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Interfaces;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;

namespace PMB.Application.Commands.Handlers;
public record CheckForkCommand(ForkMain Fork): IRequest<(bool checkCountForks,bool checkIdenticalStakes,bool checkNotOverlappedStakes,bool checkBetName)>;

[UsedImplicitly]
internal sealed class CheckForkCommandHandler: IRequestHandler<CheckForkCommand, (bool checkCountForks,bool checkIdenticalStakes,bool checkNotOverlappedStakes,bool checkBetName)>
{
    private readonly ForkSettings _forkSettings;

    public CheckForkCommandHandler(ISettingsProvider settingsProvider)
    {
        _forkSettings = settingsProvider.GetSettings();
    }

    public Task<(bool checkCountForks,bool checkIdenticalStakes,bool checkNotOverlappedStakes,bool checkBetName)> Handle(CheckForkCommand request, CancellationToken cancellationToken)
    {
        var fork = request.Fork;

        var checkCountForks = true;
        var checkIdenticalStakes = true;
        var checkNotOverlappedStakes = true;
        var checkBetName = true;
            
        var firstNameEvent = fork?.FirstBet?.EventName;
        var secondNameEvent = fork?.SecondBet?.EventName;
        var (firstNameEventTwin, secondNameEventTwin) =
            FilterCountStakes.TwinString(firstNameEvent, secondNameEvent);
        
        if (fork != null)
        {
            var countForksInOneEvent = FilterCountStakes.CountForksInOneEvent.GetValueOrDefault(firstNameEventTwin, 0) +
                                       FilterCountStakes.CountForksInOneEvent.GetValueOrDefault(secondNameEventTwin, 0);
            
            checkCountForks = countForksInOneEvent < _forkSettings.CountAdmissibleNonOverlappedInOneEvent;
            
            var countIdenticalBets = _forkSettings.CountOfIdenticalSurebetsInOneEvent;
        
            var firstBetValue = fork.FirstBet.BetValue.Trim();
            var secondBetValue = fork.SecondBet.BetValue.Trim();

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

            checkIdenticalStakes = countStakes < countIdenticalBets;
            
            //Подсчитывает количество ставок для текущего события
            countStakes = FilterCountStakes.CountNotOverlappedStakes.GetValueOrDefault(firstNameEventTwin, 0) 
                              + FilterCountStakes.CountNotOverlappedStakes.GetValueOrDefault(secondNameEventTwin, 0);

            checkNotOverlappedStakes = countStakes < _forkSettings.CountAdmissibleNonOverlappedInOneEvent;

            var betNameSecond = BetNameFilter.BetNames.GetValueOrDefault(fork.FirstBet.BetValue);
            var betNameFirst = BetNameFilter.BetNames.GetValueOrDefault(fork.SecondBet.BetValue);
            
            if (betNameSecond != null)
            {
                checkBetName = betNameSecond.ToLower() == fork.SecondBet.BetValue.ToLower();
            }
            if (betNameFirst != null)
            {
                checkBetName = betNameFirst.ToLower() == fork.FirstBet.BetValue.ToLower();
            }
        }

        var result = (checkCountForks,checkIdenticalStakes,checkNotOverlappedStakes,checkBetName);

        return Task.FromResult(result);
    }
}