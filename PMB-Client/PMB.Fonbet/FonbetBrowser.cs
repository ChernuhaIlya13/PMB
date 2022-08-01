using CefSharp;
using PMB.Cef.Core;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;

namespace PMB.Fonbet;

public class FonbetBrowser : BotBrowser
{
    public FonbetBrowser(Bookmaker bookmaker, string captcha, RequestContextSettings requestContextSettings) : base(bookmaker, captcha, requestContextSettings)
    {
    }
    protected override JsProxyBase ResolveWorker(Bookmaker bookmaker, string captcha, BotBrowser browser) =>
        new JsProxyFonbet(captcha, this);
}

