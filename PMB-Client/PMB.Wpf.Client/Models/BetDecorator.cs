using PMB.Browsers.Common;
using PMB.Domain.ForkModels;

namespace PMB.Wpf.Client.Models
{
    public record BetDecorator(BetMain BetMain, IBotBrowserDecorator Decorator);
}