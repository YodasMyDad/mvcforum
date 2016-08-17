using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.General;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface ITopicService
    {
        Topic SanitizeTopic(Topic topic);
        IList<Topic> GetAll(List<Category> allowedCategories);
        IList<SelectListItem> GetAllSelectList(List<Category> allowedCategories, int amount);
        IList<Topic> GetHighestViewedTopics(int amountToTake, List<Category> allowedCategories);
        IList<Topic> GetPopularTopics(DateTime? from, DateTime? to, List<Category> allowedCategories, int amountToShow = 20);
        Topic Add(Topic topic);
        IList<Topic> GetTodaysTopics(int amountToTake, List<Category> allowedCategories);
        PagedList<Topic> GetRecentTopics(int pageIndex, int pageSize, int amountToTake, List<Category> allowedCategories);
        IList<Topic> GetRecentRssTopics(int amountToTake, List<Category> allowedCategories);
        IList<Topic> GetTopicsByUser(Guid memberId, List<Category> allowedCategories);
        IList<Topic> GetTopicsByLastPost(List<Guid> postIds, List<Category> allowedCategories);
        PagedList<Topic> GetPagedTopicsByCategory(int pageIndex, int pageSize, int amountToTake, Guid categoryId);
        PagedList<Topic> GetPagedPendingTopics(int pageIndex, int pageSize, List<Category> allowedCategories);
        IList<Topic> GetPendingTopics(List<Category> allowedCategories, MembershipRole usersRole);
        int GetPendingTopicsCount(List<Category> allowedCategories);
        IList<Topic> GetRssTopicsByCategory(int amountToTake, Guid categoryId);
        PagedList<Topic> GetPagedTopicsByTag(int pageIndex, int pageSize, int amountToTake, string tag, List<Category> allowedCategories);
        IList<Topic> SearchTopics(int amountToTake, string searchTerm, List<Category> allowedCategories);
        PagedList<Topic> GetTopicsByCsv(int pageIndex, int pageSize, int amountToTake, List<Guid> topicIds, List<Category> allowedCategories);
        PagedList<Topic> GetMembersActivity(int pageIndex, int pageSize, int amountToTake, Guid memberGuid, List<Category> allowedCategories);
        IList<Topic> GetTopicsByCsv(int amountToTake, List<Guid> topicIds, List<Category> allowedCategories);
        IList<Topic> GetSolvedTopicsByMember(Guid memberId, List<Category> allowedCategories);
        Topic GetTopicBySlug(string slug);
        Topic Get(Guid topicId);
        List<Topic> Get(List<Guid> topicIds, List<Category> allowedCategories);
        void Delete(Topic topic, IUnitOfWork unitOfWork);
        int TopicCount(List<Category> allowedCategories);
        Post AddLastPost(Topic topic, string postContent);
        List<MarkAsSolutionReminder> GetMarkAsSolutionReminderList(int days);
        /// <summary>
        /// Mark a topic as solved
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="post"></param>
        /// <param name="marker"></param>
        /// <param name="solutionWriter"></param>
        /// <returns>True if topic has been marked as solved</returns>
        bool SolveTopic(Topic topic, Post post, MembershipUser marker, MembershipUser solutionWriter);
        IList<Topic> GetAllTopicsByCategory(Guid categoryId);
        PagedList<Topic> GetPagedTopicsAll(int pageIndex, int pageSize, int amountToTake, List<Category> allowedCategories);
        IList<Topic> GetTopicBySlugLike(string slug);
        bool PassedTopicFloodTest(string topicTitle, MembershipUser user);
    }
}
