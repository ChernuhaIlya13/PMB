using CefSharp;
using PMB.Cef.Core.FakeConfig;

namespace PMB.Cef.Core.Handlers
{
    public class RenderProcessMessageHandler : IRenderProcessMessageHandler
    {
        public string _jsCode { get; }
        private readonly string _fakeProfile;
        private readonly bool _disableAnonym;
        
        public RenderProcessMessageHandler(string jscode, string fakeProfile,bool disableAnonym)
        {
            _jsCode = jscode;
            _fakeProfile = fakeProfile;
            _disableAnonym = disableAnonym;
        }
        
        public async void OnContextCreated(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
            if(!_disableAnonym)
                await FakeExtensions.InjectFake(frame, _jsCode, _fakeProfile);
        }

        public void OnContextReleased(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
        }

        public void OnFocusedNodeChanged(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IDomNode node)
        {
        }

        public void OnUncaughtException(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, JavascriptException exception)
        {
        }
    }
}
