using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface IPostRepository
    {
        IEnumerable<Post> GetAll();
        Post GetTopicStarterPost(Guid topicId);
        IEnumerable<Post> GetAllWithTopics();
        IList<Post> GetLowestVotedPost(int amountToTake);
        IList<Post> GetHighestVotedPost(int amountToTake);
        IList<Post> GetByMember(Guid memberId, int amountToTake);
        IList<Post> GetSolutionsByMember(Guid memberId);
        IList<Post> GetPostsByTopic(Guid topicId);
        PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order);
        IList<Post> GetPostsByMember(Guid memberId);
        IList<Post> GetAllSolutionPosts();
        PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm); 

        int PostCount();

        Post Add(Post item);
        Post Get(Guid id);
        void Delete(Post item);
        void Update(Post item);
    }
}
