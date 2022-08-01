using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PMB.Cef.Core.RuCaptcha
{
    public class RuCaptchaClient
    {
        private readonly string _apiKey;
        private static Dictionary<string, string> _errors;
        private static readonly Uri Host = new("https://rucaptcha.com/");

        public RuCaptchaClient(string apiKey)
        {
            _apiKey = apiKey;
            if (_errors != null)
                return;
            
            _errors = new Dictionary<string, string>
            {
                {
                    "CAPCHA_NOT_READY",
                    "Капча в работе, ещё не расшифрована, необходимо повтороить запрос через несколько секунд."
                },
                { "ERROR_WRONG_ID_FORMAT", "Неверный формат ID капчи. ID должен содержать только цифры." },
                { "ERROR_WRONG_CAPTCHA_ID", "Неверное значение ID капчи." },
                { "ERROR_CAPTCHA_UNSOLVABLE", "Капчу не смогли разгадать 3 разных работника. Средства за эту капчу не списываются." },
                { "ERROR_WRONG_USER_KEY", "Не верный формат параметра key, должно быть 32 символа." },
                { "ERROR_KEY_DOES_NOT_EXIST", "Использован несуществующий key." },
                { "ERROR_ZERO_BALANCE", "Баланс Вашего аккаунта нулевой." },
                { "ERROR_NO_SLOT_AVAILABLE", "Текущая ставка распознования выше, чем максимально установленная в настройках Вашего аккаунта." },
                { "ERROR_ZERO_CAPTCHA_FILESIZE", "Размер капчи меньше 100 Байт." },
                { "ERROR_TOO_BIG_CAPTCHA_FILESIZE", "Размер капчи более 100 КБайт." },
                { "ERROR_WRONG_FILE_EXTENSION", "Ваша капча имеет неверное расширение, допустимые расширения jpg,jpeg,gif,png." },
                { "ERROR_IMAGE_TYPE_NOT_SUPPORTED", "Сервер не может определить тип файла капчи." },
                { "ERROR_IP_NOT_ALLOWED", "В Вашем аккаунте настроено ограничения по IP с которых можно делать запросы. И IP, с которого пришёл данный запрос не входит в список разрешённых." }
            };
        }
        

        public async Task<string> GetCaptcha(string captchaId)
        {
            return await MakeGetRequest("http://rucaptcha.com/res.php?key=" + _apiKey + "&action=get&id=" + captchaId);
        }
        
        public async Task<string> StartResolveGoogleCaptchaV2(string googleKey, string pageUrl)
        {
            return await MakeGetRequest("http://rucaptcha.com/in.php?key=" + _apiKey + "&method=userrecaptcha&googlekey=" + googleKey + "&pageurl=" + pageUrl);
        }

        private async Task<string> MakeGetRequest(string endpoint)
        {
            var client = new HttpClient
            {
                BaseAddress = Host
            };

            var response = await client.GetAsync(endpoint);
            var serviceAnswer = await response.Content.ReadAsStringAsync();
            return ParseAnswer(serviceAnswer);
        }

        private string ParseAnswer(string serviceAnswer)
        {
            if (RuCaptchaClient._errors.Keys.Contains<string>(serviceAnswer))
                return RuCaptchaClient._errors[serviceAnswer] + " (" + serviceAnswer + ")" + "error";
            if (serviceAnswer.StartsWith("OK|"))
                return serviceAnswer.Substring(3);
            return serviceAnswer;
        }
    }
}
