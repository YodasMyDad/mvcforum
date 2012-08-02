// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EFCachingProvider.Caching
{
    /// <summary>
    /// Simple cache implementation. Not very efficient, uses simple LRU strategy.
    /// </summary>
    public sealed class InMemoryCache : ICache, IDisposable
    {
        private int cacheHits;
        private int cacheMisses;
        private int cacheAdds;
        private int cacheItemInvalidations;

        // entry key -> CacheEntry
        private Dictionary<string, CacheEntry> entries = new Dictionary<string, CacheEntry>();

        private CacheEntry lruChainHead;
        private CacheEntry lruChainTail;
        private object lruLock = new object();

        // EntitySet -> set of cache entry keys
        private Dictionary<string, HashSet<CacheEntry>> entitySetDependencies = new Dictionary<string, HashSet<CacheEntry>>();
        private ReaderWriterLockSlim entriesLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Initializes a new instance of the InMemoryCache class.
        /// </summary>
        public InMemoryCache()
            : this(Int32.MaxValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the InMemoryCache class.
        /// </summary>
        /// <param name="maxItems">The maximum number of items which can be stored in the cache.</param>
        public InMemoryCache(int maxItems)
        {
            this.MaxItems = maxItems;
            this.GetCurrentDate = () => DateTime.Now;
        }

        /// <summary>
        /// Gets the number of cache hits.
        /// </summary>
        /// <value>The number of cache hits.</value>
        public int CacheHits
        {
            get { return this.cacheHits; }
        }

        /// <summary>
        /// Gets the number of cache misses.
        /// </summary>
        /// <value>The number of cache misses.</value>
        public int CacheMisses
        {
            get { return this.cacheMisses; }
        }

        /// <summary>
        /// Gets the number of cache adds.
        /// </summary>
        /// <value>The number of cache adds.</value>
        public int CacheItemAdds
        {
            get { return this.cacheAdds; }
        }

        /// <summary>
        /// Gets the number of cache item invalidations.
        /// </summary>
        /// <value>The number of cache item invalidations.</value>
        public int CacheItemInvalidations
        {
            get { return this.cacheItemInvalidations; }
        }

        /// <summary>
        /// Gets the maximum number of items the cache can hold.
        /// </summary>
        /// <value>The maximum number of items the cache can hold.</value>
        public int MaxItems { get; private set; }

        /// <summary>
        /// Gets the number of items in the cache.
        /// </summary>
        /// <value>The number of items in the cache.</value>
        public int Count
        {
            get
            {
                this.entriesLock.EnterReadLock();
                int count = this.entries.Count;
                this.entriesLock.ExitReadLock();
                return count;
            }
        }

        internal Func<DateTime> GetCurrentDate { get; set; }

        internal CacheEntry LruChainHead
        {
            get { return this.lruChainHead; }
        }

        internal CacheEntry LruChainTail
        {
            get { return this.lruChainTail; }
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
            CacheEntry entry;
            var currentDate = this.GetCurrentDate();

            this.entriesLock.EnterReadLock();
            bool success = this.entries.TryGetValue(key, out entry);
            this.entriesLock.ExitReadLock();

            if (success)
            {
                if (currentDate >= entry.ExpirationTime)
                {
                    success = false;
                    this.InvalidateExpiredEntries();
                }
            }

            if (!success)
            {
                Interlocked.Increment(ref this.cacheMisses);
                value = null;
                return false;
            }
            else
            {
                Interlocked.Increment(ref this.cacheHits);
                this.MoveToTopOfLruChain(entry);
                entry.LastAccessTime = this.GetCurrentDate();
                if (entry.SlidingExpiration > TimeSpan.Zero)
                {
                    entry.ExpirationTime = this.GetCurrentDate().Add(entry.SlidingExpiration);
                }

                value = entry.Value;
                return true;
            }
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
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (dependentEntitySets == null)
            {
                throw new ArgumentNullException("dependentEntitySets");
            }

            var newEntry = new CacheEntry()
            {
                Key = key,
                KeyHashCode = key.GetHashCode(),
                Value = value,
                DependentEntitySets = dependentEntitySets,
                SlidingExpiration = slidingExpiration,
                ExpirationTime = absoluteExpiration,
            };

            if (slidingExpiration > TimeSpan.Zero)
            {
                newEntry.ExpirationTime = this.GetCurrentDate().Add(slidingExpiration);
            }
            else
            {
                newEntry.ExpirationTime = absoluteExpiration;
            }

            this.entriesLock.EnterWriteLock();

            CacheEntry oldEntry;

            if (this.entries.TryGetValue(key, out oldEntry))
            {
                this.InvalidateSingleEntry(oldEntry);
            }

            // too many items in the cache - invalidate the last one in LRU chain
            if (this.entries.Count >= this.MaxItems)
            {
                this.InvalidateSingleEntry(this.lruChainTail);
            }

            this.entries.Add(key, newEntry);

            foreach (string entitySet in dependentEntitySets)
            {
                HashSet<CacheEntry> queriesDependentOnSet;

                if (!this.entitySetDependencies.TryGetValue(entitySet, out queriesDependentOnSet))
                {
                    queriesDependentOnSet = new HashSet<CacheEntry>();
                    this.entitySetDependencies[entitySet] = queriesDependentOnSet;
                }

                queriesDependentOnSet.Add(newEntry);
            }

            Interlocked.Increment(ref this.cacheAdds);
            this.MoveToTopOfLruChain(newEntry);
            newEntry.LastAccessTime = this.GetCurrentDate();
            this.entriesLock.ExitWriteLock();
        }

        /// <summary>
        /// Invalidates all cache entries which are dependent on any of the specified entity sets.
        /// </summary>
        /// <param name="entitySets">The entity sets.</param>
        public void InvalidateSets(IEnumerable<string> entitySets)
        {
            if (entitySets == null)
            {
                throw new ArgumentNullException("entitySets");
            }

            this.entriesLock.EnterWriteLock();
            foreach (string entitySet in entitySets)
            {
                HashSet<CacheEntry> dependentEntries;

                if (this.entitySetDependencies.TryGetValue(entitySet, out dependentEntries))
                {
                    foreach (CacheEntry entry in dependentEntries.ToArray())
                    {
                        this.InvalidateSingleEntry(entry);
                    }
                }
            }

            this.entriesLock.ExitWriteLock();
        }

        /// <summary>
        /// Invalidates cache entry with a given key.
        /// </summary>
        /// <param name="key">The cache key.</param>
        public void InvalidateItem(string key)
        {
            this.entriesLock.EnterWriteLock();
            CacheEntry entry;
            if (this.entries.TryGetValue(key, out entry))
            {
                this.InvalidateSingleEntry(entry);
            }

            this.entriesLock.ExitWriteLock();
        }

        /// <summary>
        /// Releases unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.entriesLock.Dispose();
        }

        private void InvalidateSingleEntry(CacheEntry entry)
        {
            this.RemoveFromLruChain(entry);

            Interlocked.Increment(ref this.cacheItemInvalidations);
            Debug.Assert(this.entriesLock.IsWriteLockHeld, "Must be holding write lock");
            this.entries.Remove(entry.Key);
            foreach (string set in entry.DependentEntitySets)
            {
                this.entitySetDependencies[set].Remove(entry);
            }
        }

        private void MoveToTopOfLruChain(CacheEntry entry)
        {
            lock (this.lruLock)
            {
                if (entry != this.lruChainHead)
                {
                    if (entry == this.lruChainTail)
                    {
                        this.lruChainTail = this.lruChainTail.PreviousEntry;
                    }

                    if (entry.PreviousEntry != null)
                    {
                        entry.PreviousEntry.NextEntry = entry.NextEntry;
                    }

                    if (entry.NextEntry != null)
                    {
                        entry.NextEntry.PreviousEntry = entry.PreviousEntry;
                    }

                    if (this.lruChainHead != null)
                    {
                        this.lruChainHead.PreviousEntry = entry;
                    }

                    entry.NextEntry = this.lruChainHead;
                    entry.PreviousEntry = null;
                    this.lruChainHead = entry;

                    if (this.lruChainTail == null)
                    {
                        this.lruChainTail = entry;
                    }
                }
            }
        }

        private void RemoveFromLruChain(CacheEntry entry)
        {
            lock (this.lruLock)
            {
                if (entry == this.lruChainHead)
                {
                    this.lruChainHead = this.lruChainHead.NextEntry;
                }

                if (entry.PreviousEntry != null)
                {
                    entry.PreviousEntry.NextEntry = entry.NextEntry;
                }
                else
                {
                    this.lruChainHead = entry.NextEntry;
                }

                if (entry.NextEntry != null)
                {
                    entry.NextEntry.PreviousEntry = entry.PreviousEntry;
                }
                else
                {
                    this.lruChainTail = entry.PreviousEntry;
                }
            }
        }

        private void InvalidateExpiredEntries()
        {
            this.entriesLock.EnterWriteLock();

            var now = this.GetCurrentDate();
            CacheEntry nextEntry;
            for (CacheEntry entryToExpire = this.LruChainHead; entryToExpire != null; entryToExpire = nextEntry)
            {
                // remember this reference as the invalication function will destroy it
                nextEntry = entryToExpire.NextEntry;
                if (now >= entryToExpire.ExpirationTime)
                {
                    this.InvalidateSingleEntry(entryToExpire);
                }
            }

            this.entriesLock.ExitWriteLock();
        }

        /// <summary>
        /// Cache entry.
        /// </summary>
        internal class CacheEntry : IEquatable<CacheEntry>
        {
            internal int KeyHashCode { get; set; }

            internal string Key { get; set; }

            internal object Value { get; set; }

            internal IEnumerable<string> DependentEntitySets { get; set; }

            internal TimeSpan SlidingExpiration { get; set; }

            internal DateTime ExpirationTime { get; set; }

            internal DateTime LastAccessTime { get; set; }

            internal CacheEntry NextEntry { get; set; }

            internal CacheEntry PreviousEntry { get; set; }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
            /// </summary>
            /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
            /// <returns>
            /// A value of <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            /// <exception cref="T:System.NullReferenceException">
            /// The <paramref name="obj"/> parameter is null.
            /// </exception>
            public override bool Equals(object obj)
            {
                CacheEntry other = obj as CacheEntry;
                if (other == null)
                {
                    return false;
                }

                return this.Equals(other);
            }

            /// <summary>
            /// Determines whether the specified <see cref="CacheEntry"/> is equal to this instance.
            /// </summary>
            /// <param name="other">The other cache entry.</param>
            /// <returns>
            /// A value of <c>true</c> if the specified cache entry is equal to this instance; otherwise, <c>false</c>.
            /// </returns>
            public bool Equals(CacheEntry other)
            {
                if (other == null)
                {
                    throw new ArgumentNullException("other");
                }

                if (this.KeyHashCode != other.KeyHashCode)
                {
                    return false;
                }

                return this.Key.Equals(other.Key, StringComparison.Ordinal);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return this.KeyHashCode;
            }
        }
    }
}
