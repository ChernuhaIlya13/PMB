using System;

namespace PMB.Domain;

public class StatisticModel
{
    public TimeSpan TimeOfCurrentSession { get; set; }
        
    public int ReceivedForks { get; set; }

    public int CanceledForksByFilters { get; set; }

    public int PuttedForksCount { get; set; }
        

    public int NotOverlappedForksCount { get; set; }

    public string BotStatus { get; set; } = "Старт";
}