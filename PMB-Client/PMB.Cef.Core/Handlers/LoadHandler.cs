using System.Threading.Tasks;
using CefSharp;
using PMB.Cef.Core.FakeConfig;

namespace PMB.Cef.Core.Handlers
{
    public class LoadHandler : ILoadHandler
    {
        private readonly string _jsCode;
        private readonly string _injectCode;
        private readonly string _fakeProfile;
        private readonly bool _disableAnonym;
        
        public LoadHandler(string jsCode,string injectCode,string fakeProfile,bool disableAnonym)
        {
            _jsCode = jsCode;
            _injectCode = injectCode;
            _fakeProfile = fakeProfile;
            _disableAnonym = disableAnonym;
        }
        
        public void OnFrameLoadEnd(IWebBrowser chromiumWebBrowser, FrameLoadEndEventArgs frameLoadEndArgs)
        {
        }
        
        public async void OnFrameLoadStart(IWebBrowser chromiumWebBrowser, FrameLoadStartEventArgs frameLoadStartArgs)
        {
                await frameLoadStartArgs.Frame.EvaluateScriptAsync(_jsCode, "extensions::jscode");
                if (!_disableAnonym)
                    await FakeExtensions.InjectFake(frameLoadStartArgs, _injectCode, _fakeProfile);
        }
        
        public void OnLoadError(IWebBrowser chromiumWebBrowser, LoadErrorEventArgs loadErrorArgs)
        {

        }
        
        public void OnLoadingStateChange(IWebBrowser chromiumWebBrowser, LoadingStateChangedEventArgs loadingStateChangedArgs)
        {
        }
    }
}
