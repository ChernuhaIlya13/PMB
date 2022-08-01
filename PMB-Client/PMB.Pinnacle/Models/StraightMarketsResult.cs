using System;
using System.Collections.Generic;

namespace PMB.Pinnacle.Models
{
    public class Limit
    {
        public decimal Amount { get; set; }
        
        public string Type { get; set; }
    }

    public class PriceInfo
    {
        public string Designation { get; set; }
        
        public int Price { get; set; }
    }

    public class StraightMarketsResult
    {
        public DateTime CutoffAt { get; set; }
        
        public bool IsAlternate { get; set; }
        
        public string Key { get; set; }
        
        public List<Limit> Limits { get; set; }
        
        public int MatchupId { get; set; }
        
        public string Side { get; set; }
        
        public int Period { get; set; }
        
        public List<PriceInfo> Prices { get; set; }
        
        public string Type { get; set; }
        
        public int Version { get; set; }
    }
}