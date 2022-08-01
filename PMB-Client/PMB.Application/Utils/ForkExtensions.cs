using System;
using System.Linq;
using PMB.Abb.Models.Models;
using PMB.Domain.ForkModels;
using PMB.Models.PositiveModels;

namespace PMB.Application.Utils;

public static class ForkExtensions
{
    private static (string score, string[] previousScores) ParseMainScoreAndPreviousScores(string currentScore)
    {
        if (string.IsNullOrWhiteSpace(currentScore))
        {
            return (string.Empty, Array.Empty<string>());
        }

        string mainScoreFirst;
        string[] previousScores;
        if (currentScore.Contains("("))
        {
            mainScoreFirst = currentScore[..currentScore.IndexOf("(", StringComparison.InvariantCultureIgnoreCase)];
            previousScores = currentScore[currentScore.IndexOf("(", StringComparison.InvariantCultureIgnoreCase)..]
                .Replace(")", "").Split(" ");
        }
        else
        {
            mainScoreFirst = currentScore;
            previousScores = Array.Empty<string>();
        }

        return (mainScoreFirst, previousScores);
    }

    public static ForkMain Convert(this AbbFork value)
    {
        var sportStr = Enum.TryParse<AbbSport>(value.AbbDto.SportId.ToString(), out var sport)
            ? sport.ToString()
            : AbbSport.None.ToString();
        var (scoreFBet, previousScoresFBet) = ParseMainScoreAndPreviousScores(value.Bets[0].CurrentScore);
        var (scoreSBet, previousScoresSBet) = ParseMainScoreAndPreviousScores(value.Bets[1].CurrentScore);

        var betValueFirst = AbbBetValue.BetValues[value.Bets[0].MarketAndBetType];
        var betValueSecond = AbbBetValue.BetValues[value.Bets[1].MarketAndBetType];
        return new ForkMain
        {
            Id = value.AbbDto.Id,
            Lifetime = (int)(DateTimeOffset.Now - value.AbbDto.CreatedAt).TotalSeconds,
            Profit = (decimal)value.AbbDto.Percent,
            Sport = sportStr,
            SportId = value.AbbDto.SportId,
            Away = value.AbbDto.Away,
            Home = value.AbbDto.Home,
            FirstBet = new BetMain
            {
                BookmakerId = value.Bets[0].BookmakerId,
                Bookmaker = Enum.TryParse<AbbBookmaker>(value.Bets[0].BookmakerId.ToString(), out var bookmakerFirst)
                    ? bookmakerFirst.ToString()
                    : AbbBookmaker.None.ToString(),
                Coefficient = (decimal)value.Bets[0].Coefficient,
                Direction = value.Bets[0].Diff == 0 ? Direction.Freeze :
                    (int)value.Bets[0].Diff == 1 ? Direction.Up : Direction.Down,
                Id = value.Bets[0].Id,
                Sport = sportStr,
                Teams = value.Bets[0].Home + "|" + value.Bets[0].Away,
                BetId = value.Bets[0].DirectLink,
                BetValue = betValueFirst,
                BetType = string.Empty.ConvertToBetType(),
                Period = AbbPeriod.Periods[value.Bets[0].PeriodId],
                EvId = value.Bets[0].BookmakerEventDirectLink,
                MatchData = new BetMain.MatchDataInfo
                {
                    Liga = value.Bets[0].League,
                    Score = scoreFBet,
                    Time = string.Empty,
                    AdditionalData = string.Empty,
                    PreviousScores = previousScoresFBet
                },
                Url = value.Bets[0].BookmakerEventDirectLink,
                MarketAndBetType = value.Bets[0].MarketAndBetType.ToString(),
                SportId = value.AbbDto.SportId,
                MarketAndBetTypeParam = value.Bets[0].MarketAndBetTypeParam,
                EventName = value.Bets[0].EventName
            },
            SecondBet = new BetMain
            {
                BookmakerId = value.Bets[1].BookmakerId,
                Bookmaker = Enum.TryParse<AbbBookmaker>(value.Bets[1].BookmakerId.ToString(), out var bookmakerSecond)
                    ? bookmakerSecond.ToString()
                    : AbbBookmaker.None.ToString(),
                Coefficient = (decimal)value.Bets[1].Coefficient,
                Direction = value.Bets[1].Diff == 0 ? Direction.Freeze :
                    (int)value.Bets[1].Diff == 1 ? Direction.Up : Direction.Down,
                Id = value.Bets[1].Id,
                Sport = sportStr,
                Teams = value.Bets[1].Home + "|" + value.Bets[1].Away,
                BetId = value.Bets[1].DirectLink,
                BetValue = betValueSecond,
                Period = AbbPeriod.Periods[value.Bets[1].PeriodId],
                EvId = value.Bets[1].BookmakerEventDirectLink,
                BetType = string.Empty.ConvertToBetType(),
                MatchData = new BetMain.MatchDataInfo
                {
                    Liga = value.Bets[1].League,
                    Score = scoreSBet,
                    Time = string.Empty,
                    AdditionalData = string.Empty,
                    PreviousScores = previousScoresSBet
                },
                Url = value.Bets[1].BookmakerEventDirectLink,
                MarketAndBetType = value.Bets[1].MarketAndBetType.ToString(),
                SportId = value.AbbDto.SportId,
                MarketAndBetTypeParam = value.Bets[1].MarketAndBetTypeParam,
                EventName = value.Bets[1].EventName
            },
            ForkScanner = ForkScannerType.AllBestBets,
        };
    }

    public static T ParseEnum<T>(int position, T defaultValue)
        where T: Enum
    {
        return Enum.IsDefined(typeof(T), position) ? (T)Enum.ToObject(typeof(T), position) : defaultValue;
    }
    
    public static TimeSpan? FetchMinutesFromBookmaker(string rawTime)
    {
        if (string.IsNullOrWhiteSpace(rawTime))
        {
            return null;
        }

        if (rawTime.Contains(":", StringComparison.OrdinalIgnoreCase))
        {
            var splittedTimes = rawTime.Split(":");
            var minutesMarathonBk = Int32.Parse(splittedTimes.First());
            var secondsMarathonBk = Int32.Parse(splittedTimes.Last());
            return minutesMarathonBk >= 60 ? new TimeSpan(minutesMarathonBk / 60, minutesMarathonBk % 60, secondsMarathonBk) : new TimeSpan(0, minutesMarathonBk % 60, secondsMarathonBk);
        }
        if (Int32.TryParse(rawTime,out var digit))
        {
            return digit >= 60 ? new TimeSpan(digit / 60, digit % 60, 0) : new TimeSpan(0, digit % 60, 0);
        }
        return null;
    }
}