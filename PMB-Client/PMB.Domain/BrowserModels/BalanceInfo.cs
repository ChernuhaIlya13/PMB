namespace PMB.Domain.BrowserModels;

public class BalanceInfo
{
    /// <summary>
    /// Значение баланса
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// Валюта
    /// </summary>
    public string Currency { get; set; }
}