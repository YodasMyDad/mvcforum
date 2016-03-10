using System;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Providers;

namespace MVCForum.Website.Application.StorageProviders
{
    public static class StorageProvider
    {
        private static readonly Lazy<IStorageProvider> CurrentStorageProvider = new Lazy<IStorageProvider>(() =>
        {
            var type = SiteConstants.Instance.StorageProviderType;
            if (string.IsNullOrEmpty(type))
            {
                return new DiskStorageProvider();
            }

            try
            {
                return TypeFactory.GetInstanceOf<IStorageProvider>(type);
            }
            catch (Exception)
            {
                return new DiskStorageProvider();
            }
        });

        public static IStorageProvider Current => CurrentStorageProvider.Value;
    }
}