using PMB.Cef.Core;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;

namespace PMB.Pinnacle.Browser
{
    public class JsProxyPinnacle : JsProxyBase
    {
        private PinnacleClient _client;
        private readonly Proxy _proxy;
        
        public JsProxyPinnacle(string captcha, BotBrowser browser,Proxy proxy,PinnacleClient client) 
            : base(captcha, browser)
        {
            _proxy = proxy;
            _client = client;
        }
    }
}