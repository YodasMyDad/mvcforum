using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IVoteService
    {
        Vote Add(Vote vote);
        Vote Get(Guid id);
        void Delete(Vote vote);
        IList<Vote> GetAllVotesByUser(Guid membershipId);
        List<Vote> GetVotesByPosts(List<Guid> postIds);
        List<Vote> GetVotesByPost(Guid postId);
    }
}
