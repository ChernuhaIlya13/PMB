using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Models;
using PMB.Application.Utils;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;

namespace PMB.Application.Queries.Handlers;

public record ValidateTimeForkQuery(BetDecorator[] BetDecorators) : IRequest<StatusParseMinutes>;

[UsedImplicitly]
internal sealed class ValidateTimeForkQueryHandler: IRequestHandler<ValidateTimeForkQuery, StatusParseMinutes>
{
    public async Task<StatusParseMinutes> Handle(ValidateTimeForkQuery request, CancellationToken cancellationToken)
    {
        Task<(StatusParseMinutes Status, TimeSpan? Time)>[] timeTasks = request.BetDecorators.Select(
            async betDecorator =>
            {
                if (string.IsNullOrEmpty(betDecorator.BetMain?.EvId))
                {
                    return (StatusParseMinutes.UnknownFail, null);
                }

                MatchData matchData = new();
                try
                {
                    matchData =
                        await betDecorator.Decorator.GetMatchData(new MatchDataParam(betDecorator.BetMain.EvId));
                }
                catch
                {
                    return (StatusParseMinutes.TimeFailedParse,null);
                }
                if (matchData == null)
                    return (StatusParseMinutes.TimeFailedParse,null);
                    
                var time = ForkExtensions.FetchMinutesFromBookmaker(matchData.Time.MainTime);

                if (!time.HasValue || time.Value == TimeSpan.Zero)
                    return (StatusParseMinutes.TimeFailedParse, null);

                return (StatusParseMinutes.None, time);
            }).ToArray();

        var times = await Task.WhenAll(timeTasks);

        if (times.All(x => x.Status == StatusParseMinutes.None && x.Time != null))
        {
            return times
                .SelectMany(x =>
                    times.Select(y =>
                        Math.Abs((x.Time!.Value - y.Time!.Value).TotalSeconds) < TimeSpan.FromMinutes(3).TotalSeconds))
                .All(x => x)
                ? StatusParseMinutes.TimeMatch
                : StatusParseMinutes.TimeNotMatch;
        }

        if (times.Any(x => x.Status == StatusParseMinutes.TimeFailedParse))
        {
            return StatusParseMinutes.TimeFailedParse;
        }

        return StatusParseMinutes.UnknownFail;
    }
}