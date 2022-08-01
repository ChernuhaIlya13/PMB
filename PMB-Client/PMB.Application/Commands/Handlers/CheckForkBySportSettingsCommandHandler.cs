using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Models;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;

namespace PMB.Application.Commands.Handlers;

public record CheckForkBySportSettingsCommand(BetDecorator[] BetDecorators) : IRequest<bool>;

[UsedImplicitly]
internal sealed class CheckForkBySportSettingsCommandHandler : IRequestHandler<CheckForkBySportSettingsCommand,bool>
{
    private IPanelLogger _panelLogger { get; }

    public CheckForkBySportSettingsCommandHandler(IPanelLogger looger)
    {
        _panelLogger = looger;
    }
    public async Task<bool> Handle(CheckForkBySportSettingsCommand request, CancellationToken cancellationToken)
    {
        var betDecorators = request.BetDecorators;
        var firstDecorator = betDecorators.FirstOrDefault();
        var secondDecorator = betDecorators.Skip(1).FirstOrDefault();
        var bets = new List<BetMain>()
        {
            firstDecorator.BetMain,
            secondDecorator.BetMain
        };
        if (!CurrentSportIsTennis(bets))
        {
            return true;
        }
        
        var isValidGamesInFirstDecorator = await firstDecorator.Decorator.CheckGameInTennis(firstDecorator.BetMain.EvId);
        var isValidGamesInSecondDecorator =  await secondDecorator.Decorator.CheckGameInTennis(secondDecorator.BetMain.EvId);
        
        var result = isValidGamesInFirstDecorator && isValidGamesInSecondDecorator;
        if (!result)
        {
            await _panelLogger.AddInfoLog("11,13,23,36 Геймы отменяю");
            return false;
        }

        return true;
    }

    public bool CurrentSportIsTennis(List<BetMain> bets)
    {
        return bets.Any(bet => bet.Sport.Contains("tennis", StringComparison.OrdinalIgnoreCase));
    }
}