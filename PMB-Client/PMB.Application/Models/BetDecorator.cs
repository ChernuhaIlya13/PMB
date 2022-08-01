using PMB.Browsers.Common;
using PMB.Domain.ForkModels;

namespace PMB.Application.Models;

public record BetDecorator(BetMain BetMain, IBotBrowserDecorator Decorator);

public record BetDecoratorSumma(string Summa, BetDecorator BetDecorator);

public record BetBalance(decimal SumOfStake, string BookmakerName);
