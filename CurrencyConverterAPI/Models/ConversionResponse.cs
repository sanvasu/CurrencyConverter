namespace CurrencyConverterAPI.Models;

public class ConversionResponse
{
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; }
    public string ToCurrency { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ExchangeRate { get; set; }
}