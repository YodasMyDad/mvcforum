namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Entities;

    public partial interface IVoteService : IContextService
    {
        Vote Add(Vote vote);
        Vote Get(Guid id);
        void Delete(Vote vote);
        IList<Vote> GetAllVotesByUser(Guid membershipId);
        Dictionary<Guid, List<Vote>> GetVotesByPosts(List<Guid> postIds);
        Dictionary<Guid, Dictionary<Guid, List<Vote>>> GetVotesByTopicsGroupedIntoPosts(List<Guid> topicIds);
        List<Vote> GetVotesByPost(Guid postId);
    }
}