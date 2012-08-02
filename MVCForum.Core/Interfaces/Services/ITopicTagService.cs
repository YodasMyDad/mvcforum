using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface ITopicTagService
    {
        IEnumerable<TopicTag> GetAll();
        void DeleteByName(string tagName);
        IEnumerable<TopicTag> GetByTopic(Topic topic);
        PagedList<TopicTag> GetPagedGroupedTags(int pageIndex, int pageSize);
        PagedList<TopicTag> SearchPagedGroupedTags(string search, int pageIndex, int pageSize);
        TopicTag Add(TopicTag topic);
        void Add(string tags, Topic topic);
        void DeleteByTopic(Topic topic);
        void DeleteTags(IEnumerable<TopicTag> tags);
        void UpdateTagNames(string tagName, string oldTagName);
        Dictionary<string, int> GetPopularTags(int? amount);
    }
}
