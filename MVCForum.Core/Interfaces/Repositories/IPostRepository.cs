using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IPostRepository
    {
        IEnumerable<Post> GetAll();
        Post GetTopicStarterPost(Guid topicId);
        IEnumerable<Post> GetAllWithTopics();
        IList<Post> GetLowestVotedPost(int amountToTake);
        IList<Post> GetHighestVotedPost(int amountToTake);
        IList<Post> GetByMember(Guid memberId, int amountToTake);
        IList<Post> GetSolutionsByMember(Guid memberId);
        IList<Post> GetPostsByTopic(Guid topicId);
        IList<Post> GetPostsByTopics(List<Guid> topicId);
        PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize);
        PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order);
        IList<Post> GetPostsByMember(Guid memberId);
        IList<Post> GetAllSolutionPosts();
        PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, List<string> searchTerms, List<Category> allowedCategories); 

        int PostCount();

        Post Add(Post item);
        Post Get(Guid id);
        void Delete(Post item);
        void Update(Post item);
    }
}
