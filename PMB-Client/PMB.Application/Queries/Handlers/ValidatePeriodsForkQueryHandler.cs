using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Models;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;

namespace PMB.Application.Queries.Handlers;

public record ValidatePeriodsForkQuery(List<BetDecorator> Decorators) : IRequest<StatusParsePeriods>;

[UsedImplicitly]
internal sealed class ValidatePeriodsForkQueryHandler: IRequestHandler<ValidatePeriodsForkQuery, StatusParsePeriods>
{
    public async Task<StatusParsePeriods> Handle(ValidatePeriodsForkQuery request, CancellationToken cancellationToken)
    {
        var firstBetDecorator = request.Decorators.First();
        var secondBetDecorator = request.Decorators.Skip(1).First();
        MatchData firstMatchDataInfo = new();
        MatchData secondMatchDataInfo = new();
        try
        {
            firstMatchDataInfo = await firstBetDecorator.Decorator.GetMatchData(new MatchDataParam(firstBetDecorator.BetMain.EvId));
            secondMatchDataInfo = await secondBetDecorator.Decorator.GetMatchData(new MatchDataParam(secondBetDecorator.BetMain.EvId));
        }
        catch
        {
            return StatusParsePeriods.FailedParsePeriods;
        }

        return StatusParsePeriods.None;
    }
}