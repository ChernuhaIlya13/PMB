using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;
using PMB.Domain.Logger;
using BrowserSettings = CefSharp.Core.BrowserSettings;
using RequestContext = CefSharp.RequestContext;
using RequestContextSettings = CefSharp.RequestContextSettings;

namespace PMB.Cef.Core
{
    public class BotBrowser: ChromiumWebBrowser
    {
        public readonly string BookmakerName;

        public readonly JsProxyBase Worker;

        protected BotBrowser(Bookmaker bookmaker, string captcha, RequestContextSettings requestContextSettings)
        {
            BrowserSettings = new BrowserSettings()
            {
                ImageLoading = bookmaker.BrowserOptions.Auth.ShowImages ? CefState.Default : CefState.Disabled,
            };
            RequestContext = new RequestContext(requestContextSettings);

            // ReSharper disable once VirtualMemberCallInConstructor
            var worker = ResolveWorker(bookmaker, captcha, this);

            BookmakerName = bookmaker.BookmakerName;
            Address = bookmaker.Uri;
            Worker = worker;
            JavascriptObjectRepository.Register("worker", worker);
        }

        protected virtual JsProxyBase ResolveWorker(Bookmaker bookmaker, string captcha, BotBrowser browser) =>
            new(captcha, browser);

        public void SubscribeOnLogger(IPanelLogger logger) =>
            Worker.LoggerEvents.ObserveOn(
                new SynchronizationContextScheduler(
                    new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher)))
                .Subscribe((async delegate(string s)
                {
                    await logger.AddInfoLog(s);
                }));
    }
}
