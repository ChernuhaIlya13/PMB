using PMB.Domain.BrowserModels;

namespace PMB.Browsers.Common
{
    public record BookmakerCommonInfo(string Uri, AuthBookmaker AuthInfo, string[] BookmakerUrls, string Host);
}