namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.Entities;
    using Models.General;

    public partial interface ITopicTagService : IContextService
    {
        IEnumerable<TopicTag> GetAll();
        void DeleteByName(string tagName);
        IList<TopicTag> GetStartsWith(string term, int amountToTake = 4);
        IList<TopicTag> GetContains(string term, int amountToTake = 4);
        IEnumerable<TopicTag> GetByTopic(Topic topic);
        Task<PaginatedList<TopicTag>> GetPagedGroupedTags(int pageIndex, int pageSize);
        Task<PaginatedList<TopicTag>> SearchPagedGroupedTags(string search, int pageIndex, int pageSize);
        TopicTag Add(TopicTag tag);
        TopicTag Get(Guid tag);
        TopicTag Get(string tag);
        void Add(string[] tags, Topic tag, bool isAllowedToAddTags);
        IEnumerable<string> CreateTagsFromCsv(string tags);
        bool HasNewTags(IEnumerable<string> tags);
        void DeleteByTopic(Topic tag);
        void DeleteTags(IEnumerable<TopicTag> tags);
        void UpdateTagNames(string tagName, string oldTagName);
        Dictionary<TopicTag, int> GetPopularTags(int? amount, List<Category> allowedCategories);
        TopicTag GetTagName(string tag);
        void Delete(TopicTag item);
    }
}