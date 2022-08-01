using CefSharp;
using PMB.Cef.Core;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;

namespace PMB.Pinnacle.Browser
{
    public class PinnacleBrowser : BotBrowser
    {
        private PinnacleClient _client;
        public PinnacleBrowser(Bookmaker bookmaker, string captcha, RequestContextSettings requestContextSettings,PinnacleClient client)
            : base(bookmaker, captcha, requestContextSettings)
        {
            _client = client;
        }

        protected override JsProxyBase ResolveWorker(Bookmaker bookmaker, string captcha, BotBrowser browser) =>
            new JsProxyPinnacle(captcha, this, bookmaker.BrowserOptions.Proxy,_client);
    }
}