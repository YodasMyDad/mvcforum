﻿using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using System.Linq;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class TopicTagService : ITopicTagService
    {
        private readonly ITopicTagRepository _tagRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly ICategoryService _categoryService;
        private readonly IBadgeService _badgeService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="tagRepository"></param>
        /// <param name="topicRepository"></param>
        /// <param name="categoryService"></param>
        public TopicTagService(ITopicTagRepository tagRepository, ITopicRepository topicRepository, ICategoryService categoryService, IBadgeService badgeService)
        {
            _tagRepository = tagRepository;
            _topicRepository = topicRepository;
            _categoryService = categoryService;
            _badgeService = badgeService;
        }

        /// <summary>
        /// Get all topic tags
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TopicTag> GetAll()
        {
            return _tagRepository.GetAll();
        }

        /// <summary>
        /// Delete tag by name
        /// </summary>
        /// <param name="tagName"></param>
        public void DeleteByName(string tagName)
        {
            var tag = _tagRepository.GetTagName(tagName);
            _tagRepository.Delete(tag);
        }

        public IList<TopicTag> GetStartsWith(string term, int amountToTake = 4)
        {
            term = StringUtils.SafePlainText(term);
            return _tagRepository.GetStartsWith(term, amountToTake);
        }

        public IList<TopicTag> GetContains(string term, int amountToTake = 4)
        {
            term = StringUtils.SafePlainText(term);
            return _tagRepository.GetContains(term, amountToTake);
        }

        /// <summary>
        /// Get tags by topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IEnumerable<TopicTag> GetByTopic(Topic topic)
        {
            return _tagRepository.GetByTopic(topic);
        }

        /// <summary>
        /// Gets a paged list of tags, grouped if any are duplicate
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<TopicTag> GetPagedGroupedTags(int pageIndex, int pageSize)
        {
            return _tagRepository.GetPagedGroupedTags(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets tags by search term, in a paged list and grouped if any are duplicate
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<TopicTag> SearchPagedGroupedTags(string search, int pageIndex, int pageSize)
        {
            return _tagRepository.SearchPagedGroupedTags(StringUtils.SafePlainText(search), pageIndex, pageSize);            
        }

        /// <summary>
        /// Create a new topic tag
        /// </summary>
        /// <param name="topicTag"></param>
        /// <returns></returns>
        public TopicTag Add(TopicTag topicTag)
        {
            return _tagRepository.Add(topicTag);
        }

        public TopicTag Get(Guid tag)
        {
            return _tagRepository.Get(tag);
        }

        public TopicTag Get(string tag)
        {
            tag = StringUtils.SafePlainText(tag);
            return _tagRepository.Get(tag);
        }

        /// <summary>
        /// Add new tags to a topic, ignore existing ones
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="topic"></param>
        public void Add(string tags, Topic topic)
        {
            if(!string.IsNullOrEmpty(tags))
            {
                tags = StringUtils.SafePlainText(tags);

                var splitTags = tags.Replace(" ", "").Split(',');
              
                if(topic.Tags == null)
                {
                    topic.Tags = new List<TopicTag>();
                }                   

                var newTagNames = splitTags.Select(tag => tag);
                var newTags = new List<TopicTag>();
                var existingTags = new List<TopicTag>();

                foreach (var newTag in newTagNames.Distinct())
                {
                    var tag = _tagRepository.GetTagName(newTag);
                    if (tag != null)
                    {
                        // Exists
                        existingTags.Add(tag);
                    }
                    else
                    {
                        // Doesn't exists
                        var nTag = new TopicTag
                            {
                                Tag = newTag,
                                Slug = ServiceHelpers.CreateUrl(newTag)
                            };

                        _tagRepository.Add(nTag);
                        newTags.Add(nTag);
                    }
                }

                newTags.AddRange(existingTags);
                topic.Tags = newTags;

                // Fire the tag badge check
                _badgeService.ProcessBadge(BadgeType.Tag, topic.User);
            }
        }

        /// <summary>
        /// Delete all tags by topic, ignores tags used by other topics
        /// </summary>
        /// <param name="topic"></param>
        public void DeleteByTopic(Topic topic)
        {
                var tags = topic.Tags;
                foreach (var topicTag in tags)
                {
                    // If this tag has a count of topics greater than this one topic
                    // then its tagged by more topics so don't delete
                    if(topicTag.Topics.Count() <= 1)
                    {
                        _tagRepository.Delete(topicTag);   
                    }
                }
        }

        /// <summary>
        /// Delete a list of tags
        /// </summary>
        /// <param name="tags"></param>
        public void DeleteTags(IEnumerable<TopicTag> tags)
        {
                foreach (var topicTag in tags)
                {
                    _tagRepository.Delete(topicTag);
                }
        }

        /// <summary>
        /// Update an existing tag name
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="oldTagName"></param>
        public void UpdateTagNames(string tagName, string oldTagName)
        {
                // run new and old names through safe filter first
                var safeNewName = StringUtils.SafePlainText(tagName);
                var safeOldName = StringUtils.SafePlainText(oldTagName);

                // Now remove any spaces
                safeNewName = safeNewName.Replace(" ", "-");

                // get all the old tags by name
                var oldTag = _tagRepository.GetTagName(safeOldName);
                if(oldTag != null)
                {
                    oldTag.Tag = safeNewName;
                }
        }

        /// <summary>
        /// Get a specified amount of the most popular tags, ordered by use amount
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public Dictionary<TopicTag, int> GetPopularTags(int? amount, List<Category> allowedCategories)
        {
            return _tagRepository.GetPopularTags(amount, allowedCategories);
        }
    }
}
