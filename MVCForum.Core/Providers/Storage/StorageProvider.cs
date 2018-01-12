namespace MvcForum.Core.Providers.Storage
{
    using System;
    using Constants;
    using Interfaces.Providers;

    public static class StorageProvider
    {
        private static readonly Lazy<IStorageProvider> CurrentStorageProvider = new Lazy<IStorageProvider>(() =>
        {
            var type = SiteConstants.Instance.StorageProviderType;
            if (string.IsNullOrWhiteSpace(type))
            {
                return new DiskStorageProvider();
            }

            try
            {
                return (IStorageProvider)Activator.CreateInstance(Type.GetType(type));
            }
            catch (Exception)
            {
                return new DiskStorageProvider();
            }
        });

        public static IStorageProvider Current => CurrentStorageProvider.Value;
    }
}