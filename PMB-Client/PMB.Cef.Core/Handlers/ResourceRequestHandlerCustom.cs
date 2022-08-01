using CefSharp;
using CefSharp.Handler;

namespace PMB.Cef.Core.Handlers
{
    public class ResourceRequestHandlerCustom : ResourceRequestHandler
    {
        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request,
            IRequestCallback callback)
        {
            if (request.Url.Contains("lawfilter.ertelecom"))
            {
                if (!browser.IsDisposed && !browser.MainFrame.IsDisposed)
                {
                    browser.MainFrame.ExecuteJavaScriptAsync("console.error('" + request.Method + " " + request.Url + " блокировка на клиенте от провайдера');", request.Url, 1);
                }

                using (callback)
                {
                    callback.Continue(false);
                    return CefReturnValue.Cancel;
                }
                    //callback.Continue(false);
            }

            return CefReturnValue.Continue;
        }
    }
}