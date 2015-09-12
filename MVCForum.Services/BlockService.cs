using System;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class BlockService : IBlockService
    {
        private readonly IBlockRepository _blockRepository;

        public BlockService(IBlockRepository blockRepository)
        {
            _blockRepository = blockRepository;
        }

        public Block Add(Block block)
        {
            block.Date = DateTime.UtcNow;
            return _blockRepository.Add(block);
        }

        public void Delete(Block block)
        {
           _blockRepository.Delete(block);
        }

        public Block Get(Guid id)
        {
            return _blockRepository.Get(id);
        }
    }
}
