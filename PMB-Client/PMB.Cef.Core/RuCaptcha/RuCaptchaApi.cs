using System;
using System.Threading.Tasks;

namespace PMB.Cef.Core.RuCaptcha
{
    public class RuCaptchaApi
    {
        private readonly string _apiKey;
        private readonly RuCaptchaClient _captchaClient;

        public RuCaptchaApi(string apiKey)
        {
            _apiKey = apiKey;
            if (string.IsNullOrWhiteSpace(_apiKey))
                return;
            
            _captchaClient = new RuCaptchaClient(_apiKey);
        }

        public async Task<string> StartResolveGoogleCaptcha(string googleKey, string pageUrl)
        {
            if (string.IsNullOrWhiteSpace(googleKey))
                throw new ArgumentException(googleKey + " не может быть пустым!");
            if (string.IsNullOrWhiteSpace(pageUrl))
                throw new ArgumentException(pageUrl + " не может быть пустым!");
            return await _captchaClient.StartResolveGoogleCaptchaV2(googleKey, pageUrl);
        }

        public async Task<string> GetGoogleCaptchaResponse(string taskId)
        {
            return await _captchaClient.GetCaptcha(taskId);
        }
    }
}
