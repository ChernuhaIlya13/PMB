using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Utils;
using PMB.Client;
using PMB.Models.V1.Requests;
using PMB.Models.V1.Responses;

namespace PMB.Application.Commands.Handlers;

public record LoginCommand(string BotKey) : IRequest<LoginCommandResult>;

public record LoginCommandResult(string Title, string Message);

[UsedImplicitly]
internal sealed class LoginCommandHandler: IRequestHandler<LoginCommand, LoginCommandResult>
{
    private readonly PmbApiClient _client;

    public LoginCommandHandler(PmbApiClient client)
    {
        _client = client;
    }

    public async Task<LoginCommandResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var key = request.BotKey;
        
        if (string.IsNullOrWhiteSpace(key))
        {
            return new LoginCommandResult("Проверка Ключа", "Пустое значение ключа");
        }
        
        BotLoginResponse botResponse;
        try
        {
            botResponse = await _client.BotLogin(new BotLoginRequest
            {
                Key = key
            }, cancellationToken);
        }
        catch(Exception)
        {
            return new LoginCommandResult("Проверка Ключа", "Неверное значение ключа");
        }
        
        if (!string.IsNullOrWhiteSpace(botResponse.Token))
        {
            UserCredentials.JwtToken = botResponse.Token;
            UserCredentials.UserKey = key;
        }

        return null;
    }
}