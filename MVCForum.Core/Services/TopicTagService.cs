namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Constants;
    using Data.Context;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;
    using Models.Enums;
    using Models.General;
    using Utilities;

    public partial class TopicTagService : ITopicTagService
    {
        private readonly IBadgeService _badgeService;
        private readonly IMvcForumContext _context;
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="badgeService"></param>
        /// <param name="cacheService"></param>
        public TopicTagService(IMvcForumContext context, IBadgeService badgeService, ICacheService cacheService)
        {
            _badgeService = badgeService;
            _cacheService = cacheService;
            _context = context;
        }

        /// <summary>
        /// Get all topic tags
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TopicTag> GetAll()
        {
            return _context.TopicTag.AsNoTracking();
        }

        /// <summary>
        /// Delete tag by name
        /// </summary>
        /// <param name="tagName"></param>
        public void DeleteByName(string tagName)
        {
            var tag = GetTagName(tagName);
            Delete(tag);
        }

        public IList<TopicTag> GetStartsWith(string term, int amountToTake = 4)
        {
            term = StringUtils.SafePlainText(term);
            return _context.TopicTag
                .AsNoTracking()
                .Where(x => x.Tag.StartsWith(term))
                .Take(amountToTake)
                .ToList();
        }

        public IList<TopicTag> GetContains(string term, int amountToTake = 4)
        {
            term = StringUtils.SafePlainText(term);
            return _context.TopicTag
                                                                    .AsNoTracking()
                                                                    .Where(x => x.Tag.ToUpper().Contains(term.ToUpper()))
                                                                    .Take(amountToTake)
                                                                    .ToList();
        }

        /// <summary>
        /// Get tags by topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IEnumerable<TopicTag> GetByTopic(Topic topic)
        {
            return _context.TopicTag
                .Where(x => x.Topics.Contains(topic))
                .ToList();
        }

        /// <summary>
        /// Gets a paged list of tags, grouped if any are duplicate
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedList<TopicTag>> GetPagedGroupedTags(int pageIndex, int pageSize)
        {

            // Get the topics using an efficient
            var query = _context.TopicTag
                            .OrderByDescending(x => x.Tag);


            // Return a paged list
            return await PaginatedList<TopicTag>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets tags by search term, in a paged list and grouped if any are duplicate
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedList<TopicTag>> SearchPagedGroupedTags(string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);

            // Get the topics using an efficient
            var query = _context.TopicTag
                            .Where(x => x.Tag.ToUpper().Contains(search.ToUpper()))
                            .OrderBy(x => x.Tag);

            // Return a paged list
            return await PaginatedList<TopicTag>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        /// <summary>
        /// Create a new topic tag
        /// </summary>
        /// <param name="topicTag"></param>
        /// <returns></returns>
        public TopicTag Add(TopicTag topicTag)
        {
            _context.TopicTag.Add(topicTag);
            return topicTag;
        }

        public TopicTag Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.TopicTag.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.TopicTag.FirstOrDefault(x => x.Id == id));
        }

        public TopicTag Get(string tag)
        {
            tag = StringUtils.SafePlainText(tag);
            var cacheKey = string.Concat(CacheKeys.TopicTag.StartsWith, "Get-", tag);
            return _cacheService.CachePerRequest(cacheKey, () => _context.TopicTag.FirstOrDefault(x => x.Slug.Equals(tag)));
        }

        /// <summary>
        /// Add new tags to a topic, ignore existing ones
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="topic"></param>
        /// <param name="isAllowedToAddTags"></param>
        public void Add(string tags, Topic topic, bool isAllowedToAddTags)
        {
            if (!string.IsNullOrWhiteSpace(tags))
            {
                tags = StringUtils.SafePlainText(tags);

                var newTagNames = tags.ToLower().TrimStart().TrimEnd()
                    .Replace(" ", "-").Split(',')
                    .Select(tag => tag)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct();

                if (topic.Tags == null)
                {
                    topic.Tags = new List<TopicTag>();
                }

                var entityTags = new List<TopicTag>();

                foreach (var newTag in newTagNames)
                {
                    var tag = GetTagName(newTag);
                    if (tag != null)
                    {
                        // Exists
                        entityTags.Add(tag);
                    }
                    else if(isAllowedToAddTags)
                    {
                        // Doesn't exists
                        var nTag = new TopicTag
                        {
                            Tag = newTag,
                            Slug = ServiceHelpers.CreateUrl(newTag)
                        };

                        Add(nTag);
                        entityTags.Add(nTag);
                    }
                }

                topic.Tags = entityTags;

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
                if (topicTag.Topics.Count <= 1)
                {
                    Delete(topicTag);
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
                Delete(topicTag);
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
            var oldTag = GetTagName(safeOldName);
            if (oldTag != null)
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
            var categoryIds = allowedCategories.Select(x => x.Id);
            amount = amount ?? int.MaxValue;

            //var test = _context.TopicTag.SqlQuery("").ToList<TopicTag>();

            var tags = _context.TopicTag
                .Include(x => x.Topics.Select(s => s.Category))
                .AsNoTracking()
                .Select(x => new
                {
                    topictag = x,
                    topiccount = x.Topics.Count(c => categoryIds.Contains(c.Category.Id))
                })
                .Where(x => x.topiccount > 0)
                .OrderByDescending(x => x.topiccount)
                .Take((int)amount);

            return tags.ToDictionary(tag => tag.topictag, tag => tag.topiccount);
        }

        public TopicTag GetTagName(string tag)
        {
            tag = StringUtils.SafePlainText(tag);
            return _context.TopicTag.FirstOrDefault(x => x.Tag == tag);
        }

        public void Delete(TopicTag item)
        {
            _context.TopicTag.Remove(item);
        }
    }
}
