using PMB.Wpf.Client.Annotations;
using PMB.Wpf.Client.Commands;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PMB.Application.Commands;
using PMB.Application.Commands.Handlers;
using PMB.Wpf.Client.View.LoginView;
using PMB.Wpf.Client.Infra;
using PMB.Wpf.Client.Utilities;

namespace PMB.Wpf.Client.ViewModel
{
    public sealed class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;

        public ICommand Login { get; set; }

        public ICommand ForgotKey { get; set; }

        public string UserKey { get; set; } = "key";

        public readonly DialogCoordinatorCustom DialogCoordinatorCustom;
        
        public LoginViewModel(DialogCoordinatorCustom dialogCoordinatorCustom,
            IServiceProvider serviceProvider)
        {
            Login = new AsyncRelayCommand(_ => LoginCommand());
            ForgotKey = new AsyncRelayCommand(_ => ForgotKeyCommand());
            
            DialogCoordinatorCustom = dialogCoordinatorCustom;
            _serviceProvider = serviceProvider;
            _mediator = _serviceProvider.GetRequiredService<IMediator>();
        }

        private async Task LoginCommand()
        {
            var result = await _mediator.Send(new LoginCommand(UserKey));

            if (result == null)
            {
                _serviceProvider.Show<MainWindow>().Close<LoginView>();
            }
            else
            {
                await DialogCoordinatorCustom.ShowMessageAsyncDefault(this, result.Title, result.Message);
            }
        }

        private async Task ForgotKeyCommand() =>
            await DialogCoordinatorCustom.ShowMessageAsyncDefault(this, "Забыли пароль?", "Пишите,звоните,поможем");
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
