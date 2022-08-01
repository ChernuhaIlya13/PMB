using Microsoft.Extensions.DependencyInjection;
using PMB.Browsers.Common;
using PMB.Pinnacle.Browser;

namespace PMB.Pinnacle
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UsePinnacle(this IServiceCollection services)
        {
            services.AddSingleton<IBotBrowserDecorator, PinnacleDecorator>();

            return services;
        }
    }
}