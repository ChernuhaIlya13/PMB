using System;
using CefSharp;
using Newtonsoft.Json;
using PMB.Cef.Core;
using PMB.Cef.Core.Handlers;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;

namespace PMB.Browsers.Common
{
    public static class BotBrowserFactory
    {
        public static BotBrowser CreateBrowser(Bookmaker bookmaker, JsLoader loader, 
            Func<RequestContextSettings, string, Bookmaker, BotBrowser> factory)
        {
            var requestContextSettings = new RequestContextSettings
            {
                CachePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\CefSharp\\Cache\\{bookmaker.BookmakerName}"
            };
            var fakeProfile = JsonConvert.SerializeObject(bookmaker.BrowserOptions.Anonym);

            var browser = factory(requestContextSettings, "f1e251951af2935522b58e5ca0b07d3f", bookmaker);
            browser.LoadHandler = new LoadHandler(loader.GetJsCode(bookmaker.BookmakerName), loader.GetJsCode("fakeinjection"),fakeProfile,bookmaker.BrowserOptions.Anonym.DisableAnonym);
            browser.RenderProcessMessageHandler = new RenderProcessMessageHandler(loader.GetJsCode("fakeinjection"),fakeProfile,bookmaker.BrowserOptions.Anonym.DisableAnonym);
            browser.MenuHandler = new CustomContextMenuHandler();

            return browser;
        }
    }
}
