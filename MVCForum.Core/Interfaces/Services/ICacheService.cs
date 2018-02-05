namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Enums;

    public partial interface ICacheService
    {
        T Get<T>(string key);

        /// <summary>
        /// Cache objects for a specified amount of time
        /// </summary>
        /// <param name="key">The cache key</param>
        /// <param name="data">Object / Data to cache</param>
        /// <param name="minutesToCache">How many minutes to cache them for</param>
        void Set(string key, object data, CacheTimes minutesToCache);
        bool IsSet(string key);
        void Invalidate(string key);
        void Clear();
        void ClearStartsWith(string keyStartsWith);
        void ClearStartsWith(List<string> keysStartsWith);
        //T CachePerRequest<T>(string cacheKey, Func<T> getCacheItem);
        //void SetPerRequest(string cacheKey, object objectToCache);
    }
}
