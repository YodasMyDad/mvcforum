using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IPostRepository
    {
        IEnumerable<Post> GetAll(List<Category> allowedCategories);
        Post GetTopicStarterPost(Guid topicId);
        IEnumerable<Post> GetAllWithTopics(List<Category> allowedCategories);
        IList<Post> GetLowestVotedPost(int amountToTake);
        IList<Post> GetHighestVotedPost(int amountToTake);
        IList<Post> GetByMember(Guid memberId, int amountToTake, List<Category> allowedCategories);
        IList<Post> GetSolutionsByMember(Guid memberId, List<Category> allowedCategories);
        IList<Post> GetPostsByTopic(Guid topicId);
        IList<Post> GetPostsByTopics(List<Guid> topicId, List<Category> allowedCategories);
        PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize);
        PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order);
        IList<Post> GetPostsByMember(Guid memberId, List<Category> allowedCategories);
        IList<Post> GetAllSolutionPosts(List<Category> allowedCategories);
        PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, List<string> searchTerms, List<Category> allowedCategories);
        int PostCount(List<Category> allowedCategories);
        Post Add(Post item);
        Post Get(Guid id);
        void Delete(Post item);
        void Update(Post item);
    }
}
