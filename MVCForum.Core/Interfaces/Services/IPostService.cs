using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IPostService
    {
        Post SanitizePost(Post post);
        Post GetTopicStarterPost(Guid topicId);
        IEnumerable<Post> GetAll(List<Category> allowedCategories);
        IList<Post> GetLowestVotedPost(int amountToTake);
        IList<Post> GetHighestVotedPost(int amountToTake);
        IList<Post> GetByMember(Guid memberId, int amountToTake, List<Category> allowedCategories);
        IList<Post> GetReplyToPosts(Post post);
        IList<Post> GetReplyToPosts(Guid postId);
        IEnumerable<Post> GetPostsByFavouriteCount(Guid postsByMemberId, int minAmountOfFavourites);
        IEnumerable<Post> GetPostsFavouritedByOtherMembers(Guid postsByMemberId);
        PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm, List<Category> allowedCategories);
        PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order);
        PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize, List<Category> allowedCategories);
        IList<Post> GetPendingPosts(List<Category> allowedCategories, MembershipRole usersRole);
        int GetPendingPostsCount(List<Category> allowedCategories);
        Post Add(Post post);
        Post Get(Guid postId);
        IList<Post> GetPostsByTopics(List<Guid> topicIds, List<Category> allowedCategories);
        bool Delete(Post post, IUnitOfWork unitOfWork, bool ignoreLastPost);
        IList<Post> GetSolutionsByMember(Guid memberId, List<Category> allowedCategories);
        int PostCount(List<Category> allowedCategories);
        Post AddNewPost(string postContent, Topic topic, MembershipUser user, out PermissionSet permissions);
        string SortSearchField(bool isTopicStarter, Topic topic, IList<TopicTag> tags);
        IList<Post> GetPostsByMember(Guid memberId, List<Category> allowedCategories);
        IList<Post> GetAllSolutionPosts(List<Category> allowedCategories);
        IList<Post> GetPostsByTopic(Guid topicId);
        IEnumerable<Post> GetAllWithTopics(List<Category> allowedCategories);
        bool PassedPostFloodTest(MembershipUser user);
    }
}
