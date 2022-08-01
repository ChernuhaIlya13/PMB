namespace PMB.Pinnacle.Models;

public class StraightQuoteModelResult
{
    public ClassModel[] Classes { get; set; }
    
    public Limit[] Limits { get; set; }
    
    public SelectionModelExtended[] Selections { get; set; }
}