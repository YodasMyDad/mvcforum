// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;

namespace EFCachingProvider.Caching
{
    /// <summary>
    /// Caching policy.
    /// </summary>
    public abstract class CachingPolicy
    {
        /// <summary>
        /// Initializes static members of the CachingPolicy class.
        /// </summary>
        static CachingPolicy()
        {
            NoCaching = new NoCachingPolicy();
            CacheAll = new CacheAllPolicy();
        }

        /// <summary>
        /// Gets the caching policy which does no caching.
        /// </summary>
        /// <value>The no caching.</value>
        public static CachingPolicy NoCaching { get; private set; }

        /// <summary>
        /// Gets the caching policy which caches results of all queries.
        /// </summary>
        /// <value>The no caching.</value>
        public static CachingPolicy CacheAll { get; private set; }

        /// <summary>
        /// Determines whether the specified command definition can be cached.
        /// </summary>
        /// <param name="commandDefinition">The command definition.</param>
        /// <returns>
        /// A value of <c>true</c> if the specified command definition can be cached; otherwise, <c>false</c>.
        /// </returns>
        protected internal abstract bool CanBeCached(EFCachingCommandDefinition commandDefinition);

        /// <summary>
        /// Gets the minimum and maximum number cacheable rows for a given command definition.
        /// </summary>
        /// <param name="cachingCommandDefinition">The command definition.</param>
        /// <param name="minCacheableRows">The minimum number of cacheable rows.</param>
        /// <param name="maxCacheableRows">The maximum number of cacheable rows.</param>
        protected internal abstract void GetCacheableRows(EFCachingCommandDefinition cachingCommandDefinition, out int minCacheableRows, out int maxCacheableRows);

        /// <summary>
        /// Gets the expiration timeout for a given command definition.
        /// </summary>
        /// <param name="cachingCommandDefinition">The command definition.</param>
        /// <param name="slidingExpiration">The sliding expiration time.</param>
        /// <param name="absoluteExpiration">The absolute expiration time.</param>
        protected internal virtual void GetExpirationTimeout(EFCachingCommandDefinition cachingCommandDefinition, out TimeSpan slidingExpiration, out DateTime absoluteExpiration)
        {
            slidingExpiration = TimeSpan.Zero;
            absoluteExpiration = DateTime.MaxValue;
        }
    }
}
