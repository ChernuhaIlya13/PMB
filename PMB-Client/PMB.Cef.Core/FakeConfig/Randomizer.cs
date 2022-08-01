using System;
using System.Collections.Generic;

namespace PMB.Cef.Core.FakeConfig
{
    public static class Randomizer
    {
        private static readonly Random Rnd = new();
        
        public static int Between(int minimumValue, int maximumValue)
        {
            var data = Rnd.Next(-129, 128);
            var num = Math.Floor(Math.Max(0.0, Convert.ToDouble(data) / byte.MaxValue - 1E-11) * (maximumValue - minimumValue + 1));
            return (int) (minimumValue + num);
        }

        public static T GetRandValue<T>(this IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list == null");
            
            if (list.Count == 1)
                return list[0];
            var value = Rnd.Next(0,list.Count - 1);
            return list[value];
        }
    }
}