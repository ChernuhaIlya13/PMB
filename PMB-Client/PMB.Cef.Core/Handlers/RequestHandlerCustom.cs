using CefSharp;

namespace PMB.Cef.Core.Handlers
{
    public class RequestHandlerCustom : CefSharp.Handler.RequestHandler
    {
        public string Login { get; set; }
        
        public string Password { get; set; }

        public RequestHandlerCustom(string login,string password)
        {
            Login = login;
            Password = password;
        }
        protected override bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            if(isProxy)
            {
                using (callback)
                {
                    callback.Continue(username: Login, password: Password);
                }

                return true;
            }
            else
            {
                chromiumWebBrowser.LoadHtml("<h1>Нет установлены авторизационные данные для прокси</h1>" +
                                            "<h1>Сделайте перезапуск бота и проверьте работоспособность прокси</h1>");
            }

            return false;
        }
        
    }
}