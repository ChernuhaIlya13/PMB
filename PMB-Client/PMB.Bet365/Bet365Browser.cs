using CefSharp;
using PMB.Cef.Core;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;

namespace PMB.Bet365
{
    public class Bet365Browser: BotBrowser
    {
        public Bet365Browser(Bookmaker bookmaker, string captcha, RequestContextSettings requestContextSettings)
            : base(bookmaker, captcha, requestContextSettings)
        {
        }

        protected override JsProxyBase ResolveWorker(Bookmaker bookmaker, string captcha, BotBrowser browser) =>
            new JsProxyBet365(captcha, this);
        
    }
}