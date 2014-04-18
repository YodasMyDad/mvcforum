using System.Data.Entity;
using System.Data.Entity.Core.Common;
using EFCache;

namespace MVCForum.Data.Caching
{
    public class CachingConfiguration : DbConfiguration
    {
        public CachingConfiguration()
        {
            //TODO : Look for a second level cache that works
            var transactionHandler = new CacheTransactionHandler(new InMemoryCache());

            AddInterceptor(transactionHandler);

            Loaded +=
              (sender, args) => args.ReplaceService<DbProviderServices>(
                (s, _) => new CachingProviderServices(s, transactionHandler,
                  new DefaultCachingPolicy()));
        }
    }
}
