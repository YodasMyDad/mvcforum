using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IPostService
    {
        Post SanitizePost(Post post);
        Post GetTopicStarterPost(Guid topicId);
        IEnumerable<Post> GetAll();
        IList<Post> GetLowestVotedPost(int amountToTake);
        IList<Post> GetHighestVotedPost(int amountToTake);
        IList<Post> GetByMember(Guid memberId, int amountToTake);
        PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm, List<Category> allowedCategories);
        PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order);
        PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize);
        Post Add(Post post);
        Post Get(Guid postId);
        IList<Post> GetPostsByTopics(List<Guid> topicIds);
        void SaveOrUpdate(Post post);
        bool Delete(Post post);
        IList<Post> GetSolutionsByMember(Guid memberId);
        int PostCount();
        Post AddNewPost(string postContent, Topic topic, MembershipUser user, out PermissionSet permissions);
    }
}
