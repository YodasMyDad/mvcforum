using System;
using MVCForum.Domain.DomainModel.Entities;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IBlockRepository
    {
        Block Add(Block block);
        void Delete(Block block);
        Block Get(Guid id);
    }
}
