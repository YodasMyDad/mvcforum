// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Data.Common;

namespace EFCachingProvider
{
    /// <summary>
    /// Provider factory for EFCachingProvider
    /// </summary>
    public class EFCachingProviderFactory : DbProviderFactory, IServiceProvider
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Factory class is immutable.")]
        public static readonly EFCachingProviderFactory Instance = new EFCachingProviderFactory();

        /// <summary>
        /// Specifies whether the specific <see cref="T:System.Data.Common.DbProviderFactory"/> supports the <see cref="T:System.Data.Common.DbDataSourceEnumerator"/> class.
        /// </summary>
        /// <value></value>
        /// <returns>true if the instance of the <see cref="T:System.Data.Common.DbProviderFactory"/> supports the <see cref="T:System.Data.Common.DbDataSourceEnumerator"/> class; otherwise false.
        /// </returns>
        public override bool CanCreateDataSourceEnumerator
        {
            get { return false; }
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbCommand"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbCommand"/>.
        /// </returns>
        public override DbCommand CreateCommand()
        {
            return null;
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbCommandBuilder"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbCommandBuilder"/>.
        /// </returns>
        public override DbCommandBuilder CreateCommandBuilder()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbConnection"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbConnection"/>.
        /// </returns>
        public override DbConnection CreateConnection()
        {
            return new EFCachingConnection();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbConnectionStringBuilder"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbConnectionStringBuilder"/>.
        /// </returns>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbDataAdapter"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbDataAdapter"/>.
        /// </returns>
        public override DbDataAdapter CreateDataAdapter()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbDataSourceEnumerator"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbDataSourceEnumerator"/>.
        /// </returns>
        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return System.Data.Sql.SqlDataSourceEnumerator.Instance;
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbParameter"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbParameter"/>.
        /// </returns>
        public override DbParameter CreateParameter()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>
        /// A service object of type <paramref name="serviceType"/>.
        /// -or-
        /// null if there is no service object of type <paramref name="serviceType"/>.
        /// </returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(DbProviderServices))
            {
                return EFCachingProviderServices.Instance;
            }
            else
            {
                return null;
            }
        }
    }
}
