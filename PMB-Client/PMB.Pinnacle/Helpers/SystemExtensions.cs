using System;

namespace PMB.Pinnacle.Helpers
{
    public static class SystemExtensions
    {
        public static long ToLong(this string str) => Int64.Parse(str);
        
        public static long ToInt(this string str) => Int32.Parse(str);
        
        public static decimal ToDecimal(this string str) => decimal.Parse(str);
    }
}