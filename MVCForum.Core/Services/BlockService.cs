namespace MvcForum.Core.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Constants;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;

    public partial class BlockService : IBlockService
    {
        private IMvcForumContext _context;
        private readonly ICacheService _cacheService;

        public BlockService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
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
            return _context.Block.Find(id);
        }
    }
}
