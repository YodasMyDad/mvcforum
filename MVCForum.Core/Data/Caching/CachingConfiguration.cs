using System.Data.Entity;
using System.Data.Entity.Core.Common;
using EFCache;

namespace MVCForum.Services.Data.Caching
{
    public class CachingConfiguration : DbConfiguration
    {
        public CachingConfiguration()
        {
            var transactionHandler = new CacheTransactionHandler(new InMemoryCache());

            AddInterceptor(transactionHandler);

            var cachingPolicy = new CachingPolicy();

            Loaded +=
              (sender, args) => args.ReplaceService<DbProviderServices>(
                (s, _) => new CachingProviderServices(s, transactionHandler,
                  cachingPolicy));
        }
    }
}
