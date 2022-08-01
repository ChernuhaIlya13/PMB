using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Models;
using PMB.Domain.Logger;

namespace PMB.Application.Commands.Handlers;

public record LoadEventsCommand(BetDecorator[] BetDecorators) : IRequest<bool>;

[UsedImplicitly]
internal sealed class LoadEventsCommandHandler : IRequestHandler<LoadEventsCommand,bool>
{
    private IPanelLogger _panelLogger { get; }

    public LoadEventsCommandHandler(IPanelLogger panelLogger)
    {
        _panelLogger = panelLogger;
    }
    public async Task<bool> Handle(LoadEventsCommand request, CancellationToken cancellationToken)
    {
        var loadResult = await Task.WhenAll(request.BetDecorators.Select(async betDecorator =>
        {
            var loadEvent = false;
            try
            {
                loadEvent = await betDecorator.Decorator.LoadEvents(betDecorator.BetMain);
            }
            catch
            {
                // ignored
            }
            return loadEvent;
        }).ToArray());
        if (loadResult.Any(e => !e))
        {
            await _panelLogger.AddInfoLog("Не смог загрузить события на конторе");
            return false;
        }

        return true;
    }
}