using Gateway.Interfaces;
using StackExchange.Redis;

namespace Gateway.Services;

public class TokenBlackListingService : ITokenBlackListing
{
    private IDatabase redisDb;

    public TokenBlackListingService(IConnectionMultiplexer redis)
    {
        redisDb = redis.GetDatabase();
    }
    public async Task BlacklistTokenAsync(string token)
    {
        await redisDb.StringSetAsync($"blacklist:{token}", true, TimeSpan.FromDays(1));
    }

    public async Task<bool> IsTokenBlacklistedAsync(string token)
    {
        return (bool)await redisDb.StringGetAsync($"blacklist:{token}");
    }
}
