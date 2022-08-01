using System;

namespace PMB.Domain.ForkModels
{
    public class ForkMain 
    {
        public string Id { get; set; }

        public long ForkId { get; set; }

        public int UpdateCount { get; set; }

        public int Lifetime { get; set; }

        public Decimal Profit { get; set; }

        public string Sport { get; set; }
        
        public int SportId { get; set; }
        
        public string Away { get; set; }
        
        public string Home { get; set; }

        public string BetType => this.FirstBet.BetType.ToString();

        public string Bookmakers
        {
            get
            {
                var bookmaker = this.FirstBet.Bookmaker;
                string str1 = bookmaker;
                bookmaker = this.SecondBet.Bookmaker;
                string str2 = bookmaker;
                return str1 + "|" + str2;
            }
        }

        public string Teams => this.FirstBet.Teams + "|" + this.SecondBet.Teams;

        public string Other { get; set; }

        public DateTimeOffset CreatedAt => DateTimeOffset.Now;

        public BetMain FirstBet { get; set; }

        public BetMain SecondBet { get; set; }

        public string CridId => this.K1 + this.K2;

        public string K1 { get; set; }

        public string K2 { get; set; }

        public string Elid { get; set; }
        
        public ForkScannerType ForkScanner { get; set; }
    }
}