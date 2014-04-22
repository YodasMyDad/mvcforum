using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface ITopicTagRepository
    {
        IEnumerable<TopicTag> GetAll();
        Dictionary<string, int> GetPopularTags(int? amountToTake);
        TopicTag GetTagName(string tag);
        PagedList<TopicTag> GetPagedGroupedTags(int pageIndex, int pageSize);
        PagedList<TopicTag> SearchPagedGroupedTags(string search, int pageIndex, int pageSize);
        IEnumerable<TopicTag> GetByTopic(Topic topic);
        TopicTag Add(TopicTag item);
        TopicTag Get(Guid id);
        void Delete(TopicTag item);
        void Update(TopicTag item);
    }
}
