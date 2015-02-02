using System;
using System.Runtime.Caching;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class CacheService : ICacheService
    {
        private static ObjectCache Cache { get { return MemoryCache.Default; } }

        public object Get(string key)
        {
            return Cache[key];
        }

        public void Set(string key, object data, int cacheTime)
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.UtcNow + TimeSpan.FromMinutes(cacheTime)
            };

            Cache.Add(new CacheItem(key, data), policy);
        }

        public bool IsSet(string key)
        {
            return (Cache[key] != null);
        }

        public void Invalidate(string key)
        {
            Cache.Remove(key);
        }
    }
}
