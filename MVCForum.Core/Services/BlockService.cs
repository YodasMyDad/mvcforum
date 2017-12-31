namespace MvcForum.Core.Services
{
    using System;
    using System.Linq;
    using Constants;
    using Data.Context;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;

    public partial class BlockService : IBlockService
    {
        private readonly MvcForumContext _context;
        private readonly ICacheService _cacheService;

        public BlockService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context as MvcForumContext;
        }

        public Block Add(Block block)
        {
            block.Date = DateTime.UtcNow;
            return _context.Block.Add(block);
        }

        public void Delete(Block block)
        {
            _context.Block.Remove(block);
        }

        public Block Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.Block.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Block.FirstOrDefault(x => x.Id == id));
        }
    }
}
