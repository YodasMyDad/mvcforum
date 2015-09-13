using System;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class BlockRepository : IBlockRepository
    {
        private readonly MVCForumContext _context;
        public BlockRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public Block Add(Block block)
        {
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
