using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Diagnostics;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly IConnectionMultiplexer _redisConnection;
    private const int MaxRequestsPerTimePeriod = 100; 
    private const int TimeWindowInSeconds = 60;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConnectionMultiplexer redisConnection)
    {
        _next = next;
        _logger = logger;
        _redisConnection = redisConnection;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var correlationId = context.Request.Headers.ContainsKey("X-Correlation-ID")
            ? context.Request.Headers["X-Correlation-ID"].ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers["X-Correlation-ID"] = correlationId;

        var clientId = context.User?.FindFirst("client_id")?.Value ?? "Unknown";
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        if (clientIp == null)
            return;

        var redis = _redisConnection.GetDatabase();
        var cacheKey = $"RateLimit:{clientIp}"; 

        var requestCount = (int)await redis.StringGetAsync(cacheKey);

        if (requestCount >= MaxRequestsPerTimePeriod)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId} with IP {ClientIp}, CorrelationId: {CorrelationId}", clientId, clientIp, correlationId);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.Add("Retry-After", TimeWindowInSeconds.ToString());
            await context.Response.WriteAsync("Too many requests, please try again later.");
            return;
        }

        await redis.StringIncrementAsync(cacheKey);
        await redis.KeyExpireAsync(cacheKey, TimeSpan.FromSeconds(TimeWindowInSeconds));

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation("Response sent: {Method} {Path}, Status: {StatusCode}, Response Time: {ResponseTime} ms, CorrelationId: {CorrelationId}",
            context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds, correlationId);
    }
}

public class RequestInfo
    {
        public List<RequestDetails> Requests { get; set; }
    }

    public class RequestDetails
    {
        public DateTime Timestamp { get; set; }
    }

