using System.Net;
using System.Net.Http;

namespace PMB.Domain.BrowserModels
{
    public class Proxy
    {
        public bool UseProxy { get; set; }
        
        public Schemes Schema { get; set; }
        
        public string IpAdress { get; set; }
        
        public int Port { get; set; }
        
        public bool NeedAuthProxy { get; set; }
        
        public string Login { get; set; }
        
        public string Password { get; set; }
    }

    public static class ProxyExtensions
    {
        public static HttpClientHandler ResolveHandler(this Proxy proxy)
        {
            HttpClientHandler handler = null;
            
            if (proxy.UseProxy)
            {
                handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(proxy.IpAdress, proxy.Port)
                };

                if (proxy.NeedAuthProxy)
                {
                    handler.Proxy.Credentials = new NetworkCredential
                    {
                        UserName = proxy.Login,
                        Password = proxy.Password
                    };
                }
            }

            return handler;
        }

        public static string ToProxyString(this Proxy proxy) =>
            $"{proxy.Schema.ToString().ToLower()}://{proxy.IpAdress}:{proxy.Port}";
    }
}