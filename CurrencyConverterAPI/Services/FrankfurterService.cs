using Polly;
using Microsoft.Extensions.Caching.Memory;
using CurrencyConverterAPI.Services;
using CurrencyConverterAPI.Models;
using Polly.CircuitBreaker;

public class FrankfurterService : ICurrencyService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FrankfurterService> _logger;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreaker;
    private readonly string _baseUrl;

    public FrankfurterService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<FrankfurterService> logger,
        ApiSettings apiSettings) 
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _baseUrl = apiSettings.BaseUrl;

        var policy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        _circuitBreaker = Policy<HttpResponseMessage>
             .Handle<HttpRequestException>()
             .AdvancedCircuitBreakerAsync(
                 failureThreshold: 0.5,
                 samplingDuration: TimeSpan.FromSeconds(10),
                 minimumThroughput: 8,
                 durationOfBreak: TimeSpan.FromSeconds(30)
             );
    }


    public async Task<ExchangeRateResponse> GetLatestRatesAsync(string baseCurrency)
    {
        var cacheKey = $"rates_{baseCurrency}";

        if (_cache.TryGetValue(cacheKey, out ExchangeRateResponse cached))
        {
            return cached;
        }

        var response = await _circuitBreaker.ExecuteAsync(async () =>
        {
            var result = await _httpClient.GetAsync($"{_baseUrl}/latest?base={baseCurrency}");
            result.EnsureSuccessStatusCode();
            return result;
        });

        var rates = await response.Content.ReadFromJsonAsync<ExchangeRateResponse>();

        _cache.Set(cacheKey, rates, TimeSpan.FromMinutes(5));

        return rates;
    }

   
    public async Task<ConversionResponse> ConvertAsync(decimal amount, string from, string to)
    {
        var rates = await GetLatestRatesAsync(from);

        if (rates == null || !rates.Rates.ContainsKey(to))
        {
            throw new InvalidOperationException($"Conversion rate from {from} to {to} not found.");
        }

        decimal rate = rates.Rates[to];
        decimal convertedAmount = amount * rate;

        return new ConversionResponse
        {
            Amount = amount,
            FromCurrency = from,
            ToCurrency = to,
            ConvertedAmount = convertedAmount
        };
    }

    public async Task<PaginatedResponse<HistoricalRate>> GetHistoricalRatesAsync(
        string baseCurrency,
        DateTime from,
        DateTime to,
        int page,
        int pageSize)
    {
        var cacheKey = $"historical_{baseCurrency}_{from:yyyy-MM-dd}_{to:yyyy-MM-dd}_page{page}_size{pageSize}";

        // Check if cached data exists
        if (_cache.TryGetValue(cacheKey, out PaginatedResponse<HistoricalRate> cached))
        {
            return cached;
        }

        var historicalRates = new List<HistoricalRate>();
        var totalItems = 0;
        var currentPage = page;

        var date = from;

        while (date <= to)
        {
            var response = await _circuitBreaker.ExecuteAsync(async () =>
            {
                var dateStr = date.ToString("yyyy-MM-dd");
                var result = await _httpClient.GetAsync($"{_baseUrl}/{dateStr}?base={baseCurrency}");
                result.EnsureSuccessStatusCode();
                return result;
            });

            var rate = await response.Content.ReadFromJsonAsync<HistoricalRateResponse>();

            historicalRates.Add(new HistoricalRate
            {
                Date = date,
                Rates = rate?.Rates
            });

            date = date.AddDays(1);
        }

        // Paginate the result
        totalItems = historicalRates.Count;
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var paginatedResults = historicalRates
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var paginatedResponse = new PaginatedResponse<HistoricalRate>
        {
            Items = paginatedResults,
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = currentPage
        };

        
        _cache.Set(cacheKey, paginatedResponse, TimeSpan.FromMinutes(5));

        return paginatedResponse;
    }

}
