using System.Threading.Tasks;
using PMB.Cef.Core;
using PMB.Cef.Core.JsProxy;

namespace PMB.Bet365
{
    public class JsProxyBet365 : JsProxyBase
    {
        public JsProxyBet365(string captcha, BotBrowser browser) 
            : base(captcha, browser) { }
        
        public override async Task<bool> OpenUrl(OpenUrlParam param)
        {
            return await Browser.ExecuteAsync<bool>("openEvent", param.Url, param.FirstTeam, param.SecondTeam, param.SportId);
        }

        public async Task<bool> CheckSpinnerContainerOnPage()
        {
            return await Browser.Execute<bool>("checkSpinnerContainerOnPage");
        }
    }
}