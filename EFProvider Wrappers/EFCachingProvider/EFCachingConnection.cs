// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Data;
using System.Data.Common;
using EFCachingProvider.Caching;
using EFProviderWrapperToolkit;

namespace EFCachingProvider
{
    /// <summary>
    /// Implementation of <see cref="DbConnection"/> with support for caching of Entity Framework queries.
    /// </summary>
    public class EFCachingConnection : DbConnectionWrapper
    {
        /// <summary>
        /// Initializes a new instance of the EFCachingConnection class.
        /// </summary>
        public EFCachingConnection()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the EFCachingConnection class.
        /// </summary>
        /// <param name="wrappedConnection">The wrapped connection.</param>
        public EFCachingConnection(DbConnection wrappedConnection)
        {
            this.WrappedConnection = wrappedConnection;
            this.CachingPolicy = EFCachingProviderConfiguration.DefaultCachingPolicy;
            this.Cache = EFCachingProviderConfiguration.DefaultCache;
        }

        /// <summary>
        /// Gets or sets the cache.
        /// </summary>
        /// <value>The cache.</value>
        public ICache Cache { get; set; }

        /// <summary>
        /// Gets or sets the caching policy.
        /// </summary>
        /// <value>The caching policy.</value>
        public CachingPolicy CachingPolicy { get; set; }

        /// <summary>
        /// Gets the name of the default wrapped provider.
        /// </summary>
        /// <returns>Name of the default wrapped provider.</returns>
        protected override string DefaultWrappedProviderName
        {
            get { return EFCachingProviderConfiguration.DefaultWrappedProvider; }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.Common.DbProviderFactory"/> for this <see cref="T:System.Data.Common.DbConnection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbProviderFactory"/>.
        /// </returns>
        protected override DbProviderFactory DbProviderFactory
        {
            get { return EFCachingProviderFactory.Instance; }
        }

        /// <summary>
        /// Starts a database transaction.
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction.</param>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new EFCachingTransaction(WrappedConnection.BeginTransaction(isolationLevel), this);
        }
    }
}
