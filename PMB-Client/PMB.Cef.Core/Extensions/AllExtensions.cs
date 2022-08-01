using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMB.Cef.Core.Extensions
{
    public static class AllExtensions
    {
        public static Task SetProxy(this BotBrowser cwb, string address)
        {
            cwb.IsBrowserInitializedChanged += async (sender, args) =>
            {
                if (cwb.IsBrowserInitialized)
                {
                    await CefSharp.Core.Cef.UIThreadTaskFactory.StartNew(delegate
                    {
                        var rc = cwb.GetBrowser().GetHost().RequestContext;
                        var v = new Dictionary<string, object>();
                        v["mode"] = "fixed_servers";
                        v["server"] = (object)address;
                        string error;
                        bool success = rc.SetPreference("proxy", v, out error);
                    });
                }
            };
            
            return Task.CompletedTask;
        }
    }
}