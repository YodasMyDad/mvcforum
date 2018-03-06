namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Models.Entities;
    using Models.General;
    using Pipeline;

    public partial interface ITopicService : IContextService
    {
        IList<Topic> GetAll(List<Category> allowedCategories);
        IList<SelectListItem> GetAllSelectList(List<Category> allowedCategories, int amount);
        IList<Topic> GetHighestViewedTopics(int amountToTake, List<Category> allowedCategories);

        IList<Topic> GetPopularTopics(DateTime? from, DateTime? to, List<Category> allowedCategories,
            int amountToShow = 20);

        Task<IPipelineProcess<Topic>> Create(Topic topic, HttpPostedFileBase[] files, string tags, bool subscribe, string postContent, Post post);

        Task<IPipelineProcess<Topic>> Edit(Topic topic, HttpPostedFileBase[] files, string tags, bool subscribe, string postContent, string originalTopicName, List<PollAnswer> pollAnswers, int closePollAfterDays);

        IList<Topic> GetTodaysTopics(int amountToTake, List<Category> allowedCategories);

        Task<PaginatedList<Topic>> GetRecentTopics(int pageIndex, int pageSize, int amountToTake,
            List<Category> allowedCategories);

        IList<Topic> GetRecentRssTopics(int amountToTake, List<Category> allowedCategories);
        IList<Topic> GetTopicsByUser(Guid memberId, List<Category> allowedCategories);
        IList<Topic> GetTopicsByLastPost(List<Guid> postIds, List<Category> allowedCategories);
        Task<PaginatedList<Topic>> GetPagedTopicsByCategory(int pageIndex, int pageSize, int amountToTake, Guid categoryId);
        Task<PaginatedList<Topic>> GetPagedPendingTopics(int pageIndex, int pageSize, List<Category> allowedCategories);
        IList<Topic> GetPendingTopics(List<Category> allowedCategories, MembershipRole usersRole);
        int GetPendingTopicsCount(List<Category> allowedCategories);
        IList<Topic> GetRssTopicsByCategory(int amountToTake, Guid categoryId);

        Task<PaginatedList<Topic>> GetPagedTopicsByTag(int pageIndex, int pageSize, int amountToTake, string tag,
            List<Category> allowedCategories);

        IList<Topic> SearchTopics(int amountToTake, string searchTerm, List<Category> allowedCategories);

        Task<PaginatedList<Topic>> GetTopicsByCsv(int pageIndex, int pageSize, int amountToTake, List<Guid> topicIds,
            List<Category> allowedCategories);

        Task<PaginatedList<Topic>> GetMembersActivity(int pageIndex, int pageSize, int amountToTake, Guid memberGuid,
            List<Category> allowedCategories);

        IList<Topic> GetTopicsByCsv(int amountToTake, List<Guid> topicIds, List<Category> allowedCategories);
        IList<Topic> GetSolvedTopicsByMember(Guid memberId, List<Category> allowedCategories);
        Topic GetTopicBySlug(string slug);
        Topic Get(Guid topicId);
        List<Topic> Get(List<Guid> topicIds, List<Category> allowedCategories);
        Task<IPipelineProcess<Topic>> Delete(Topic topic);
        int TopicCount(List<Category> allowedCategories);

        List<MarkAsSolutionReminder> GetMarkAsSolutionReminderList(int days);

        /// <summary>
        ///     Mark a topic as solved
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="post"></param>
        /// <param name="marker"></param>
        /// <param name="solutionWriter"></param>
        /// <returns>True if topic has been marked as solved</returns>
        Task<bool> SolveTopic(Topic topic, Post post, MembershipUser marker, MembershipUser solutionWriter);

        IList<Topic> GetAllTopicsByCategory(Guid categoryId);

        Task<PaginatedList<Topic>> GetPagedTopicsAll(int pageIndex, int pageSize, int amountToTake,
            List<Category> allowedCategories);

        IList<Topic> GetTopicBySlugLike(string slug);
        bool PassedTopicFloodTest(string topicTitle, MembershipUser user);
    }
}