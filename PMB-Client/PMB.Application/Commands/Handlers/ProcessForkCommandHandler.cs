using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Abb.Models.Models;
using PMB.Application.Utils;
using PMB.Browsers.Common;
using PMB.Domain.BrowserModels;
using PMB.Domain.ForkModels;
using PMB.Domain.Logger;

namespace PMB.Application.Commands.Handlers;

public record ProcessForkCommand(ForkMain Fork, List<IBotBrowserDecorator> Decorators, int NotOverlappedForksCount,
    decimal FirstBookmakerStavka, decimal SecondBookmakerStavka,ForkSettings Settings) : IRequest<ProcessForkCommandResult>;

public record ProcessForkCommandResult(bool Ok, decimal FirstBookmakerStavka, decimal SecondBookmakerStavka,
    int NotOverlappedForks);

[UsedImplicitly]
internal sealed class ProcessForkCommandHandler : IRequestHandler<ProcessForkCommand, ProcessForkCommandResult>
{
    private readonly IMediator _mediator;
    private readonly IPanelLogger _panelLogger;
    
    public ProcessForkCommandHandler(IPanelLogger panelLogger, IMediator mediator)
    {
        _panelLogger = panelLogger;
        _mediator = mediator;
    }

    public async Task<ProcessForkCommandResult> Handle(ProcessForkCommand request, CancellationToken cancellationToken)
    {
        var fork = request.Fork;
        var settings = request.Settings;
        
        var firstBookmaker = ForkExtensions.ParseEnum(fork.FirstBet.BookmakerId, AbbBookmaker.None); 
        var secondBookmaker = ForkExtensions.ParseEnum(fork.SecondBet.BookmakerId, AbbBookmaker.None); 
        var sport = ForkExtensions.ParseEnum(fork.FirstBet.SportId, AbbSport.None);
        
        cancellationToken.ThrowIfCancellationRequested();
        
        await _panelLogger.AddForkInfo("Пришла вилка", "", fork.Home + fork.Away, $"{firstBookmaker} - {secondBookmaker}", sport.ToString());
        
        try
        {
            var result = await _mediator.Send(new CheckBetsCommand(
                fork, 
                request.Decorators,
                request.NotOverlappedForksCount,
                request.FirstBookmakerStavka,
                request.SecondBookmakerStavka,settings), cancellationToken);
            return new (result.Ok, result.FirstBookmakerStavka, result.SecondBookmakerStavka, result.NotOverlappedForks);
        }
        catch
        {
            await _panelLogger.AddInfoLog("Catch ProcessFork");
        }

        return new (false, request.FirstBookmakerStavka, request.SecondBookmakerStavka, request.NotOverlappedForksCount);;
    }
}