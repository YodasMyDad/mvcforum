using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class CacheService : ICacheService
    {
        private static ObjectCache Cache { get { return MemoryCache.Default; } }

        private static IDictionaryEnumerator GetCacheToEnumerate()
        {
            return (IDictionaryEnumerator)((IEnumerable)Cache).GetEnumerator();
        }

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

        public void Clear()
        {
            var keys = new List<string>();
            var enumerator = GetCacheToEnumerate();

            while (enumerator.MoveNext())
            {
                keys.Add(enumerator.Key.ToString());
            }

            foreach (var t in keys)
            {
                Cache.Remove(t);
            }
        }

        public void ClearStartsWith(string keyStartsWith)
        {
            var keys = new List<string>();
            var enumerator = GetCacheToEnumerate();

            while (enumerator.MoveNext())
            {
                keys.Add(enumerator.Key.ToString());
            }

            foreach (var t in keys.Where(x => x.StartsWith(keyStartsWith)))
            {
                Cache.Remove(t);
            }
        }


    }
}
