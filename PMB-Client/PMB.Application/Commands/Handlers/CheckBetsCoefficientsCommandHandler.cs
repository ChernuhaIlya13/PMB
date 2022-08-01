using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Interfaces;
using PMB.Application.Models;
using PMB.Domain.BrowserModels;
using PMB.Utils;

namespace PMB.Application.Commands.Handlers;

public record CheckBetsCoefficientCommand(BetDecorator[] BetDecorators) : IRequest<bool>;

[UsedImplicitly]
internal sealed class CheckBetsCoefficientsCommandHandler : IRequestHandler<CheckBetsCoefficientCommand,bool>
{
    private readonly ForkSettings _forkSettings;

    public CheckBetsCoefficientsCommandHandler(ISettingsProvider settingsProvider)
    {
        _forkSettings = settingsProvider.GetSettings();
    }

    public async Task<bool> Handle(CheckBetsCoefficientCommand request, CancellationToken cancellationToken)
    {
        var decorators = request.BetDecorators;
        decimal firstCoefficient;
        decimal secondCoefficient;
        try
        {
            firstCoefficient  = await decorators.First().Decorator.GetStakeCoefficientFromCoupon(decorators.First().BetMain);
            secondCoefficient = await decorators.Skip(1).First().Decorator.GetStakeCoefficientFromCoupon(decorators.Skip(1).First().BetMain);
        }
        catch
        {
            return false;
        }
        var forkProfit = DoStakeCommandHandler.CalcCurrentForkProfit(firstCoefficient, secondCoefficient);
        ViewModelUpdater.ForkProfit.OnNext(Convert.ToSingle(forkProfit));
        
        var isNormalProfit = forkProfit >= _forkSettings.Profit.Start && forkProfit <= _forkSettings.Profit.Finish;

        return isNormalProfit;
    }
}