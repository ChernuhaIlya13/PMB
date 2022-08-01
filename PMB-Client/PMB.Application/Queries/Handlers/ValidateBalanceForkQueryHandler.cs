using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MediatR;
using PMB.Application.Models;
using PMB.Client;
using PMB.Domain.BrowserModels;
using PMB.Models.V1.Requests;

namespace PMB.Application.Queries.Handlers;

public record ValidateBalanceForkQuery(BetDecoratorSumma[] BetDecoratorsSumma) : IRequest<(StatusParseBalance StatusBalance,List<BetBalance> BetBalances)>;

[UsedImplicitly]
internal sealed class ValidateBalanceForkQueryHandler : IRequestHandler<ValidateBalanceForkQuery,(StatusParseBalance StatusBalance,List<BetBalance> BetBalances)>
{
        private PmbApiClient _client;
        private readonly Dictionary<List<string>, string> _parseToCharCode = new()
        {
            {new List<string>(){"$"},"USD"}, // доллар
            {new List<string>(){"€"},"EUR"}, // евро
            {new List<string>(){""},"UAH"},  // украинские гривны
            {new List<string>(){"£"},"GBP"}, // фунты стерлингов
            {new List<string>(){""},"PLN"},  // польский злотый
            {new List<string>(){""},"KZT"},  // казахстанские тенге
            {new List<string>(){""},"BYN"},  // белорусский рубль
            {new List<string>(){""},"AMD"},  // армянский драмы
            {new List<string>(){""},"AZN"},  // азербайджанский манат
            {new List<string>(){""},"MDL"},  // молдавских леев
            {new List<string>(){""},"CAD"},  // канадский доллар
            {new List<string>(){""},"SEK"},  // шведских крон
            {new List<string>(){""},"AUD"},  // австралийский доллар
            {new List<string>(){"₽"},"RUB"}  // рубли
        };
        private List<string> _charCodes
        {
            get
            {
                var charCodes = new List<string>()
                {
                    "руб"
                };
                foreach (var keyValuePair in _parseToCharCode)
                {
                    charCodes.Add(keyValuePair.Value);
                }
                return charCodes;
            }
        }

        public ValidateBalanceForkQueryHandler(PmbApiClient client)
        {
            _client = client;
        }

        public async Task<(StatusParseBalance StatusBalance,List<BetBalance> BetBalances)> Handle(ValidateBalanceForkQuery request,CancellationToken token)
        {
            var currenciesTask = _client.GetCurrencies(new CurrencyRatesRequest() {Date = DateTimeOffset.Now}, CancellationToken.None);

            var (firstSumFromRandom, (_, firstBkDecorator)) = request.BetDecoratorsSumma.First();
            var (secondSumFromRandom, (_, secondBkDecorator)) = request.BetDecoratorsSumma.Skip(1).First();

            var firstBalanceFromRandomToNativeBalance = 0m;
            var secondBalanceFromRandomToNativeBalance = 0m;
            var firstBalanceInRubles = 0m;
            var secondBalanceInRubles = 0m;

            string firstCurrency;
            string secondCurrency;

            var firstBalanceInfoFromBrowser = new BalanceInfo();
            var secondBalanceInfoFromBrowser = new BalanceInfo();
            
            try
            {
                firstBalanceInfoFromBrowser = await firstBkDecorator.GetBalanceInfo();
                secondBalanceInfoFromBrowser = await secondBkDecorator.GetBalanceInfo();
            }
            catch
            {
                return (StatusParseBalance.FailedParseBalance, null);
            }
            
            if(!_charCodes.Contains(firstBalanceInfoFromBrowser.Currency))
            {
                firstCurrency = _parseToCharCode.FirstOrDefault(charCode => charCode.Key.Contains(firstBalanceInfoFromBrowser.Currency)).Value ?? "";
            }
            else
            {
                firstCurrency = firstBalanceInfoFromBrowser.Currency;
            }
            
            if (string.IsNullOrWhiteSpace(firstBalanceInfoFromBrowser.Amount.ToString(CultureInfo.CurrentCulture)))
                return (StatusParseBalance.FailedParseBalance,null);

            if(!_charCodes.Contains(secondBalanceInfoFromBrowser.Currency))
            {
                secondCurrency = _parseToCharCode.FirstOrDefault(charCode => charCode.Key.Contains(secondBalanceInfoFromBrowser.Currency)).Value ?? "";
            }
            else
            {
                secondCurrency = secondBalanceInfoFromBrowser.Currency;
            }
            
            if (string.IsNullOrWhiteSpace(secondBalanceInfoFromBrowser.Amount.ToString(CultureInfo.CurrentCulture)))
                return (StatusParseBalance.FailedParseBalance,null);

            var currencies = await currenciesTask;
            currencies.CbrRates.ToList().ForEach(rate =>
            {
                if (rate.CharCode == firstCurrency)
                {
                    firstBalanceInRubles = (int) (rate.Value / rate.Nominal) * firstBalanceInfoFromBrowser.Amount;
                    firstBalanceFromRandomToNativeBalance = Math.Round(Convert.ToDecimal(firstSumFromRandom) / (rate.Value / rate.Nominal),2);
                }
                
                if (rate.CharCode == secondCurrency)
                {
                    secondBalanceInRubles = (int) (rate.Value / rate.Nominal) * secondBalanceInfoFromBrowser.Amount;
                    secondBalanceFromRandomToNativeBalance = Math.Round(Convert.ToDecimal(secondSumFromRandom) / (rate.Value / rate.Nominal),2);
                }
            });
            if (firstCurrency.ToLower() == "руб")
            {
                firstBalanceInRubles = firstBalanceInfoFromBrowser.Amount;
                firstBalanceFromRandomToNativeBalance = Convert.ToDecimal(firstSumFromRandom);
            }
            if (secondCurrency.ToLower() == "руб")
            {
                secondBalanceInRubles = secondBalanceInfoFromBrowser.Amount;
                secondBalanceFromRandomToNativeBalance = Convert.ToDecimal(secondSumFromRandom);
            }
            
            if (Convert.ToDecimal(firstSumFromRandom) > Convert.ToDecimal(firstBalanceInRubles) ||
                Convert.ToDecimal(secondSumFromRandom) > Convert.ToDecimal(secondBalanceInRubles))
            {
                return (StatusParseBalance.NotEnoughMoney,null);
            }
            
            var betBalance = new List<BetBalance>
            {
                new(firstBalanceFromRandomToNativeBalance,firstBkDecorator.BookmakerName),
                new(secondBalanceFromRandomToNativeBalance,secondBkDecorator.BookmakerName),
            };
            
            return (StatusParseBalance.Success,betBalance);
        }
}