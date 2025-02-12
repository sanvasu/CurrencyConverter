

namespace CurrencyConverterAPI.Services
{
    using CurrencyConverterAPI.Models;
    public interface ICurrencyService
    {
        Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency);
        Task<ConversionResponse> ConvertAsync(decimal amount, string from, string to);
        Task<PaginatedResponse<HistoricalRate>> GetHistoricalRatesAsync(
            string baseCurrency,
            DateTime from,
            DateTime to,
            int page,
            int pageSize);
    }
}

