using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface ITopicTagRepository
    {
        IEnumerable<TopicTag> GetAll();
        Dictionary<TopicTag, int> GetPopularTags(int? amountToTake, List<Category> allowedCategories);
        TopicTag GetTagName(string tag);
        PagedList<TopicTag> GetPagedGroupedTags(int pageIndex, int pageSize);
        PagedList<TopicTag> SearchPagedGroupedTags(string search, int pageIndex, int pageSize);
        IEnumerable<TopicTag> GetByTopic(Topic topic);
        IList<TopicTag> GetStartsWith(string term, int amountToTake);
        IList<TopicTag> GetContains(string term, int amountToTake);
        TopicTag Add(TopicTag item);
        TopicTag Get(Guid id);
        TopicTag Get(string tag);
        void Delete(TopicTag item);
        void Update(TopicTag item);
    }
}
