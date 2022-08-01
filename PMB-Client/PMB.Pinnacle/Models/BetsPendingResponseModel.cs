using System;

namespace PMB.Pinnacle.Models;

public class BetsPendingResponseModel
{
    public DateTimeOffset CreatedAt { get; set; }
    
    public long Id { get; set; }
    
    public string OddsFormat { get; set; }
    
    public string Outcome { get; set; }
    
    public Decimal Price { get; set; }

    public string RequestId { get; set; }
    
    public SelectionModelBetsPendingResponse[] Selections { get; set; }
    
    public object SettledAt { get; set; }
    
    public double Stake { get; set; }
    
    public string Status { get; set; }
    
    public double ToWin { get; set; }
    
    public string Type { get; set; }
    
    public int WagerNumber { get; set; }
    
    public object WinLoss { get; set; }
}

public class MarketModel
{
    public string Key { get; set; }
    
    public int Period { get; set; }
    
    public string Type { get; set; }
}

public class SelectionModelBetsPendingResponse
{
    public string Designation { get; set; }
    
    public MarketModel[] Market { get; set; }
    
    public RelatedMatchUpsResult Matchup { get; set; }
    
    public string Outcome { get; set; }
    
    public object Points { get; set; }
    
    public Decimal Price { get; set; }
    
    public string Status { get; set; }
}