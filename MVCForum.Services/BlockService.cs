namespace MVCForum.Services
{
    using Domain.Constants;
    using System;
    using System.Linq;
    using Domain.DomainModel.Entities;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;

    public partial class BlockService : IBlockService
    {
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        public BlockService(IMVCForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context as MVCForumContext;
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
