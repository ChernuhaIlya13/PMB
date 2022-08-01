using Microsoft.Extensions.DependencyInjection;
using PMB.Bet365;
using PMB.Fonbet;
using PMB.Pinnacle;

namespace PMB.Browsers
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseBookmakers(this IServiceCollection services)
        {
            services.UseBet365();
            services.UsePinnacle();
            services.UseFonbet();
            services.AddSingleton<DecoratorFactory>();
            
            return services;
        }
    }
}