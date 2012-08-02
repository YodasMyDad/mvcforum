// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;

namespace EFCachingProvider.Caching
{
    /// <summary>
    /// Custom caching policy on per-table basis.
    /// </summary>
    public class CustomCachingPolicy : CachingPolicy
    {
        /// <summary>
        /// Initializes a new instance of the CustomCachingPolicy class.
        /// </summary>
        public CustomCachingPolicy()
        {
            this.MinCacheableRows = 0;
            this.MaxCacheableRows = 1000;
            this.CacheableTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.NonCacheableTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets the minimal number of cacheable rows.
        /// </summary>
        /// <value>Minimal number of cacheable rows.</value>
        public int MinCacheableRows { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of cacheable rows.
        /// </summary>
        /// <value>Maximum number of cacheable rows.</value>
        public int MaxCacheableRows { get; set; }

        /// <summary>
        /// Gets the collection of cacheable tables.
        /// </summary>
        /// <value>The cacheable tables.</value>
        public ICollection<string> CacheableTables { get; private set; }

        /// <summary>
        /// Gets the collection of non cacheable tables.
        /// </summary>
        /// <value>The non cacheable tables.</value>
        public ICollection<string> NonCacheableTables { get; private set; }

        /// <summary>
        /// Determines whether the specified command definition can be cached.
        /// </summary>
        /// <param name="commandDefinition">The command definition.</param>
        /// <returns>
        /// A value of <c>true</c> if the specified command definition can be cached; otherwise, <c>false</c>.
        /// </returns>
        protected internal override bool CanBeCached(EFCachingCommandDefinition commandDefinition)
        {
            if (commandDefinition == null)
            {
                throw new ArgumentNullException("commandDefinition");
            }

            // check if we have any non-cacheable tables in the query
            if (this.NonCacheableTables.Count > 0)
            {
                foreach (EntitySetBase esb in commandDefinition.AffectedEntitySets)
                {
                    if (this.NonCacheableTables.Contains(esb.Name))
                    {
                        return false;
                    }
                }
            }

            // by default everything is cacheable
            if (this.CacheableTables.Count == 0)
            {
                return true;
            }
            else
            {
                // unless the user has explicitly specified which tables to cache
                foreach (EntitySetBase esb in commandDefinition.AffectedEntitySets)
                {
                    if (!this.CacheableTables.Contains(esb.Name))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Gets the minimum and maximum number cacheable rows for a given command definition.
        /// </summary>
        /// <param name="cachingCommandDefinition">The command definition.</param>
        /// <param name="minCacheableRows">The minimum number of cacheable rows.</param>
        /// <param name="maxCacheableRows">The maximum number of cacheable rows.</param>
        protected internal override void GetCacheableRows(EFCachingCommandDefinition cachingCommandDefinition, out int minCacheableRows, out int maxCacheableRows)
        {
            minCacheableRows = this.MinCacheableRows;
            maxCacheableRows = this.MaxCacheableRows;
        }
    }
}
