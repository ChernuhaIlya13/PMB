using System.Collections.Generic;
using System.Security.AccessControl;
using PMB.Cef.Core.FakeConfig;

namespace PMB.Domain.BrowserModels
{
    public class BrowserOptions
    {
        public Proxy Proxy { get; } = new();
        
        public AuthBookmaker Auth { get; } = new();
        
        public bool SaveCacheInMemory { get; set; }

        public FakeProfile Anonym { get; set; } = new();
    }
}