using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;

namespace PMB.Application.Commands.Handlers;

public record CheckBetIdAndGetBetsCommand(ForkMain Fork) : IRequest<CheckBetIdAndGetBetsCommandResult>;

public record CheckBetIdAndGetBetsCommandResult(bool IsValid, BetMain[] BetsFromStash);

[UsedImplicitly]
internal sealed class CheckBetIdAndGetBetsCommandHandler: IRequestHandler<CheckBetIdAndGetBetsCommand, CheckBetIdAndGetBetsCommandResult>
{
    private readonly IPanelLogger _panelLogger;

    public CheckBetIdAndGetBetsCommandHandler(IPanelLogger panelLogger)
    {
        _panelLogger = panelLogger;
    }

    public async Task<CheckBetIdAndGetBetsCommandResult> Handle(CheckBetIdAndGetBetsCommand request, CancellationToken cancellationToken)
    {
        var first = request.Fork.FirstBet;
        var second = request.Fork.SecondBet;
        
        var betsFromStash = new List<BetMain>
        {
            first,
            second
        };
        
        var betIdsExists = !string.IsNullOrWhiteSpace(first.BetId) && !string.IsNullOrWhiteSpace(second.BetId);
        if (!betIdsExists)
        {
            await _panelLogger.AddInfoLog("Не нашёл BetId в одной из ставок");
            return new CheckBetIdAndGetBetsCommandResult(false, null);
        }
        
        return new CheckBetIdAndGetBetsCommandResult(true, betsFromStash.ToArray());
    }
}