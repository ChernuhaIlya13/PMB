using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PMB.Application.Interfaces;
using PMB.Client;
using PMB.Models.PositiveModels;
using PMB.Models.V1.Requests;
using PMB.Models.V1.Responses;
using PMB.Wpf.Client.Options;
using PMB.Wpf.Client.Utilities;

namespace PMB.Wpf.Client.Services
{
    public class ForksServiceProvider
    {
        public readonly Subject<Fork> Forks = new();
        
        private readonly string _url;

        private string _token;

        private readonly ISettingsProvider _settings;

        private HubConnection _connection;

        private readonly PmbApiClient _client;
        
        public ForksServiceProvider(ISettingsProvider settingsProvider, IOptions<SignalrOptions> hubOptions,PmbApiClient client)
        {
            _settings = settingsProvider;
            _url = hubOptions.Value.HubUrl;
            _client = client;
        }

        private async Task<BotLoginResponse> Login(string key)
        {
            return await _client.BotLogin(new BotLoginRequest()
            {
                Key = key
            });
        }
        
        public async Task Initialize(string key)
        {
            var result = await Login(key);

            _token = result.Token;

            _connection = new HubConnectionBuilder()
                .WithUrl(_url, (options) =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_token);
                    options.CloseTimeout = new TimeSpan(0, 1, 0);
                })
                .WithAutomaticReconnect(new []{2, 4, 6}.Select(x => TimeSpan.FromSeconds(x)).ToArray())
                .Build();

            _connection.Reconnected += async connectionId =>
            {
                await Task.Delay(1000);
                await ReadyToGetForks();
            };
            
            _connection.Closed += async exception =>
            {
                await _connection.StartAsync();
                await ReadyToGetForks();
            }; 
                
            _connection.On<Fork>("ReceiveForks", fork =>
            {
                 Forks.OnNext(fork);
            });
            
            await _connection.StartAsync();
            await SetFilters();
        }

        private async Task SetFilters()
        {
            var settings = _settings.GetSettings();
            
            await _connection.InvokeAsync("SetFilters", settings.ConvertToForksFilterMessage());
        }
        
        public async Task ReadyToGetForks()
        {
            await _connection.InvokeAsync("ReadyToGetForks");
        }

        public async Task ReadyToGetForksSafe()
        {
            while(_connection.State != HubConnectionState.Connected)
            {
                await Task.Delay(1000);
            }
            await _connection.InvokeAsync("ReadyToGetForks");
        }

        public Task Dispose()
        {
            Forks?.Dispose();
            return _connection?.DisposeAsync().AsTask();
        }

        public HubConnectionState GetHubState() => _connection.State;
    }
}