// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Data.Common;
using System.Data.Objects;
using EFCachingProvider.Caching;
using EFProviderWrapperToolkit;

namespace EFCachingProvider
{
    /// <summary>
    /// Extension methods for EFCachingProvider.
    /// </summary>
    public static class EFCachingExtensionMethods
    {
        /// <summary>
        /// Gets the cache associated with this connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns><see cref="ICache"/> implementation.</returns>
        public static ICache GetCache(this DbConnection connection)
        {
            return connection.UnwrapConnection<EFCachingConnection>().Cache;
        }

        /// <summary>
        /// Gets the cache associated with the object context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Object context.</returns>
        public static ICache GetCache(this ObjectContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return GetCache(context.Connection);
        }

        /// <summary>
        /// Sets the <see cref="ICache"/> implementation for given connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="cache">The cache.</param>
        public static void SetCache(this DbConnection connection, ICache cache)
        {
            connection.UnwrapConnection<EFCachingConnection>().Cache = cache;
        }

        /// <summary>
        /// Sets the <see cref="ICache"/> implementation for given object context.
        /// </summary>
        /// <param name="context">The object context.</param>
        /// <param name="cache">The cache.</param>
        public static void SetCache(this ObjectContext context, ICache cache)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            SetCache(context.Connection, cache);
        }
    }
}
