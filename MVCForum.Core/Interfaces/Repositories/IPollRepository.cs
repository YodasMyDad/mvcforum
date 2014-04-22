using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IPollRepository
    {
        List<Poll> GetAllPolls();
        Poll Add(Poll poll);
        Poll Get(Guid id);
        void Delete(Poll item);
    }
}
