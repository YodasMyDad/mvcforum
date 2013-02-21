using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var allPosts = _postRepository.GetAllWithTopics().ToList();
            var mappedPosts = allPosts.Select(MapToModel);
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
        /// <returns></returns>
        public LuceneSearchModel MapToModel(Post post)
        {
            var model = new LuceneSearchModel
                {
                    Id = post.Id,
                    PostContent = post.PostContent,
                    DateCreated = post.DateCreated,
                    TopicId = post.Topic.Id,
                    TopicUrl = post.Topic.NiceUrl,
                    Username = post.User.UserName,
                    UserId = post.User.Id
                };
            if(post.IsTopicStarter)
            {
                model.TopicName = post.Topic.Name;
            }
            return model;
        }

        /// <summary>
        /// Map a topic to the Lucene Model
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public LuceneSearchModel MapToModel(Topic topic)
        {
            return new LuceneSearchModel
            {
                Id = topic.LastPost.Id,
                PostContent = topic.LastPost.PostContent,
                TopicName = topic.Name,
                DateCreated = topic.LastPost.DateCreated,
                TopicId = topic.Id,
                Username = topic.LastPost.User.UserName,
                UserId = topic.LastPost.User.Id,
                TopicUrl = topic.NiceUrl
            };
        }

        /// <summary>
        /// Search the index by search term and specific field
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="fieldName"></param>
        /// <param name="doFuzzySearch"></param>
        /// <returns></returns>
        public IEnumerable<LuceneSearchModel> Search(string searchTerm, string fieldName, bool doFuzzySearch = false)
        {
            return GoLucene.Search(searchTerm, fieldName, doFuzzySearch);
        }

        /// <summary>
        /// Search the index by search term
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="doFuzzySearch"></param>
        /// <returns></returns>
        public IEnumerable<LuceneSearchModel> Search(string searchTerm, bool doFuzzySearch = false)
        {
            return GoLucene.Search(searchTerm, string.Empty, doFuzzySearch);
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
