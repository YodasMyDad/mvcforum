using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class TopicTagRepository : ITopicTagRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public TopicTagRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public IEnumerable<TopicTag> GetAll()
        {
            return _context.TopicTag.AsNoTracking();
        }

        public Dictionary<TopicTag, int> GetPopularTags(int? amountToTake, List<Category> allowedCategories)
        {
            var categoryIds = allowedCategories.Select(x => x.Id);
            amountToTake = amountToTake ?? int.MaxValue;

            //var test = _context.TopicTag.SqlQuery("").ToList<TopicTag>();

            var tags = _context.TopicTag
                .Include(x => x.Topics.Select(s => s.Category))
                .AsNoTracking()
                .Select(x => new
                {
                    topics = x.Topics.Where(c => categoryIds.Contains(c.Category.Id)),
                    topictag = x,
                    topiccount = x.Topics.Count(c => categoryIds.Contains(c.Category.Id))
                })
                .Where(x => x.topiccount > 0)
                .OrderByDescending(x => x.topiccount)
                .Take((int) amountToTake);

            return tags.ToDictionary(tag => tag.topictag, tag => tag.topiccount);
        }

        public TopicTag GetTagName(string tag)
        {
            return _context.TopicTag.FirstOrDefault(x => x.Tag == tag);
        }

        public PagedList<TopicTag> GetPagedGroupedTags(int pageIndex, int pageSize)
        {
            var totalCount = _context.TopicTag.Count();

            // Get the topics using an efficient
            var results = _context.TopicTag
                                .OrderByDescending(x => x.Tag)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();


            // Return a paged list
            return new PagedList<TopicTag>(results, pageIndex, pageSize, totalCount);
        }

        public PagedList<TopicTag> SearchPagedGroupedTags(string search, int pageIndex, int pageSize)
        {
            var totalCount = _context.TopicTag.Count(x => x.Tag.Contains(search));

            // Get the topics using an efficient
            var results = _context.TopicTag
                                .Where(x => x.Tag.Contains(search))
                                .OrderBy(x => x.Tag)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();


            // Return a paged list
            return new PagedList<TopicTag>(results, pageIndex, pageSize, totalCount);
        }

        public IEnumerable<TopicTag> GetByTopic(Topic topic)
        {
            return _context.TopicTag
                .Where(x => x.Topics.Contains(topic))
                .ToList();
        }

        public IList<TopicTag> GetStartsWith(string term, int amountToTake)
        {
            return _context.TopicTag
                .AsNoTracking()
                .Where(x => x.Tag.StartsWith(term))
                .Take(amountToTake)
                .ToList();
        }

        public IList<TopicTag> GetContains(string term, int amountToTake)
        {
            return _context.TopicTag
                .AsNoTracking()
                .Where(x => x.Tag.ToUpper().Contains(term.ToUpper()))
                .Take(amountToTake)
                .ToList();
        }

        public TopicTag Add(TopicTag topicTag)
        {
            _context.TopicTag.Add(topicTag);
            return topicTag;
        }

        public TopicTag Get(Guid id)
        {
            return _context.TopicTag.FirstOrDefault(x => x.Id == id);
        }

        public TopicTag Get(string tag)
        {
            return _context.TopicTag.FirstOrDefault(x => x.Slug.Equals(tag));
        }

        public void Delete(TopicTag item)
        {
            _context.TopicTag.Remove(item);
        }

        public void Update(TopicTag item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.TopicTag.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;  
        }
    }
}
