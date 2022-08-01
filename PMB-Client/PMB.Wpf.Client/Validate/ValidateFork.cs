using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PMB.Cef.Core.JsProxy;
using PMB.Domain.BrowserModels;
using PMB.Models.PositiveModels;
using PMB.Wpf.Client.Models;

namespace PMB.Wpf.Client.Validate
{
    public record MaxMinTime(TimeSpan maxValue, TimeSpan minValue);
    
    public static class ValidateFork
    {
        public static async Task<StatusParseMinutes> ValidateMinutes(ParamsDecorator firstBrowserParamsDecorator, ParamsDecorator secondBrowserParamsDecorator)
        {
            try
            {
                var timeFirstBookmaker = await GetParsedTimeFromBookmaker(firstBrowserParamsDecorator);
                if (!timeFirstBookmaker.HasValue || timeFirstBookmaker.Value == TimeSpan.Zero)
                    return StatusParseMinutes.TimeFailedParse;
            
                var timeSecondBookmaker = await GetParsedTimeFromBookmaker(secondBrowserParamsDecorator);
                if (!timeSecondBookmaker.HasValue || timeSecondBookmaker.Value == TimeSpan.Zero)
                    return StatusParseMinutes.TimeFailedParse;

                var maxMinTime = timeFirstBookmaker > timeSecondBookmaker ? new MaxMinTime(timeFirstBookmaker.Value,timeSecondBookmaker.Value) 
                    : new MaxMinTime(timeSecondBookmaker.Value,timeFirstBookmaker.Value);

                if (maxMinTime.maxValue - maxMinTime.minValue < TimeSpan.FromMinutes(2))
                {
                    return StatusParseMinutes.TimeMatch;
                }
                
                return StatusParseMinutes.TimeNotMatch;

            }
            catch (Exception)
            {
                Debug.WriteLine("Ошибка в ValidateMinutes");
                return StatusParseMinutes.UnknownFail;
            }
        }

        private static bool IsValidSportForTime(Fork fork)
        {
            return fork.Sport == Sport.Football || fork.Sport == Sport.Futsal || fork.Sport == Sport.Handball ||
                   fork.Sport == Sport.Hockey || fork.Sport == Sport.Basketball;
        }
        private static async Task<TimeSpan?> GetParsedTimeFromBookmaker(ParamsDecorator paramsDecorator)
        {
            MatchData matchData = new();
            try
            {
                matchData =
                    await paramsDecorator.decorator.GetMatchData(new MatchDataParam(paramsDecorator.eventId));
            }
            catch
            {
                return null;
            }

            var parsedTimeFromBookmaker = FetchMinutesFromBookmaker(matchData.Time.MainTime);
            return parsedTimeFromBookmaker;
        }
        
        private static TimeSpan? FetchMinutesFromBookmaker(string rawTime)
        {
            if (string.IsNullOrWhiteSpace(rawTime))
            {
                return null;
            }

            if (rawTime.Contains(":", StringComparison.OrdinalIgnoreCase))
            {
                var minutesMarathonBk = Convert.ToInt32(rawTime.Split(":").First());
                var secondsMarathonBk = Convert.ToInt32(rawTime.Split(":").Last());
                return minutesMarathonBk >= 60 ? new TimeSpan(minutesMarathonBk / 60, minutesMarathonBk % 60, secondsMarathonBk) : new TimeSpan(0, minutesMarathonBk % 60, secondsMarathonBk);
            }
            if (Int32.TryParse(rawTime,out var digit))
            {
                return digit >= 60 ? new TimeSpan(digit / 60, digit % 60, 0) : new TimeSpan(0, digit % 60, 0);
            }
            return null;
        }
        private static TimeSpan? FetchMinutesFromForkToTimeSpan(string time,Sport sport)
        {
            if (string.IsNullOrWhiteSpace(time))
            {
                return null;
            }

            if (ContainsMinutes(time))
            {
                var intMinutes = Convert.ToInt32(time.Split(" ").First());
                return intMinutes >= 60
                    ? new TimeSpan(intMinutes / 60, intMinutes % 60, 0)
                    : new TimeSpan(0, intMinutes % 60, 0);
            }

            if (time.Contains(":"))
            {
                var minutes = Convert.ToInt32(time.Split(":").First());
                var seconds = Convert.ToInt32(time.Split(":").Last());
                return minutes >= 60
                    ? new TimeSpan(minutes / 60, minutes % 60, seconds)
                    : new TimeSpan(0, minutes % 60, seconds);
            }
            return null;
        }
        
        private static bool ContainsMinutes(string value) => value.Contains("min");
    }
}