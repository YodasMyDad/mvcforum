using System;
using System.Linq;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{
    public partial class BlockService : IBlockService
    {
        private readonly MVCForumContext _context;
        public BlockService(IMVCForumContext context)
        {
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
            return _context.Block.FirstOrDefault(x => x.Id == id);
        }
    }
}
