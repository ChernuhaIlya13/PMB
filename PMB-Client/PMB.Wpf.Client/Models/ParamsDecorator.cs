using PMB.Browsers.Common;

namespace PMB.Wpf.Client.Models
{
    public record ParamsDecorator(IBotBrowserDecorator decorator, string eventId);
}