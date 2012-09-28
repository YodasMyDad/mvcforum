using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface IPollRepository
    {
        List<Poll> GetAllPolls();
        Poll Add(Poll poll);
        Poll Get(Guid id);
        void Delete(Poll item);
    }
}
