using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.DomainModel;

namespace MVCForum.API
{
    public class TopicAPI : ITopicAPI
    {
        private readonly ITopicRepository _topicRepository;
        private readonly IPostRepository _postRepository;

        public TopicAPI(ITopicRepository topicRepository, IPostRepository postRepository)
        {
            _topicRepository = topicRepository;
            _postRepository = postRepository;
        }

        /// <summary>
        /// Return topics by a specified user that are marked as solved
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IList<Topic> GetSolvedTopicsByMember(Guid memberId)
        {
            return _topicRepository.GetSolvedTopicsByMember(memberId);
        }

        public IList<Topic> GetTopicsByCatId(Guid categoryId)
        {
            return _topicRepository.GetAllTopicsByCategory(categoryId);
        }

        public IList<Topic> GetTopicsByMember(Guid memberId)
        {
            return _topicRepository.GetTopicsByUser(memberId);
        }

        public PagedList<Topic> GetAllTopicsPaged(int pageIndex, int pageSize, int amountToTake)
        {
            return _topicRepository.GetPagedTopicsAll(pageIndex, pageSize, amountToTake);
        }

        public void Create(Topic topic)
        {
            _topicRepository.Add(topic);
        }

        public void Delete(Topic topic)
        {
            _topicRepository.Delete(topic);
        }

        public Topic Get(Guid id)
        {
            return _topicRepository.Get(id);
        }

        public Topic GetTopicBySlug(string slug)
        {
            return _topicRepository.GetTopicBySlug(slug);
        }
    }
}
