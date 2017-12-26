namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using DomainModel.Entities;

    public partial interface IPollService
    {
        List<Poll> GetAllPolls();
        Poll Add(Poll poll);
        Poll Get(Guid id);
        void Delete(Poll item);
    }
}