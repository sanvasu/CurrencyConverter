namespace CurrencyConverterAPI.Models;

public class ExchangeRateResponse
{
    public string BaseCurrency { get; set; }
    public Dictionary<string, decimal> Rates { get; set; }
    public DateTime Date { get; set; }
}