using System.Threading.Tasks;
using CefSharp;

namespace PMB.Cef.Core.FakeConfig
{
    public static class FakeExtensions
    {
        public static async Task InjectFake(IFrame frame,string fakeInjection,string fakeProfile)
        {
            if (!frame.IsDisposed)
            {
                //await frame.EvaluateScriptAsync($"{fakeInjection};setFakeProfile({fakeProfile});_secret();injectFake()", "extensions::injectionCode");
                await frame.EvaluateScriptAsync($"{fakeInjection}");
            }
        }
        public static async Task InjectFake(FrameLoadStartEventArgs frameLoad,string fakeInjection,string fakeProfile)
        {
            if (!frameLoad.Frame.IsDisposed)
            {
                await frameLoad.Frame.EvaluateScriptAsync($"{fakeInjection}");
                //await frameLoad.Frame.EvaluateScriptAsync($"{fakeInjection};setFakeProfile({fakeProfile});_secret();injectFake()", "extensions::injectionCode");
            }
        }
    }
}