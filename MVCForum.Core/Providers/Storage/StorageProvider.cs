namespace MvcForum.Core.Providers.Storage
{
    using System;
    using Interfaces.Providers;
    using Reflection;

    public static class StorageProvider
    {
        private static readonly Lazy<IStorageProvider> CurrentStorageProvider = new Lazy<IStorageProvider>(() =>
        {
            var type = ForumConfiguration.Instance.StorageProviderType;
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new Exception(
                    "Unable to find storage provider instance, check StorageProviderType is correct in forum.config");
            }

            try
            {
                var storageProviders = ImplementationManager.GetInstances<IStorageProvider>();
                return storageProviders[type];
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Unable to create storage provider instance, check StorageProviderType is correct in forum.config",
                    ex);
            }
        });

        public static IStorageProvider Current => CurrentStorageProvider.Value;
    }
}