using System.Collections.Generic;

namespace PMB.Application;

public static class FilterCountStakes
{
    public static Dictionary<string,int> CountForksInOneEvent = new ();

    public static Dictionary<string, Dictionary<string, int>> CountStakesInOneEvent = new();

    public static Dictionary<string, int> CountNotOverlappedStakes = new();

    public static void IncrementCountForksInOneEvent(string firstNameEvent, string secondNameEvent)
    {
        var eventNames = firstNameEvent + "|" + secondNameEvent;
        var result = CountNotOverlappedStakes.GetValueOrDefault(eventNames, -1);
        if (result >= 0)
        {
            CountForksInOneEvent[eventNames]++;
        }
        else
        {
            CountForksInOneEvent.Add(eventNames, 1);
        }
        
    }

    public static void IncrementCountStakesInOneEvent(string firstNameEvent, string secondNameEvent, string firstBetValue, string secondBetValue)
    {
        var eventNames = firstNameEvent + "|" + secondNameEvent;
        var betValues = firstBetValue + "|" + secondBetValue;
        var result = CountStakesInOneEvent.GetValueOrDefault(eventNames, null);
        if (result != null)
        {
            if (result.GetValueOrDefault(betValues, -1) >= 0)
            {
                CountStakesInOneEvent[eventNames][betValues]++;
            }
            else
            {
                CountStakesInOneEvent[eventNames].Add(betValues, 1);
            }
        }
        else
        {
            CountStakesInOneEvent.Add(eventNames, new Dictionary<string, int>()
            {
                {
                    betValues,
                    1
                }
            });
        }
    }

    public static void IncrementCountNotOverlappedStakes(string firstNameEvent, string secondNameEvent)
    {
        var eventNames = firstNameEvent + "|" + secondNameEvent;
        var result = CountNotOverlappedStakes.GetValueOrDefault(eventNames, -1);
        if (result >= 0)
        {
            CountNotOverlappedStakes[eventNames]++;
        }
        else
        {
            CountNotOverlappedStakes.Add(eventNames, 1);
        }
    }
    

    public static (string, string) TwinString(string str1, string str2)
    {
        return ($"{str1}|{str2}",$"{str2}|{str1}");
    }
}