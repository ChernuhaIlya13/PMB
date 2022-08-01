using System.Collections.Generic;

namespace PMB.Pinnacle.Models
{
    public class AuthInfo
    {
        public string XApiKey { get; set; }
        
        public string XSession { get; set; }
        
        public string XDeviceUuid { get; set; }
    }

    public static class AuthInfoExtensions
    {
        public static List<(string Key, string Value)> ResolveHeaders(this AuthInfo info)
        {
            return new List<(string, string)>
            {
                ( "x-api-key", info?.XApiKey ),
                ( "x-session", info?.XSession ),
                ( "x-device-uuid", info?.XDeviceUuid )
            };
        }
    }
}