using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PMB.Client;
using System.IO;
using System.Linq;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using PMB.Abb.Client;
using PMB.Application;
using PMB.Application.Interfaces;
using PMB.Browsers;
using PMB.Browsers.Common;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.Logger;
using PMB.Wpf.Client.Infra;
using PMB.Wpf.Client.Operations;
using PMB.Wpf.Client.Options;
using PMB.Wpf.Client.Services;
using PMB.Wpf.Client.Utilities;
using PMB.Wpf.Client.View.BotSettings;
using PMB.Wpf.Client.View.LoginView;
using PMB.Wpf.Client.ViewModel;
using PMB.Cef.Core;

namespace PMB.Wpf.Client
{
    public partial class App
    {
        private readonly IHost _host;
        
        public App()
        {
            _host = CreateHostBuilder().Build();
        }

        private static IHostBuilder CreateHostBuilder(string[] args = null)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(l =>
                {
                    l.ClearProviders();
                    l.AddNLog();
                })
                .ConfigureAppConfiguration(configuration =>
                    {
                        configuration
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
                    }
                )
                .ConfigureServices((context, services) =>
                {
                    NLog.LogManager.Configuration = new NLogLoggingConfiguration(context.Configuration.GetSection("NLog"));

                    services.Configure<SettingsOptions>(context.Configuration.GetSection(nameof(SettingsOptions)));
                    services.Configure<SignalrOptions>(context.Configuration.GetSection(nameof(SignalrOptions)));
                    services.AddSingleton<ISettingsProvider, FilterSettingsProvider>();
                    services.AddHttpClient<PmbApiClient>(x => context.Configuration.Bind("Http:Pmb", x));
                    services.AddApplication();
                    services.AddSingleton<WindowManager>();
                    services.AddSingleton<ForksServiceProvider>();
                    services.AddSingleton<IPanelLogger, PanelLogger>();
                    services.AddSingleton<ForkBetOperations>();
                    services.AddSingleton<BrowserOperations>();
                    services.AddSingleton<ForkProcessor>();
                    services.AddSingleton(sp =>
                    {
                        var settings = sp.GetRequiredService<IOptions<SettingsOptions>>().Value;
                        return new JsLoader(settings.Bookmakers.Select(x => x.Bookmaker + ".js").ToArray());
                    });
                    services.UseAbbProvider(x => context.Configuration.Bind("AbbProviderOptions", x));
                    services.UseBookmakers();
                    services.AddLogging(builder => builder.ClearProviders().AddConsole());
                    
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<MainWindowViewModel>();
                    services.AddSingleton<BotSettingsWindow>();
                    services.AddSingleton<BotSettingsViewModel>();
                    services.AddSingleton<LoginView>();
                    services.AddSingleton<LoginViewModel>();
                    services.AddSingleton<IDialogCoordinator,DialogCoordinator>();
                    services.AddSingleton<DialogCoordinatorCustom>();
                });
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            _host.RunAsync();
            _host.Services.Show<LoginView>();
            
            ChromiumInit.Init();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.Services.GetRequiredService<ForksServiceProvider>().Dispose();
            await _host.StopAsync();
            _host.Dispose();
            
            base.OnExit(e);
        }
    }
}
