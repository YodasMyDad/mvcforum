using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface ITopicRepository
    {
        IList<Topic> GetAll(List<Category> allowedCategories);
        IList<Topic> GetHighestViewedTopics(int amountToTake, List<Category> allowedCategories);
        IList<Topic> GetPopularTopics(DateTime from, DateTime to, int amountToShow, List<Category> allowedCategories);
        IList<Topic> GetTodaysTopics(int amountToTake, List<Category> allowedCategories);
        IList<Topic> GetSolvedTopicsByMember(Guid memberId, List<Category> allowedCategories);
        PagedList<Topic> GetRecentTopics(int pageIndex, int pageSize, int amountToTake, List<Category> allowedCategories);
        IList<Topic> GetRecentRssTopics(int amountToTake, List<Category> allowedCategories);
        IList<Topic> GetTopicsByUser(Guid memberId, List<Category> allowedCategories);
        IList<Topic> GetAllTopicsByCategory(Guid categoryId);
        IList<Topic> GetTopicsByLastPost(List<Guid> postIds, List<Category> allowedCategories);
        PagedList<Topic> GetPagedPendingTopics(int pageIndex, int pageSize, List<Category> allowedCategories);
        PagedList<Topic> GetPagedTopicsByCategory(int pageIndex, int pageSize, int amountToTake, Guid categoryId);
        PagedList<Topic> GetPagedTopicsAll(int pageIndex, int pageSize, int amountToTake, List<Category> allowedCategories);
        PagedList<Topic> SearchTopics(int pageIndex, int pageSize, int amountToTake, List<string> searchTerm, List<Category> allowedCategories);
        PagedList<Topic> GetTopicsByCsv(int pageIndex, int pageSize, int amountToTake, List<Guid> topicIds, List<Category> allowedCategories);
        IList<Topic> GetTopicsByCsv(int amountToTake, List<Guid> topicIds, List<Category> allowedCategories);
        IList<Topic> GetRssTopicsByCategory(int amountToTake, Guid categoryId);
        PagedList<Topic> GetPagedTopicsByTag(int pageIndex, int pageSize, int amountToTake, string tag, List<Category> allowedCategories);
        PagedList<Topic> GetMembersActivity(int pageIndex, int pageSize, int amountToTake, Guid memberGuid, List<Category> allowedCategories);
        Topic GetTopicBySlug(string slug);
        IList<Topic> GetTopicBySlugLike(string slug);
        int TopicCount(List<Category> allowedCategories);
        Topic Add(Topic item);
        Topic Get(Guid id);
        List<Topic> Get(List<Guid> ids, List<Category> allowedCategories);
        void Delete(Topic item);
    }
}
