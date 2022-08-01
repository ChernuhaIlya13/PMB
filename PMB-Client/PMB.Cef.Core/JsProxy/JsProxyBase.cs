using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CefSharp;
using PMB.Cef.Core.RuCaptcha;

namespace PMB.Cef.Core.JsProxy
{
    public class JsProxyBase
    {
        private const string CaptchaNotReady = "CAPCHA_NOT_READY";
        protected readonly BotBrowser Browser;

        public readonly Subject<string> CouponWindowResult = new();

        public readonly Subject<string> BrowserLoaded = new();

        public readonly Subject<string> LoggerEvents = new();

        public bool Logined { get; set; } = false;


        private RuCaptchaApi Captcha { get; }
        public JsProxyBase(string captcha, BotBrowser browser)
        {
            Captcha = new RuCaptchaApi(captcha);
            Browser = browser;
        }

        public void SetLogined(bool value)
        {
            Logined = value;
        }

        public bool GetLogined()
        {
            return Logined;
        }

        public async Task<bool> EasyLogin(string login, string password)
        {
            return await Browser.ExecuteAsync<bool>("easyLogin", login, password);
        }

        public void SetFakeProfile(string profile)
        {
            Browser.ExecuteScriptAsync("setFakeProfile", profile);
        }
        
        public async Task<bool> FindStakeAndClick(string betId)
        {
            return await Browser.ExecuteAsync<bool>("findStakeAndClick", betId);
        }
        public async Task<bool> SetStakeSum(decimal sum)
        {
            return await Browser.ExecuteAsync<bool>("setStakeSum", sum);
        }

        public async Task<bool> DoStake()
        {
            return await Browser.ExecuteAsync<bool>("doStake");
        }

        public async Task<bool> CheckWindowResult()
        {
            return await Browser.ExecuteAsync("checkWindowResult");
        }

        public async Task<bool> CheckWindowResultText()
        {
            return await Browser.ExecuteAsync<bool>("checkWindowResultTextSuccess");
        }

        public virtual async Task<bool> OpenUrl(OpenUrlParam param)
        {
            await Browser.LoadUrlAsync(param.Url);
            return true;
        }
        
        public void DomLoaded(string url)
        {
            BrowserLoaded.OnNext(url);
            Debug.WriteLine(url);
        }

        public void CouponWindowResultCallback(string couponName)
        {
            CouponWindowResult.OnNext(couponName);
            Debug.WriteLine(couponName);
        }

        public void Logi(string str)
        {
            Debug.WriteLine(str);
            LoggerEvents.OnNext(str);
        }

        public async Task<string> GetGoogleCapcha(string googleKey, string pageUrl)
        {
            var capchaId = await Captcha.StartResolveGoogleCaptcha(googleKey, pageUrl);
            
            var capchaResponse = CaptchaNotReady;
            while (capchaResponse.Contains(CaptchaNotReady))
            {
                var result = await Browser.Execute<bool>("isLogined");
                if (result)
                    break;
                capchaResponse = await Captcha.GetGoogleCaptchaResponse(capchaId);
                if (capchaResponse.Contains(CaptchaNotReady))
                {
                    Console.WriteLine("Capcha not resolve");
                    Debug.WriteLine("Capcha not resolve");
                    await Task.Delay(15000);
                }
            }
            
            return capchaResponse;
        }
        
        public void NotificateInBrowserHtml(string message)
        {
            Browser.GetMainFrame()
                .LoadHtml(message,true);
        }

        public void SendMouseMove(int x,int y)
        {
            var broHost = Browser.GetBrowserHost();
            broHost.SendMouseMoveEvent(new MouseEvent(x,y,CefEventFlags.None),true);
        }

        public void SendMouseWheel(int x,int y,int deltaX,int deltaY)
        {
            var broHost = Browser.GetBrowserHost();
            broHost.SendMouseWheelEvent(x,y,deltaX,deltaY,CefEventFlags.None);
        }

        public void SendMouseClick(int x,int y)
        {
            var broHost = Browser.GetBrowserHost();
            broHost.SendMouseClickEvent(x,y,MouseButtonType.Left,true,1,CefEventFlags.None);
        }

        public void SetFocus()
        {
            var broHost = Browser.GetBrowserHost();
            broHost.SetFocus(true);
        }

        public async Task<string> PrintSource()
        {
            var sourceCode = await Browser.GetSourceAsync();
            return sourceCode;
        }
    }
}