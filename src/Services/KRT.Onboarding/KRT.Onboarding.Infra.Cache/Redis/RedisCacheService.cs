using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using KRT.Onboarding.Domain.Interfaces; // <--- O IMPORT QUE FALTAVA

namespace KRT.Onboarding.Infra.Cache.Redis;

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public string InstanceName { get; set; } = "KRT_";
    public int DefaultExpirationMinutes { get; set; } = 60;
}

public class RedisCacheService : ICacheService, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly RedisSettings _settings;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IOptions<RedisSettings> settings,
        ILogger<RedisCacheService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // Conecta ao Redis (Lazy connection é melhor, mas vamos manter simples)
        _redis = ConnectionMultiplexer.Connect(_settings.ConnectionString);
        _database = _redis.GetDatabase();

        _logger.LogInformation("Redis cache connected to {Connection}", _settings.ConnectionString);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var value = await _database.StringGetAsync(fullKey);

            if (value.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache miss for key {Key}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key {Key}", key);
            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            var exp = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

            await _database.StringSetAsync(fullKey, serialized, exp);
            _logger.LogDebug("Cached value for key {Key} with expiration {Expiration}", key, exp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            await _database.KeyDeleteAsync(fullKey);
            _logger.LogDebug("Removed cache for key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from cache for key {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var fullPattern = GetFullKey(pattern);
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());

            var keys = server.Keys(pattern: fullPattern + "*").ToArray();
            if (keys.Length > 0)
            {
                await _database.KeyDeleteAsync(keys);
                _logger.LogDebug("Removed {Count} keys matching pattern {Pattern}", keys.Length, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing values by pattern {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.KeyExistsAsync(fullKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence for key {Key}", key);
            return false;
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.StringIncrementAsync(fullKey, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing key {Key}", key);
            return 0;
        }
    }

    public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            var exp = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);

            return await _database.StringSetAsync(fullKey, serialized, exp, When.NotExists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value if not exists for key {Key}", key);
            return false;
        }
    }

    private string GetFullKey(string key) => $"{_settings.InstanceName}{key}";

    public void Dispose()
    {
        _redis?.Dispose();
    }
}
