using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Abb.Client.Client;
using PMB.Application.Utils;
using PMB.Domain.Logger;

namespace PMB.Application.Commands.Handlers;

public record StartBotCommand : IRequest<bool>;

[UsedImplicitly]
internal sealed class StartBotCommandHandler: IRequestHandler<StartBotCommand,bool>
{
    private readonly IAbbSignalRClient _abbClient;
    private readonly IPanelLogger _panelLogger;

    public StartBotCommandHandler(
        IAbbSignalRClient abbClient, 
        IPanelLogger panelLogger)
    {
        _abbClient = abbClient;
        _panelLogger = panelLogger;
    }


    public async Task<bool> Handle(StartBotCommand request, CancellationToken cancellationToken)
    {
        var connectedAbbClient = false;
        try
        {
            await _abbClient.Start(UserCredentials.JwtToken, CancellationToken.None);
            connectedAbbClient = true;
        }
        catch (Exception)
        {
            connectedAbbClient = false;
        }

        return connectedAbbClient;
    }
}