using Ecommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Ecommerce.Infrastructure.Services;

public class RedisTokenBlacklistService : ITokenBlacklistService
{
    private readonly IDatabase _redisDatabase;
    private readonly ILogger<RedisTokenBlacklistService> _logger;
    private const string BlacklistKeyPrefix = "blacklist:jti:";

    public RedisTokenBlacklistService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisTokenBlacklistService> logger)
    {
        _redisDatabase = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    public async Task BlacklistTokenAsync(string jti, DateTimeOffset expiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            _logger.LogWarning("Attempted to blacklist a token with an empty JTI.");
            return;
        }

        var key = $"{BlacklistKeyPrefix}{jti}";
        var remainingTime = expiresAtUtc - DateTimeOffset.UtcNow;

        // Only add to blacklist if the token hasn't already expired
        if (remainingTime <= TimeSpan.Zero)
        {
            _logger.LogInformation("Token with JTI {Jti} already expired. Not adding to blacklist.", jti);
            return;
        }

        try
        {
            await _redisDatabase.StringSetAsync(key, "blacklisted", remainingTime);
            _logger.LogInformation("Token with JTI {Jti} successfully blacklisted in Redis.", jti);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "RedisException occurred while trying to blacklist token with JTI {Jti}.", jti);
            // Depending on policy, you might rethrow or handle. For blacklisting, logging might be sufficient.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while trying to blacklist token with JTI {Jti}.", jti);
        }
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti)
    {
        if (string.IsNullOrWhiteSpace(jti))
        {
            _logger.LogWarning("Attempted to check blacklist status for an empty JTI.");
            return false; // Consistent with Fail-Open if JTI is invalid
        }
        var key = $"{BlacklistKeyPrefix}{jti}";

        try
        {
            return await _redisDatabase.KeyExistsAsync(key);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "RedisException occurred while checking if token with JTI {Jti} is blacklisted. Assuming not blacklisted (Fail-Open).", jti);
            return false; // Fail-Open: If Redis is down, assume token is not blacklisted.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while checking if token with JTI {Jti} is blacklisted. Assuming not blacklisted (Fail-Open).", jti);
            return false; // Fail-Open
        }
    }
}