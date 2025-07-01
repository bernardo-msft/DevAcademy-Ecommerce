using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Repositories;
using StackExchange.Redis;
using System.Text.Json;

namespace Ecommerce.Infrastructure.Persistence.Repositories;

public class RedisCartRepository : ICartRepository
{
    private readonly IDatabase _redisDatabase;
    private const string CartKeyPrefix = "cart:";
    private static readonly TimeSpan CartExpiry = TimeSpan.FromDays(7);

    public RedisCartRepository(IConnectionMultiplexer redis)
    {
        _redisDatabase = redis.GetDatabase();
    }

    public async Task<bool> DeleteAsync(string cartId)
    {
        return await _redisDatabase.KeyDeleteAsync(CartKeyPrefix + cartId);
    }

    public async Task<ShoppingCartDto?> GetByIdAsync(string cartId)
    {
        var redisValue = await _redisDatabase.StringGetAsync(CartKeyPrefix + cartId);
        if (redisValue.IsNullOrEmpty)
        {
            return null;
        }
        return JsonSerializer.Deserialize<ShoppingCartDto>(redisValue!);
    }

    public async Task<bool> RenameAsync(string oldCartId, string newCartId)
    {
        return await _redisDatabase.KeyRenameAsync(CartKeyPrefix + oldCartId, CartKeyPrefix + newCartId);
    }

    public async Task<ShoppingCartDto> UpdateAsync(string cartId, ShoppingCartDto cart)
    {
        var json = JsonSerializer.Serialize(cart);
        await _redisDatabase.StringSetAsync(CartKeyPrefix + cartId, json, CartExpiry);
        return cart;
    }
}