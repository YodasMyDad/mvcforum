// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Caching;
using EFCachingProvider.Caching;

namespace EFCachingProvider.Web
{
    /// <summary>
    /// Implementation of <see cref="ICache"/> which works with ASP.NET cache object.
    /// </summary>
    public class AspNetCache : ICache
    {
        private const string DependentEntitySetPrefix = "dependent_entity_set_";
        private HttpContext httpContext;

        /// <summary>
        /// Initializes a new instance of the AspNetCache class.
        /// </summary>
        public AspNetCache()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the AspNetCache class.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        public AspNetCache(HttpContext httpContext)
        {
            this.httpContext = httpContext;
        }

        private Cache HttpCache
        {
            get
            {
                if (this.httpContext != null)
                {
                    return this.httpContext.Cache;
                }

                var context = HttpContext.Current;
                if (context == null)
                {
                    throw new InvalidOperationException("Unable to determine HTTP context.");
                }

                return context.Cache;
            }
        }

        /// <summary>
        /// Tries to the get entry by key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>
        /// A value of <c>true</c> if entry was found in the cache, <c>false</c> otherwise.
        /// </returns>
        public bool GetItem(string key, out object value)
        {
            key = GetCacheKey(key);
            value = this.HttpCache.Get(key);

            return value != null;
        }

        /// <summary>
        /// Adds the specified entry to the cache.
        /// </summary>
        /// <param name="key">The entry key.</param>
        /// <param name="value">The entry value.</param>
        /// <param name="dependentEntitySets">The list of dependent entity sets.</param>
        /// <param name="slidingExpiration">The sliding expiration.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan slidingExpiration, DateTime absoluteExpiration)
        {
            key = GetCacheKey(key);
            var cache = this.HttpCache;

            foreach (var entitySet in dependentEntitySets)
            {
                this.EnsureEntryExists(DependentEntitySetPrefix + entitySet);
            }

            try
            {
                CacheDependency cd = new CacheDependency(new string[0], dependentEntitySets.Select(c => DependentEntitySetPrefix + c).ToArray());
                cache.Insert(key, value, cd, absoluteExpiration, slidingExpiration, CacheItemPriority.Normal, null);
            }
            catch (Exception)
            {
                // there's a possibility that one of the dependencies has been evicted by another thread
                // in this case just don't put this item in the cache
            }
        }

        /// <summary>
        /// Invalidates all cache entries which are dependent on any of the specified entity sets.
        /// </summary>
        /// <param name="entitySets">The entity sets.</param>
        public void InvalidateSets(IEnumerable<string> entitySets)
        {
            foreach (string entitySet in entitySets)
            {
                this.HttpCache.Remove(DependentEntitySetPrefix + entitySet);
            }
        }

        /// <summary>
        /// Invalidates cache entry with a given key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        public void InvalidateItem(string key)
        {
            key = GetCacheKey(key);
            this.HttpCache.Remove(key);
        }

        /// <summary>
        /// Hashes the query to produce cache key..
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Hashed query which becomes a cache key.</returns>
        private static string GetCacheKey(string query)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(query);
            string hashString = Convert.ToBase64String(MD5.Create().ComputeHash(bytes));
            return hashString;
        }

        private void EnsureEntryExists(string key)
        {
            var cache = this.HttpCache;

            if (cache.Get(key) == null)
            {
                try
                {
                    cache.Insert(key, key, null, Cache.NoAbsoluteExpiration, Cache.NoSlidingExpiration);
                }
                catch (Exception)
                {
                    // ignore exceptions.
                }
            }
        }
    }
}