using System;
using System.Text.Json;
using EcommerceMinified.Domain.Interfaces.Caching;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace EcommerceMinified.Application.Caching;

public class RedisService : IRedisService
{
    protected readonly ILogger<RedisService> Logger;
    private readonly IDistributedCache _cache;
    protected readonly AsyncCircuitBreakerPolicy CircuitBreakerPolicy;
    private readonly TimeSpan _circuitBreakTime = TimeSpan.FromSeconds(60);
    private readonly int _redisTimeoutInMilliseconds = 1000;
    private readonly int _exceptionCount = 5;
    private static readonly int _redisCacheValidatyMinutes = 5;

    public RedisService(ILogger<RedisService> logger, IDistributedCache cache)
    {
        Logger = logger;
        _cache = cache;

        CircuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(_exceptionCount, _circuitBreakTime,
                onBreak: (ex, t) =>
                {
                    logger.LogWarning(ex, $"Circuit broken! Time: {t}");
                },
                onReset: () =>
                {
                    logger.LogWarning($"Circuit reset!");
                }
            );
    }

    public async Task<T?> GetAsync<T>(Guid id)
    {
        try
        {
            var key = $"{typeof(T).Name}_{id}";

            var result = await CircuitBreakerPolicy.ExecuteAsync(async (_) =>
            {
                var response = await _cache.GetStringAsync(key);

                if (response is null)
                {
                    return default;
                }
                var value = JsonSerializer.Deserialize<T>(response);

                return value;

            }, new CancellationTokenSource(TimeSpan.FromMilliseconds(_redisTimeoutInMilliseconds)).Token);

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting value from Redis");
        }

        return default;
    }

    public async Task SetAsync<T>(Guid id, T value, TimeSpan? expiration = null)
    {
        var key = $"{typeof(T).Name}_{id}";

        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration.HasValue ? expiration.Value : TimeSpan.FromMinutes(_redisCacheValidatyMinutes)
            };

            var policyWrap = Policy.WrapAsync(CircuitBreakerPolicy, Policy.NoOpAsync());

            await policyWrap.ExecuteAsync(async (_) =>
            {
                await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), options, CancellationToken.None);
            }, new CancellationTokenSource(TimeSpan.FromMilliseconds(_redisTimeoutInMilliseconds)).Token);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error setting value in Redis, key: {@keyJSON}, value: {@value}, erro: {@message}",
                             key, JsonSerializer.Serialize(value), ex.Message);
        }
    }

    public async Task RemoveAsync<T>(Guid id)
    {
        var key = $"{typeof(T).Name}_{id}";

        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing value from Redis, key: {@key}", key);
        }
    }
}
