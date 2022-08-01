using Microsoft.Extensions.DependencyInjection;
using PMB.Browsers.Common;

namespace PMB.Fonbet;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection UseFonbet(this IServiceCollection services)
    {
        services.AddSingleton<IBotBrowserDecorator, FonbetDecorator>();

        return services;
    }
}