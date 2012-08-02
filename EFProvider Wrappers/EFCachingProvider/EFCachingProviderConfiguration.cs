// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Configuration;
using EFCachingProvider.Caching;
using EFProviderWrapperToolkit;

namespace EFCachingProvider
{
    /// <summary>
    /// Default configuration settings for EFCachingProvider.
    /// </summary>
    public static class EFCachingProviderConfiguration
    {
        /// <summary>
        /// Initializes static members of the EFCachingProviderConfiguration class.
        /// </summary>
        static EFCachingProviderConfiguration()
        {
            DefaultCachingPolicy = CachingPolicy.NoCaching;
            DefaultWrappedProvider = ConfigurationManager.AppSettings["EFCachingProvider.wrappedProvider"];
        }

        /// <summary>
        /// Gets or sets the default wrapped provider.
        /// </summary>
        /// <value>The default wrapped provider.</value>
        public static string DefaultWrappedProvider { get; set; }

        /// <summary>
        /// Gets or sets default caching <see cref="ICache"/> implementation which should be used for new connections.
        /// </summary>
        public static ICache DefaultCache { get; set; }

        /// <summary>
        /// Gets or sets default caching policy to be applied to all new connections.
        /// </summary>
        public static CachingPolicy DefaultCachingPolicy { get; set; }

        /// <summary>
        /// Registers the provider factory 
        /// </summary>
        public static void RegisterProvider()
        {
            DbProviderFactoryBase.RegisterProvider("EF Caching Data Provider", "EFCachingProvider", typeof(EFCachingProviderFactory));
        }
    }
}
