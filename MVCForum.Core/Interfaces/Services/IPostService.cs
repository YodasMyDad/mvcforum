namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using Models.Entities;
    using Models.Enums;
    using Models.General;
    using Pipeline;

    public partial interface IPostService : IContextService
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

        Task<PaginatedList<Post>> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm,
            List<Category> allowedCategories);

        Task<PaginatedList<Post>> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId,
            PostOrderBy order);

        Task<PaginatedList<Post>> GetPagedPendingPosts(int pageIndex, int pageSize, List<Category> allowedCategories);
        IList<Post> GetPendingPosts(List<Category> allowedCategories, MembershipRole usersRole);
        int GetPendingPostsCount(List<Category> allowedCategories);

        Task<IPipelineProcess<Post>> Create(string postContent, Topic topic, MembershipUser user, HttpPostedFileBase[] files, bool isTopicStarter, Guid? replyTo);
        Task<IPipelineProcess<Post>> Create(Post post, HttpPostedFileBase[] files, bool isTopicStarter, Guid? replyTo);
        Task<IPipelineProcess<Post>> Edit(Post post, HttpPostedFileBase[] files, bool isTopicStarter, string postedTopicName, string postedContent);
        Task<IPipelineProcess<Post>> Move(Post post, Guid? newTopicId, string newTopicTitle, bool moveReplyToPosts);

        Post Initialise(string postContent, Topic topic, MembershipUser user);
        Post Get(Guid postId);
        IList<Post> GetPostsByTopics(List<Guid> topicIds, List<Category> allowedCategories);
        Task<IPipelineProcess<Post>> Delete(Post post, bool ignoreLastPost);
        IList<Post> GetSolutionsByMember(Guid memberId, List<Category> allowedCategories);
        int PostCount(List<Category> allowedCategories);
        IList<Post> GetPostsByMember(Guid memberId, List<Category> allowedCategories);
        IList<Post> GetAllSolutionPosts(List<Category> allowedCategories);
        IList<Post> GetPostsByTopic(Guid topicId);
        IEnumerable<Post> GetAllWithTopics(List<Category> allowedCategories);
        bool PassedPostFloodTest(MembershipUser user);
    }
}