using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface ITopicService
    {
        IList<Topic> GetAll();
        IList<Topic> GetHighestViewedTopics(int amountToTake);
        Topic Add(Topic topic);
        PagedList<Topic> GetRecentTopics(int pageIndex, int pageSize, int amountToTake);
        IList<Topic> GetRecentRssTopics(int amountToTake);
        IList<Topic> GetTopicsByUser(Guid memberId);
        PagedList<Topic> GetPagedTopicsByCategory(int pageIndex, int pageSize, int amountToTake, Guid categoryId);
        IList<Topic> GetRssTopicsByCategory(int amountToTake, Guid categoryId);
        PagedList<Topic> GetPagedTopicsByTag(int pageIndex, int pageSize, int amountToTake, string tag);
        PagedList<Topic> SearchTopics(int pageIndex, int pageSize, int amountToTake, string searchTerm);
        IList<Topic> GetSolvedTopicsByMember(Guid memberId);
        Topic GetTopicBySlug(string slug);
        Topic Get(Guid topicId);
        void SaveOrUpdate(Topic topic);
        void Delete(Topic topic);
        int TopicCount();
        Topic AddLastPost(Topic topic, string postContent);
        
        /// <summary>
        /// Mark a topic as solved
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="post"></param>
        /// <param name="marker"></param>
        /// <param name="solutionWriter"></param>
        /// <returns>True if topic has been marked as solved</returns>
        bool SolveTopic(Topic topic, Post post, MembershipUser marker, MembershipUser solutionWriter);
    }
}
