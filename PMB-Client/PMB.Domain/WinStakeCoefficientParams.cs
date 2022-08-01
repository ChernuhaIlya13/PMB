namespace PMB.Domain;

public class WinStakeCoefficientParams
{
    public WinStakeCoefficientParams(string eventId,bool homeParam)
    {
        EventId = eventId;
        HomeParam = homeParam;
    }
    public string EventId { get; set; }
    
    public bool HomeParam { get; set; }
}