using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

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
        IEnumerable<Post> GetPostsByFavouriteCount(Guid postsByMemberId, int minAmountOfFavourites);
        IEnumerable<Post> GetPostsFavouritedByOtherMembers(Guid postsByMemberId);
        PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm, List<Category> allowedCategories);
        PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order);
        PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize);
        int GetPendingPostsCount();
        Post Add(Post post);
        Post Get(Guid postId);
        IList<Post> GetPostsByTopics(List<Guid> topicIds, List<Category> allowedCategories);
        void SaveOrUpdate(Post post);
        bool Delete(Post post, bool isTopicDelete = false);
        IList<Post> GetSolutionsByMember(Guid memberId, List<Category> allowedCategories);
        int PostCount(List<Category> allowedCategories);
        Post AddNewPost(string postContent, Topic topic, MembershipUser user, out PermissionSet permissions);
        string SortSearchField(bool isTopicStarter, Topic topic, IList<TopicTag> tags);
    }
}
