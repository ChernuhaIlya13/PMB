using Microsoft.Extensions.DependencyInjection;
using PMB.Browsers.Common;

namespace PMB.Bet365
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseBet365(this IServiceCollection services)
        {
            services.AddSingleton<IBotBrowserDecorator, Bet365Decorator>();

            return services;
        }
    }
}