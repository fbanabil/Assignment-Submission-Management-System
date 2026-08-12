using Microsoft.Extensions.Caching.Distributed;

namespace Backend.Helpers
{
    public interface ITokenBlacklistRepository
    {
        Task AddToBlacklistAsync(string token, TimeSpan timeToLive);
        Task<bool> IsBlacklistedAsync(string token);
    }
    public class CacheTokenBlacklistRepository : ITokenBlacklistRepository
    {
        private readonly IDistributedCache _cache;
        private const string Prefix = "blacklist:";

        public CacheTokenBlacklistRepository(IDistributedCache cache)
        {
            _cache = cache;
        }


        /// <summary>
        /// This method adds a token to the blacklist with a specified time-to-live (TTL). The token will be stored in the distributed cache and will automatically expire after the given TTL.
        /// </summary>
        /// <param name="token">The token to add to the blacklist.</param>
        /// <param name="timeToLive">The time-to-live (TTL) for the token in the blacklist.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task AddToBlacklistAsync(string token, TimeSpan timeToLive)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            };

            // We just store a tiny flag "1" to save memory
            await _cache.SetStringAsync($"{Prefix}{token}", "1", options);
        }



        /// <summary>
        /// This method checks if a given token is present in the blacklist. It queries the distributed cache for the token and returns true if the token is found, indicating that it is blacklisted; otherwise, it returns false.
        /// </summary>
        /// <param name="token">The token to check for in the blacklist.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the token is blacklisted; otherwise, false.</returns>
        public async Task<bool> IsBlacklistedAsync(string token)
        {
            // We check if the token exists in the cache. If it does, it means the token is blacklisted.
            var result = await _cache.GetStringAsync($"{Prefix}{token}");
            return !string.IsNullOrEmpty(result);
        }
    }
}
