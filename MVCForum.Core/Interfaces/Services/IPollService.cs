using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IPollService
    {
        List<Poll> GetAllPolls();
        Poll Add(Poll poll);
        Poll Get(Guid id);
        void Delete(Poll item);
    }
}
