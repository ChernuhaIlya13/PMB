using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PMB.Domain.BrowserModels;
using PMB.Pinnacle.Models;

namespace PMB.Pinnacle
{
    public class BetStraightResultAlternative
    {
        public Guid RequestId { get; set; }
            
        public string Status { get; set; }
    }
    public class PinnacleClient
    {
        private static readonly JsonSerializerSettings DefaultSerializeSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };

        private const string ApiUrl = "https://api.arcadia";

        private readonly HttpClient _client;

        public PinnacleClient(AuthInfo authInfo, Proxy proxy, Uri bookmakerUrl)
        {
            var handler = proxy.ResolveHandler();

            _client = handler != null ? new HttpClient(handler) : new HttpClient();
            
            _client.BaseAddress = new Uri(string.Join(".", bookmakerUrl.Host.Split(".").Skip(1).Prepend(ApiUrl)));

            authInfo.ResolveHeaders()
                .ForEach(header => _client.DefaultRequestHeaders.Add(header.Key, header.Value));
        }
        
        public async Task<RelatedMatchUpsResult[]> GetRelatedMatchUps(string eventId)
        {
            RelatedMatchUpsResult[] result = new RelatedMatchUpsResult[0];
            try
            {
                result = await _client.GetFromJsonAsync<RelatedMatchUpsResult[]>($"/0.1/matchups/{eventId}/related");
            }
            catch
            {
                //ignore
            }
            return result;
        }

        public async Task<StraightMarketsResult[]> GetStraightMarkets(string eventId)
        {
            var marketsFirst = await _client.GetFromJsonAsync<StraightMarketsResult[]>($"/0.1/matchups/{eventId}/markets/straight?primaryOnly=false");
            var marketsSecond =
                await _client.GetFromJsonAsync<StraightMarketsResult[]>(
                    $"/0.1/matchups/{eventId}/markets/related/straight?primaryOnly=false");
            var markets = new List<StraightMarketsResult>();
            AddButchMarkets(markets, marketsFirst);
            AddButchMarkets(markets, marketsSecond);
            
            return markets.ToArray();
         }

        public void AddButchMarkets(List<StraightMarketsResult> markets, StraightMarketsResult[] insertMarkets)
        {
            for (int i = 0; i < insertMarkets.Length; i++)
            {
                markets.Add(insertMarkets[i]);
            }
        }


        public async Task<BetStraightResult> BetStraight(BetStraightModel model)
        {
            var response = await _client.PostAsync("0.1/bets/straight",
                new StringContent(JsonConvert.SerializeObject(model, DefaultSerializeSettings), Encoding.UTF8,
                    "application/json"));
            var stringResult = await response.Content.ReadAsStringAsync();
            BetStraightResult finishResponse = null;
            try
            {
                finishResponse = JsonConvert.DeserializeObject<BetStraightResult>(stringResult);
            }
            catch
            {
                var alternative = JsonConvert.DeserializeObject<BetStraightResultAlternative>(stringResult);
                finishResponse = new()
                {
                    RequestId = alternative.RequestId,
                    Status = alternative.Status
                };
            }

            return finishResponse;
        }

        public async Task<RelatedMatchUpsResult> GetEventInfo(string eventId)
        {
            var response = await _client.GetAsync($"/0.1/matchups/{eventId}");
            return await response.Content.ReadFromJsonAsync<RelatedMatchUpsResult>();
        }

        public async Task<BalanceInfo> GetBalance()
        {
            var response = await _client.GetAsync("/0.1/wallet/balance");
            var strResult = await response.Content.ReadAsStringAsync();
            return await response.Content.ReadFromJsonAsync<BalanceInfo>();
        }

        public async Task<StraightQuoteModelResult> GetStakeInfo(StraightQuoteModel request)
        {
            var response = await _client.PostAsync("/0.1/bets/straight/quote",new StringContent(JsonConvert.SerializeObject(request, DefaultSerializeSettings),Encoding.UTF8, "application/json"));
            return await response.Content.ReadFromJsonAsync<StraightQuoteModelResult>();
        }

        public async Task<StraightMarketsResult> FindMarket(string eventId,string marketKey)
        {
            var response = await _client.GetAsync($"/0.1/matchups/{eventId}/market/{marketKey}");
            return await response.Content.ReadFromJsonAsync<StraightMarketsResult>();
        }

        /// <summary>
        /// Статус "pending" - в ожидание заключение пари
        /// Статус "unsettled" - пари заключено
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public async Task<StakeInfoResult> GetPuttedStakeInfo(Guid requestId)
        {
            var response = await _client.GetAsync($"0.1/bets/pending/{requestId.ToString()}");
            var stringResult = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<StakeInfoResult>(stringResult);
            return result;
        }
        
    }
}