// Copyright (c) Microsoft Corporation.  All rights reserved.

using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace EFProviderWrapperToolkit
{
    /// <summary>
    /// Common implementation of the <see cref="DbProviderFactory"/> methods.
    /// </summary>
    [CLSCompliant(false)]
    public abstract class DbProviderFactoryBase : DbProviderFactory, IServiceProvider
    {
        private DbProviderServices services;

        /// <summary>
        /// Initializes a new instance of the DbProviderFactoryBase class.
        /// </summary>
        /// <param name="providerServices">The provider services.</param>
        protected DbProviderFactoryBase(DbProviderServices providerServices)
        {
            this.services = providerServices;
        }

        /// <summary>
        /// Specifies whether the specific <see cref="T:System.Data.Common.DbProviderFactory"/> supports the <see cref="T:System.Data.Common.DbDataSourceEnumerator"/> class.
        /// </summary>
        /// <value></value>
        /// <returns>true if the instance of the <see cref="T:System.Data.Common.DbProviderFactory"/> supports the <see cref="T:System.Data.Common.DbDataSourceEnumerator"/> class; otherwise false.
        /// </returns>
        public override bool CanCreateDataSourceEnumerator
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Registers the provider.
        /// </summary>
        /// <param name="name">Human-readable name of the provider.</param>
        /// <param name="invariantName">Invariant name of the provider.</param>
        /// <param name="factoryType">Factory type for the provider.</param>
        public static void RegisterProvider(string name, string invariantName, Type factoryType)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (string.IsNullOrEmpty(invariantName))
            {
                throw new ArgumentNullException("invariantName");
            }

            if (factoryType == null)
            {
                throw new ArgumentNullException("factoryType");
            }

            var data = (DataSet)ConfigurationManager.GetSection("system.data");
            var providerFactories = data.Tables["DbProviderFactories"];
            providerFactories.Rows.Add(name, name, invariantName, factoryType.AssemblyQualifiedName);
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
            throw new NotSupportedException();
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
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbConnection"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbConnection"/>.
        /// </returns>
        public abstract override DbConnection CreateConnection();

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
                return this.services;
            }
            else
            {
                return null;
            }
        }
    }
}
