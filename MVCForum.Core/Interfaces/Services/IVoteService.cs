namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Entities;

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