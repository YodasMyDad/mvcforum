using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Lucene;
using MVCForum.Lucene.LuceneModel;

namespace MVCForum.Services
{
    public class LuceneService : ILuceneService
    {

        private readonly IPostRepository _postRepository;
        private readonly ITopicRepository _topicRepository;

        public LuceneService(IPostRepository postRepository, ITopicRepository topicRepository)
        {
            _postRepository = postRepository;
            _topicRepository = topicRepository;
        }

        /// <summary>
        /// Updates the entire index (Could be slow depending on size)
        /// </summary>
        public void UpdateIndex()
        {
            var allPosts = _postRepository.GetAllWithTopics();
            var mappedPosts = allPosts.Select(post => MapToModel(post, post.Topic.Name));
            GoLucene.AddUpdateLuceneIndex(mappedPosts);
        }

        /// <summary>
        /// Checks the index exists, and if not creates it
        /// </summary>
        public bool CheckIndexExists()
        {
            return Directory.Exists(GoLucene._luceneDir);
            //if(!Directory.Exists(GoLucene._luceneDir)) Directory.CreateDirectory(GoLucene._luceneDir);
        }

        /// <summary>
        /// Add / Update an item in the index
        /// </summary>
        /// <param name="luceneSearchModel"></param>
        public void AddUpdate(LuceneSearchModel luceneSearchModel)
        {
            GoLucene.AddUpdateLuceneIndex(luceneSearchModel);
        }

        /// <summary>
        /// Delete the entire index
        /// </summary>
        public void DeleteEntireIndex()
        {
            GoLucene.ClearLuceneIndex();
        }

        /// <summary>
        /// Delete an item from the index
        /// </summary>
        /// <param name="id"></param>
        public void Delete(Guid id)
        {
            GoLucene.ClearLuceneIndexRecord(id);
        }

        /// <summary>
        /// Optimise the index
        /// </summary>
        public void OptimiseIndex()
        {
            GoLucene.Optimize();
        }

        /// <summary>
        /// Map a post to the Lucene Model
        /// </summary>
        /// <param name="post"></param>
        /// <param name="topicName"> </param>
        /// <returns></returns>
        public LuceneSearchModel MapToModel(Post post, string topicName)
        {
            return new LuceneSearchModel
                {
                    Id = post.Id,
                    PostContent = post.PostContent,
                    TopicName = topicName
                };
        }

        /// <summary>
        /// Map a topic to the Lucene Model
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public LuceneSearchModel MapToModel(Topic topic, string content)
        {
            return new LuceneSearchModel
            {
                Id = topic.Id,
                PostContent = content,
                TopicName = topic.Name
            };
        }

        /// <summary>
        /// Search the index by search term and specific field
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public IEnumerable<LuceneSearchModel> Search(string searchTerm, string fieldName)
        {
            return GoLucene.Search(searchTerm, fieldName);
        }

        /// <summary>
        /// Search the index by search term
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public IEnumerable<LuceneSearchModel> Search(string searchTerm)
        {
            return GoLucene.Search(searchTerm);
        }

        /// <summary>
        /// Get all records from the index
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LuceneSearchModel> GetAll()
        {
            return GoLucene.GetAllIndexRecords();
        }
    }
}
