namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using Models.Entities;

    public partial interface IBlockService : IContextService
    {
        Block Add(Block block);
        void Delete(Block block);
        Block Get(Guid id);
    }
}