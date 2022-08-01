using System;
using PMB.Models.PositiveModels;

namespace PMB.Domain.ForkModels
{
    public class BetMain
    {
        public string Id { get; set; }

        public int BookmakerId { get; set; }
        
        public string Bookmaker { get; set; }

        public Decimal Coefficient { get; set; }

        public Direction Direction { get; set; }

        public string Period { get; set; }
        
        public string BetType { get; set; }

        public string BetId { get; set; }

        public string Sport { get; set; }

        public double Parameter { get; set; }

        public string BetValue { get; set; }

        public int ForksCount { get; set; }

        public string PositiveEvId { get; set; }

        public string EvId { get; set; }

        public string OtherData { get; set; }

        public string Teams { get; set; }
        
        public int SportId { get; set; }

        public MatchDataInfo MatchData { get; set; }
        
        public string EventName { get; set; }

        /// <summary>
        /// Для AllBestBets 111960906C18A_1_9
        /// Для Positivebet https://www.ligastavok.ru/bets/ ...
        /// </summary>
        public string Url { get; set; }

        public bool IsReq { get; set; }

        public bool IsInitiator { get; set; }
        
        public string MarketAndBetType { get; set; }
        
        public float MarketAndBetTypeParam { get; set; }

        public class MatchDataInfo
        {
            public string Liga { get; set; }

            public string Score { get; set; }

            public string[] PreviousScores { get; set; }

            public string AdditionalData { get; set; }

            public string Time { get; set; }
        }
        
        public int Lifetime { get; set; }
        
        public bool SwapTeams { get; set; }
    }
}