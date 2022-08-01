using System;

namespace PMB.Pinnacle.Models
{
    public class BetStraightResult
    {
        public long Id { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
        
        public string OddsFormat { get; set; }
        
        public decimal Price { get; set; }
        
        public string Status { get; set; }

        public Guid RequestId { get; set; }
        
        public decimal Stake { get; set; }
        
        public string Type { get; set; }
        
        public object PrecisePrice { get; set; }
        
        public SelectionModel Selections { get; set; }
    }
}