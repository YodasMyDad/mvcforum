using System;

namespace MVCForum.Website.Application.StorageProviders
{
    public static class StorageProvider
    {
        private static Lazy<IStorageProvider> _currentStorageProvider = new Lazy<IStorageProvider>(() =>
        {
            var type = SiteConstants.StorageProviderType;
            if (string.IsNullOrEmpty(type))
            {
                return new DiskStorageProvider();
            }

            try
            {
                return AppHelpers.GetInstanceOf<IStorageProvider>(type);
            }
            catch (Exception)
            {
                return new DiskStorageProvider();
            }
        });

        public static IStorageProvider Current
        {
            get
            {
                return _currentStorageProvider.Value;
            }
        }
    }
}