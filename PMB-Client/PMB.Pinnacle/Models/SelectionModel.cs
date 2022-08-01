namespace PMB.Pinnacle.Models
{
    public class SelectionModel
    {
        /// <summary>
        /// 1480952682
        /// </summary>
        public long MatchupId { get; set; }

        /// <summary>
        /// s;0;tt;1.5;home
        /// </summary>
        public string MarketKey { get; set; }

        /// <summary>
        /// over
        /// </summary>
        public string Designation { get; set; }

        /// <summary>
        /// Коэффициент ставки на событии(1.943)
        /// </summary>
        public decimal Price { get; set; }
        
        //public float Points { get; set; }

    }

    public class SelectionModelExtended : SelectionModel
    {
        public object MarketAltId { get; set; }
        
        public long MarketId { get; set; }
    }
}