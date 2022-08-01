namespace PMB.Pinnacle.Models
{
    public class BetStraightModel
    {
        public bool AcceptBetterPrice { get; set; }
        
        public bool AcceptBetterPrices { get; set; }
        
        public string Class { get; set; }
        
        public string OddsFormat { get; set; }
        
        public string OriginTag { get; set; }
        
        /// <summary>
        /// Сумма ставки в долларах
        /// </summary>
        public decimal Stake { get; set; }
        
        public SelectionModel[] Selections { get; set; }
    }
}