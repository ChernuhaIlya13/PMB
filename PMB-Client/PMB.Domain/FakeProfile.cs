using PMB.Domain.BrowserModels;
using System.Collections.Generic;

namespace PMB.Cef.Core.FakeConfig
{
    public class ChromeLanguageInfo
    {
        public ChromeLanguage Language { get; set; }
        
        public string Local { get; set; }
        
        public string AcceptList { get; set; }
        
        public string Name { get; set; }
    }

    public class MediaDevicesSettings
    {
        public bool HideDevices { get; set; }
        
        public int VideoInputs { get; set; }
        
        public int AudioInputs { get; set; }
        
        public int AudioOutputs { get; set; }
        
        public int SavedVideoInputs { get; set; }
        
        public int SavedAudioInputs { get; set; }
        
        public int SavedAudioOutputs { get; set; }
        
        public string StringPresent { get; set; }
    }

    public class WebRtcSettings
    {
        public string StringPresent { get; set; }
        
        public string LocalIp { get; set; }
        
        public string PublicIp { get; set; }
        
        public string WebRtcStatus { get; set; }
    }

    public class GeoSettings
    {
        public bool _withIP { get; set; }//не используется
        
        public bool SetWithIp { get; set; }//не используется
        
        public string StringPresent { get; set; }//не используется
        
        public int Acc { get; set; }
    }

    public class InfoResult
    {
        public string status { get; set; }
        
        public object message { get; set; }
        
        public string country { get; set; }
        
        public string countryCode { get; set; }
        
        public string region { get; set; }
        
        public string regionName { get; set; }
        
        public string city { get; set; }
        
        public double lat { get; set; }
        
        public double lon { get; set; }
        
        public string timezone { get; set; }
        
        public string query { get; set; }
    }

    public class LeaksResult
    {
        public object status { get; set; }
        
        public object message { get; set; }
        
        public object country { get; set; }
        
        public object countryCode { get; set; }
        
        public object region { get; set; }
        
        public object regionName { get; set; }
        
        public object city { get; set; }
        
        public double lat { get; set; }
        
        public double lon { get; set; }
        
        public string timezone { get; set; }
        
        public string query { get; set; }
    }


    public class IpStoredInfo
    {
        public InfoResult InfoResult { get; set; }
        
        public LeaksResult LeaksResult { get; set; }
        
        public string PublicIp { get; set; }
        
        public string IP { get; set; }
    }


    public class FakeProfile
    {
        public bool DisableAnonym { get; set; }
        public BrowserType BrowserTypeType { get; set; }//не используется
        
        public WindowsVersion WindowsVersion { get; set; }
        
        public ChromeLanguageInfo ChromeLanguageInfo { get; set; }////не используется
        
        public int CurrentChromeLanguage { get; set; }//не используется
        
        public int OsVersion { get; set; }//не используется
        
        public bool IsX64 { get; set; }//не используется
        
        public string Platform { get; set; }//закомментирована в JS
        
        public int CpuConcurrency { get; set; }
        
        public int MemoryAvailable { get; set; }
        
        public bool IsSendDoNotTrack { get; set; }//не используется
        
        public bool IsMac { get; set; }//не используется
        
        public string UserAgent { get; set; }
        
        public string UserAgentChromeVersion { get; set; }//не используется
        
        public string UserAgentFullVersion { get; set; }//не используется
        
        public int UserAgentChromeVersionInt { get; set; }
        
        public string AppVersion { get; set; }//закомментирована в JS
        
        public bool HideCanvas { get; set; }
        
        public string CanvasFingerPrintHash { get; set; }
        
        public double BaseLatency { get; set; }
        
        public double ChannelDataDelta { get; set; }
        
        public double ChannelDataIndexDelta { get; set; }
        
        public double FloatFrequencyDataDelta { get; set; }
        
        public double FloatFrequencyDataIndexDelta { get; set; }
        
        public ScreenSize ScreenSize { get; set; }
        
        public bool HideFonts { get; set; }//закомментирована в JS
        
        public List<string> Fonts { get; set; }
        
        public int FontsCount { get; set; }//не используется
        
        public WebGl WebGL { get; set; }
        
        public MediaDevicesSettings MediaDevicesSettings { get; set; }
        
        public WebRtcSettings WebRtcSettings { get; set; }
        
        public GeoSettings GeoSettings { get; set; }
        
        public IpStoredInfo IpStoredInfo { get; set; }
        
        public TimezoneSetting TimezoneSetting { get; set; }
    }
}