using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Abb.Client.Providers;
using PMB.Application.Interfaces;
using PMB.Application.Utils;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;

namespace PMB.Application.Queries.Handlers;

public record ForkQuery(): IRequest<ForkMain>;

[UsedImplicitly]
internal sealed class ForkQueryHandler: IRequestHandler<ForkQuery, ForkMain>
{
    private readonly IAbbForksProvider _abbForksProvider;
    private readonly ForkSettings _forkSettings;
    
    public ForkQueryHandler(IAbbForksProvider abbForksProvider, ISettingsProvider settingsProvider)
    {
        _abbForksProvider = abbForksProvider;
        _forkSettings = settingsProvider.GetSettings();
    }

    public Task<ForkMain> Handle(ForkQuery request, CancellationToken cancellationToken)
    { 
        var forkProfit = _forkSettings.Profit;
        var coefStakes = _forkSettings.Coefficient;
        var timeOfLife = _forkSettings.TimeOfLife;
        // достает и сразу удаляет вилку из списка
        // вместо null прокинуть лямбду
        // доходность вилки,коэффициент ставок,время жизни вилки
        var fork = _abbForksProvider.Pop((fork =>
        {
            var firstBet = fork.Bets.First();
            var secondBet = fork.Bets.Skip(1).First();
            var timeOfLifeFork = DateTimeOffset.Now.ToLocalTime() - fork.AbbDto.CreatedAt.ToLocalTime();
            return firstBet.Coefficient >= coefStakes.Start
                   && firstBet.Coefficient <= coefStakes.Finish
                   && secondBet.Coefficient >= coefStakes.Start
                   && secondBet.Coefficient <= coefStakes.Finish
                   && fork.AbbDto.Percent >= forkProfit.Start && fork.AbbDto.Percent <= forkProfit.Finish
                   && timeOfLifeFork >= TimeSpan.FromSeconds(timeOfLife.Start)
                   && timeOfLifeFork <= TimeSpan.FromSeconds(timeOfLife.Finish)
                   && firstBet.BookmakerId != secondBet.BookmakerId;
            // && (firstBet.BookmakerId == 1 || firstBet.BookmakerId == 6)
            // && (secondBet.BookmakerId == 1 || secondBet.BookmakerId == 6);
        }));
        
        return Task.FromResult(fork?.Convert());
    }
}