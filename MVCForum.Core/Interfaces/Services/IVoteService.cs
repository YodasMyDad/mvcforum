using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IVoteService
    {
        Vote Add(Vote vote);
        void Delete(Vote vote);
        IList<Vote> GetAllVotesByUser(Guid membershipId);
    }
}
