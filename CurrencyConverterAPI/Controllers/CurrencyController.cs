using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CurrencyConverterAPI.Services;
using CurrencyConverterAPI.Models;

namespace CurrencyConverterAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize] 
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<CurrencyController> _logger;

    public CurrencyController(
        ICurrencyService currencyService,
        ILogger<CurrencyController> logger)
    {
        _currencyService = currencyService;
        _logger = logger;
    }

 
    [AllowAnonymous] 
    [HttpGet("rates")]
    public async Task<ActionResult<ExchangeRateResponse>> GetLatestRates(
        [FromQuery] string baseCurrency = "EUR")
    {
        var clientInfo = new RequestContext
        {
            ClientIp = HttpContext.Connection.RemoteIpAddress?.ToString(),
            ClientId = User.FindFirst("client_id")?.Value,
            Endpoint = Request.Path.Value
        };

        try
        {
            var rates = await _currencyService.GetLatestRatesAsync(baseCurrency);
            return Ok(rates);
        }
        catch (InvalidCurrencyException ex)
        {
            _logger.LogWarning(ex, "Invalid currency requested");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching exchange rates");
            return StatusCode(500);
        }
    }

   
    [Authorize(Roles = "User,Admin")] 
    [HttpPost("convert")]
    public async Task<ActionResult<ConversionResponse>> ConvertCurrency(
        [FromBody] ConversionRequest request)
    {
        if (IsRestrictedCurrency(request.FromCurrency) ||
            IsRestrictedCurrency(request.ToCurrency))
        {
            return BadRequest(new { error = "Restricted currency requested" });
        }

        try
        {
            var result = await _currencyService.ConvertAsync(
                request.Amount,
                request.FromCurrency,
                request.ToCurrency);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting currency");
            return StatusCode(500);
        }
    }

    [Authorize(Roles = "User,Admin")] 
    [HttpGet("historical")]
    public async Task<ActionResult<PaginatedResponse<HistoricalRate>>> GetHistoricalRates(
       string baseCurrency,
       DateTime from,
       DateTime to,
       int page = 1,
       int pageSize = 10)
    {
        try
        {
            var result = await _currencyService.GetHistoricalRatesAsync(baseCurrency, from, to, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching historical exchange rates");
            return StatusCode(500);
        }
    }


    private bool IsRestrictedCurrency(string currency)
    {
        var restricted = new[] { "TRY", "PLN", "THB", "MXN" };
        return restricted.Contains(currency.ToUpper());
    }
}
