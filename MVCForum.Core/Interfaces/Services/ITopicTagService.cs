using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface ITopicTagService
    {
        IEnumerable<TopicTag> GetAll();
        void DeleteByName(string tagName);
        IList<TopicTag> GetStartsWith(string term, int amountToTake = 4);
        IList<TopicTag> GetContains(string term, int amountToTake = 4);
        IEnumerable<TopicTag> GetByTopic(Topic topic);
        PagedList<TopicTag> GetPagedGroupedTags(int pageIndex, int pageSize);
        PagedList<TopicTag> SearchPagedGroupedTags(string search, int pageIndex, int pageSize);
        TopicTag Add(TopicTag tag);
        TopicTag Get(Guid tag);
        TopicTag Get(string tag);
        void Add(string tags, Topic tag);
        void DeleteByTopic(Topic tag);
        void DeleteTags(IEnumerable<TopicTag> tags);
        void UpdateTagNames(string tagName, string oldTagName);
        Dictionary<TopicTag, int> GetPopularTags(int? amount, List<Category> allowedCategories);
        TopicTag GetTagName(string tag);
        void Delete(TopicTag item);
    }
}
