// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Text;
using System.Threading;
using EFCachingProvider.Caching;
using EFProviderWrapperToolkit;

namespace EFCachingProvider
{
    /// <summary>
    /// Implementation of <see cref="DbCommand"/> wrappr which implements query caching.
    /// </summary>
    public sealed class EFCachingCommand : DbCommandWrapper
    {
        private static int cacheableCommands;
        private static int nonCacheableCommands;
        private static int cacheHits;
        private static int cacheMisses;
        private static int cacheAdds;

        private EFCachingTransaction transaction;

        /// <summary>
        /// Initializes a new instance of the EFCachingCommand class.
        /// </summary>
        /// <param name="wrappedCommand">The wrapped command.</param>
        /// <param name="commandDefinition">The command definition.</param>
        public EFCachingCommand(DbCommand wrappedCommand, DbCommandDefinitionWrapper commandDefinition)
            : base(wrappedCommand, commandDefinition)
        {
        }

        /// <summary>
        /// Gets the number of cacheable commands.
        /// </summary>
        /// <value>The cacheable commands.</value>
        public static int CacheableCommands
        {
            get { return cacheableCommands; }
        }

        /// <summary>
        /// Gets the number of non-cacheable commands.
        /// </summary>
        /// <value>The non cacheable commands.</value>
        public static int NonCacheableCommands
        {
            get { return nonCacheableCommands; }
        }

        /// <summary>
        /// Gets the number of cache hits.
        /// </summary>
        /// <value>The cache hits.</value>
        public static int CacheHits
        {
            get { return cacheHits; }
        }

        /// <summary>
        /// Gets the total number of cache misses.
        /// </summary>
        public static int CacheMisses
        {
            get { return cacheMisses; }
        }

        /// <summary>
        /// Gets the total number of cache adds.
        /// </summary>
        /// <value>The number of cache adds.</value>
        public static int CacheAdds
        {
            get { return cacheAdds; }
        }

        /// <summary>
        /// Gets or sets the <see cref="P:System.Data.Common.DbCommand.DbTransaction"/> within which this <see cref="T:System.Data.Common.DbCommand"/> object executes.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The transaction within which a Command object of a .NET Framework data provider executes. The default value is a null reference (Nothing in Visual Basic).
        /// </returns>
        protected override DbTransaction DbTransaction
        {
            get
            {
                return this.transaction;
            }

            set
            {
                this.transaction = (EFCachingTransaction)value;
                if (this.transaction != null)
                {
                    WrappedCommand.Transaction = this.transaction.WrappedTransaction;
                }
                else
                {
                    WrappedCommand.Transaction = null;
                }
            }
        }

        /// <summary>
        /// Gets <see cref="EFCachingConnection"/> used by this <see cref="T:System.Data.Common.DbCommand"/>.
        /// </summary>
        /// <returns>
        /// The connection to the data source.
        /// </returns>
        private new EFCachingConnection Connection
        {
            get { return (EFCachingConnection)base.Connection; }
        }

        private new EFCachingCommandDefinition Definition
        {
            get { return (EFCachingCommandDefinition)base.Definition; }
        }

        /// <summary>
        /// Executes a SQL statement against a connection object.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
        public override int ExecuteNonQuery()
        {
            this.UpdateAffectedEntitySets();
            return WrappedCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <returns>
        /// The first column of the first row in the result set.
        /// </returns>
        public override object ExecuteScalar()
        {
            this.UpdateAffectedEntitySets();
            return WrappedCommand.ExecuteScalar();
        }

        /// <summary>
        /// Executes the command text against the connection.
        /// </summary>
        /// <param name="behavior">An instance of <see cref="T:System.Data.CommandBehavior"/>.</param>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbDataReader"/>.
        /// </returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            ICache cache = this.Connection.Cache;
            if (cache == null)
            {
                Interlocked.Increment(ref nonCacheableCommands);
                return WrappedCommand.ExecuteReader(behavior);
            }

            this.UpdateAffectedEntitySets();
            string cacheKey = this.GetCacheKey();
            if (cacheKey == null || !this.Definition.IsCacheable() || !this.Connection.CachingPolicy.CanBeCached(this.Definition))
            {
                // non-cacheable
                Interlocked.Increment(ref nonCacheableCommands);
                return WrappedCommand.ExecuteReader(behavior);
            }

            object value;

            Interlocked.Increment(ref cacheableCommands);
            if (cache.GetItem(cacheKey, out value))
            {
                Interlocked.Increment(ref cacheHits);

                // got cache entry - create reader based on that
                return new CachingDataReaderCacheReader((DbQueryResults)value);
            }
            else
            {
                Interlocked.Increment(ref cacheMisses);
                List<string> dependentEntitySets = new List<string>();
                foreach (EntitySetBase set in this.Definition.AffectedEntitySets)
                {
                    dependentEntitySets.Add(set.Name);
                }

                int minCacheableRows, maxCacheableRows;

                this.Connection.CachingPolicy.GetCacheableRows(this.Definition, out minCacheableRows, out maxCacheableRows);

                return new EFCachingDataReaderCacheWriter(
                    this.WrappedCommand.ExecuteReader(behavior),
                    maxCacheableRows,
                    delegate(DbQueryResults entry)
                    {
                        if (entry.Rows.Count >= minCacheableRows && entry.Rows.Count <= maxCacheableRows)
                        {
                            Interlocked.Increment(ref cacheAdds);
                            DateTime absoluteExpiration;
                            TimeSpan slidingExpiration;

                            this.Connection.CachingPolicy.GetExpirationTimeout(this.Definition, out slidingExpiration, out absoluteExpiration);
                            cache.PutItem(cacheKey, entry, dependentEntitySets, slidingExpiration, absoluteExpiration);
                        }
                    });
            }
        }

        private static string GetLiteralValue(object value)
        {
            if (value is string)
            {
                return "'" + value.ToString().Replace("'", "''") + "'";
            }
            else
            {
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        private string GetCacheKey()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(CommandType);
            sb.Append("|");
            sb.Append(CommandText);

            foreach (DbParameter parameter in Parameters)
            {
                if (parameter.Direction != ParameterDirection.Input)
                {
                    // we don't cache queries with output parameters
                    return null;
                }

                sb = sb.Replace("@" + parameter.ParameterName, GetLiteralValue(parameter.Value));
            }

#if HASH_COMMANDS
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            string hashString = Convert.ToBase64String(MD5.Create().ComputeHash(bytes));
            // Console.WriteLine("hashString: {0}", hashString);
            return hashString;
#else
            return sb.ToString();
#endif
        }

        private void UpdateAffectedEntitySets()
        {
            if (this.transaction != null)
            {
                if (this.Definition.IsModification)
                {
                    this.transaction.HasModifications = true;
                }

                foreach (EntitySetBase entitySet in this.Definition.AffectedEntitySets)
                {
                    this.transaction.AddAffectedEntitySet(entitySet);
                }
            }
        }
    }
}
