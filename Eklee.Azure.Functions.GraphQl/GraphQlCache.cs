using System;
using System.Threading.Tasks;
using Eklee.Azure.Functions.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Eklee.Azure.Functions.GraphQl
{
    public class GraphQlCache : IGraphQlCache
    {
        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;

        public GraphQlCache(ICacheManager cacheManager, ILogger logger)
        {
            _cacheManager = cacheManager;
            _logger = logger;
        }

        public async Task<T> GetByKeyAsync<T>(Func<object, T> getResult, object key, int cacheDurationInSeconds)
        {
            var cacheKey = $"{typeof(T).FullName}{key}";

            var cacheResult = await _cacheManager.TryGetOrSetIfNotExistAsync(() => getResult(key), cacheKey, new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(cacheDurationInSeconds)
            });

            _logger.LogDebug($"Result from key {cacheKey} is from cache: {cacheResult.ResultIsFromCache}");

            return cacheResult.Result;
        }
    }
}