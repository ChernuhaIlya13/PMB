using CefSharp;
using CefSharp.Wpf;
using System;
using System.IO;
using System.Windows;

namespace PMB.Cef.Core
{
    public static class ChromiumInit
    {
        public static void Init()
        {
            var cefSettings = new CefSettings
            {
                RootCachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache"),
                PersistSessionCookies = true,
                AcceptLanguageList = "ru,en,en-GB,en-US",
                LocalesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "locales"),
                LogSeverity = LogSeverity.Error,
                IgnoreCertificateErrors = true,
            };

            FillCommandLineArgs(cefSettings);

            CefSharpSettings.ShutdownOnExit = true;
            CefSharpSettings.ConcurrentTaskExecution = true;

            CefSharp.Cef.EnableHighDPISupport();
            if (!CefSharp.Cef.IsInitialized && !CefSharp.Cef.Initialize(cefSettings))
                throw new ArgumentException("Chrome не инициализирован");
        }

        private static void FillCommandLineArgs(CefSettings cefSettings)
        {
            if (!cefSettings.CefCommandLineArgs.ContainsKey("disable-gpu"))
                cefSettings.CefCommandLineArgs.Add("disable-gpu", "1");
            
            if (cefSettings.CefCommandLineArgs.ContainsKey("enable-system-flash"))
                cefSettings.CefCommandLineArgs.Remove("enable-system-flash");
            
            if (cefSettings.CefCommandLineArgs.ContainsKey("enable-media-stream"))
                cefSettings.CefCommandLineArgs.Remove("enable-media-stream");
            
            cefSettings.CefCommandLineArgs.Add("enable-media-stream", "0");
            // cefSettings.CefCommandLineArgs.Add("webrtc.ip_handling_policy", "disable_non_proxied_udp");
            // cefSettings.CefCommandLineArgs.Add("webrtc.multiple_routes_enabled", "0");
            // cefSettings.CefCommandLineArgs.Add("webrtc.nonproxied_udp_enabled", "1");
        }
    }
}