using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PMB.Browsers.Common;
using PMB.Pinnacle.Models;
using PriceInfo = PMB.Browsers.Common.PriceInfo;

namespace PMB.Pinnacle.Helpers
{
    public static class BetExtensions
    {
        private static readonly IReadOnlyDictionary<string, string> MarketBetTypeMapper = new Dictionary<string, string>
        {
            {"21", "home"}, { "22", "home"}, 
            {"23", "away"}, { "24", "away"}
        };

        private static readonly IReadOnlyDictionary<string, string> DesignationMapper = new Dictionary<string, string>
        {
            { "1", "home" }, { "2", "away" },
            { "17", "home" }, { "18", "away" },
            { "19", "over" }, { "20", "under" },
            { "21", "over" }, { "22", "under" },
            { "23", "over" }, { "24", "under" }
        };
        
        private static readonly IReadOnlyDictionary<string, string> BetTypeMapper = new Dictionary<string, string>
        {
            { "T", "total" },
            { "I", "team_total" },
            { "M", "moneyline" },
            { "S", "spread" }
        };

        private static readonly IReadOnlyDictionary<string, string> BetTypeMinMapper = new Dictionary<string, string>
        {
            { "total", "ou" },
            { "team_total", "tt" },
            { "moneyline", "m" },
            { "spread", "s" }
        };

        public static (string MarketKey, string Designation) ResolvePinnacleMarketKey(string directLink, string marketAndBetType)
        {
            long eventId = 0;
            var betType = "";
            var betDataParts = new string[0];
            var betData = new string[0];
            var lineData = "";
            long periodNum = 0;
            //1495578827/2/3/4/1/27.5/4.090
            //1495578827/2/2/1/1/-3.5/3.790 (Фора -3.5)
            var parts = directLink.Split("|");//1495578827/2/2/1/1/4.5/2.000
            if (parts.Length == 1)
            {
                var partitions = parts.First().Split("/");
                betData = partitions.Skip(1).ToArray();
                periodNum = betData.First().ToInt();
                betType = BetTypeMapper["M"];
                lineData = betData[^2];//ЗАБЫЛ КАК ЭТО РАБОТАЕТ
            }
            else
            {
                betType = BetTypeMapper[parts.Skip(2).First()];
                betDataParts = parts.Skip(3).First().Split("?");
                betData = betDataParts.First().Split("/");
                lineData = betDataParts.Skip(1).First().Split("&").First().Split("=").Last();
                periodNum = betData.First().ToInt();
            }


            var sb = new StringBuilder($"s;{periodNum};{BetTypeMinMapper[betType]}");

            var designation = DesignationMapper[marketAndBetType];
            
            if (betType == "moneyline")
            {
                return (sb.ToString(), designation);
            }
            //TODO: В javascript'е происходит округление до 1 знака после запятой,нужно проверить
            // function getMarketKey(betType, period, line, teamType) {
            //     var key = 's;' + String(period) + ";" + convertToMinBetType(betType);
            //     if (betType === 'moneyline') {
            //         return key;
            //     }
            //     if (line != null) {
            //         key += ';' + parseFloat(line).toFixed(1);
            //     }
            //     if (betType !== 'team_total') {
            //         return key;
            //     }
            //     return key + ';' + teamType;
            // }
            sb.Append($";{lineData}");

            //TODO: разобраться со сранным ABB
            if (betType == "team_total")
            {
                sb.Append($";{MarketBetTypeMapper[marketAndBetType]}");
            }
            
            return (sb.ToString(), designation);
        }

        public static MatchupPricesModel ResolveMathupAndPrice(StraightMarketsResult[] straightMarkets,string marketKey,string designation)
        {
            var straight = straightMarkets?.FirstOrDefault(x =>
                x.Key.StartsWith(marketKey) && x.Prices.Any(y =>
                    y.Designation?.Equals(designation, StringComparison.InvariantCultureIgnoreCase) ?? true));

            if (straight == null)
            {
                return null;
            }
            
            var matchUp = straight!.MatchupId;
            var price = straight.Prices.FirstOrDefault(x =>
                 (bool) x.Designation?.Equals(designation, StringComparison.InvariantCultureIgnoreCase));
            
            return new MatchupPricesModel(matchUp,price);
        }
    }
}