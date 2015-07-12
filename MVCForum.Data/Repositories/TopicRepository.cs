using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web.Caching;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Utilities;

namespace MVCForum.Data.Repositories
{
    public partial class TopicRepository : ITopicRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public TopicRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Get all topics
        /// </summary>
        /// <returns></returns>
        public IList<Topic> GetAll(List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Topic.Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .Where(x => allowedCatIds.Contains(x.Category.Id))
                                .ToList();
        }

        /// <summary>
        /// Get the highest viewed topics
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Topic> GetHighestViewedTopics(int amountToTake, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                            .Where(x => x.Pending != true)
                            .Where(x => allowedCatIds.Contains(x.Category.Id))
                            .OrderByDescending(x => x.Views)
                            .Take(amountToTake)
                            .ToList();
        }


        public IList<Topic> GetPopularTopics(DateTime from, DateTime to, int amountToShow, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            var topics = _context.Post
                .Include(x => x.Topic)
                .Include(x => x.Topic.Category)
                .Include(x => x.User)
                .Include(x => x.Topic.Posts)
                .Where(x => allowedCatIds.Contains(x.Topic.Category.Id))
                .DistinctBy(x => x.Topic.Id)
                .OrderByDescending(x => x.Topic.Posts.Count(c => c.DateCreated >= from && c.DateCreated <= to))
                .ThenByDescending(x => x.VoteCount)
                .ThenByDescending(x => x.Topic.Views)
                .Take(amountToShow)
                .Select(x => x.Topic)
                .ToList();

            return topics;
        }

        public IList<Topic> GetTodaysTopics(int amountToTake, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .AsNoTracking()
                        .Where(c => c.CreateDate >= DateTime.Today && c.Pending != true)
                        .Where(x => allowedCatIds.Contains(x.Category.Id))
                        .OrderByDescending(x => x.CreateDate)
                        .Take(amountToTake)
                        .ToList();
        }

        public Topic Add(Topic topic)
        {
            _context.Topic.Add(topic);
            return topic;
        }

        public Topic Get(Guid id)
        {
            var topic = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                            .FirstOrDefault(x => x.Id == id);

            return topic;
        }

        public List<Topic> Get(List<Guid> ids, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Topic
                .Include(x => x.Category)
                .Include(x => x.LastPost.User)
                .Include(x => x.User)
                .Include(x => x.Poll)
                .Where(x => ids.Contains(x.Id) && allowedCatIds.Contains(x.Category.Id))
                .OrderByDescending(x => x.LastPost.DateCreated)
                .ToList();
        }

        public void Delete(Topic item)
        {
            _context.Topic.Remove(item);
        }

        public PagedList<Topic> GetRecentTopics(int pageIndex, int pageSize, int amountToTake, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.AsNoTracking().Count(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id));
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                                .OrderByDescending(x => x.LastPost.DateCreated)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public IList<Topic> GetRecentRssTopics(int amountToTake, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // Get the topics using an efficient query
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .AsNoTracking()
                                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                                .OrderByDescending(s => s.CreateDate)
                                .Take(amountToTake)
                                .ToList();

            return results;
        }

        public IList<Topic> GetTopicsByUser(Guid memberId, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // Get the topics using an efficient
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                                .Where(x => x.User.Id == memberId)
                                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                                .ToList();
            return results;
        }

        public IList<Topic> GetAllTopicsByCategory(Guid categoryId)
        {
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                                .Where(x => x.Category.Id == categoryId)
                                .Where(x => x.Pending != true)
                                .ToList();

            return results;
        }

        public IList<Topic> GetTopicsByLastPost(List<Guid> postIds, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            return _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                    .Where(x => postIds.Contains(x.LastPost.Id) && allowedCatIds.Contains(x.Category.Id))
                    .Where(x => x.Pending != true)
                    .ToList();
        }

        public PagedList<Topic> GetPagedPendingTopics(int pageIndex, int pageSize, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.AsNoTracking().Count(x => x.Pending == true && allowedCatIds.Contains(x.Category.Id));

            // Get the topics using an efficient
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                                .Where(x => x.Pending == true && allowedCatIds.Contains(x.Category.Id))
                                .OrderBy(x => x.LastPost.DateCreated)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public PagedList<Topic> GetPagedTopicsByCategory(int pageIndex, int pageSize, int amountToTake, Guid categoryId)
        {

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.Count(x => x.Category.Id == categoryId);
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                                .Where(x => x.Category.Id == categoryId)
                                .Where(x => x.Pending != true)
                                .OrderByDescending(x => x.IsSticky)
                                .ThenByDescending(x => x.LastPost.DateCreated)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public PagedList<Topic> GetPagedTopicsAll(int pageIndex, int pageSize, int amountToTake, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.AsNoTracking().Count(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id));
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                                .OrderByDescending(x => x.IsSticky)
                                .ThenByDescending(x => x.LastPost.DateCreated)
                                .Take(pageSize)
                                .Skip((pageIndex - 1) * pageSize)
                                .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public PagedList<Topic> SearchTopics(int pageIndex, int pageSize, int amountToTake, List<string> searchTerms, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // We might only want to display the top 100
            // but there might not be 100 topics
            var query = _context.Post
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Topic.LastPost.User)
                            .Include(x => x.Topic.Poll)
                            .Include(x => x.User)
                            .AsNoTracking()
                            .Where(x => x.Pending != true && allowedCatIds.Contains(x.Topic.Category.Id));

            // Loop through each word and see if it's in the post
            foreach (var term in searchTerms)
            {
                var sTerm = term.Trim();
                query = query.Where(x => x.PostContent.ToUpper().Contains(sTerm) || x.Topic.Name.ToUpper().Contains(sTerm));
            }

            // Distinct by the topic id
            var result = query.DistinctBy(x => x.Topic.Id);

            // Get the count
            var total = result.Count();

            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the Posts and then get the topics from the post
            // This is an interim solution, as its flawed due to multiple posts in one topic so the paging might
            // be incorrect if all posts are from one topic.
            var results = result
                        .OrderByDescending(x => x.DateCreated)
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .Select(x => x.Topic)
                        .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public PagedList<Topic> GetMembersActivity(int pageIndex, int pageSize, int amountToTake, Guid memberGuid, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Post.AsNoTracking().Where(x => x.User.Id == memberGuid && x.Pending != true && allowedCatIds.Contains(x.Topic.Category.Id)).DistinctBy(x => x.Topic.Id).Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the Posts and then get the topics from the post
            // This is an interim solution, as its flawed due to multiple posts in one topic so the paging might
            // be incorrect if all posts are from one topic.
            var results = _context.Post
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Topic.LastPost.User)
                            .Include(x => x.Topic.Poll)
                            .Include(x => x.User)
                            .AsNoTracking()
                            .Where(x => x.User.Id == memberGuid && x.Pending != true && allowedCatIds.Contains(x.Topic.Category.Id))
                            .DistinctBy(x => x.Topic.Id)
                            .OrderByDescending(x => x.DateCreated)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Return a paged list
            return new PagedList<Topic>(results.Select(x => x.Topic), pageIndex, pageSize, total);
        }

        public PagedList<Topic> GetTopicsByCsv(int pageIndex, int pageSize, int amountToTake, List<Guid> csv, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // Get the count
            var total = _context.Topic
                                .Join(csv,
                                      topic => topic.Id,
                                      guidFromCsv => guidFromCsv,
                                      (topic, guidFromCsv) => new { topic, guidFromCsv }
                                      ).Count(x => x.guidFromCsv == x.topic.Id && allowedCatIds.Contains(x.topic.Category.Id));

            // Now get the paged stuff
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                .Join(csv,
                        topic => topic.Id,
                        guidFromCsv => guidFromCsv,
                        (topic, guidFromCsv) => new { topic, guidFromCsv }
                    )
                    .Where(x => x.guidFromCsv == x.topic.Id)
                    .Where(x => x.topic.Pending != true && allowedCatIds.Contains(x.topic.Category.Id))
                    .OrderByDescending(x => x.topic.LastPost.DateCreated)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.topic)
                    .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public IList<Topic> GetTopicsByCsv(int amountToTake, List<Guid> topicIds, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            var topics = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                            .Where(x => topicIds.Contains(x.Id))
                            .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                            .OrderByDescending(x => x.LastPost.DateCreated)
                            .Take(amountToTake)
                            .ToList();

            return topics;
        }

        public IList<Topic> GetRssTopicsByCategory(int amountToTake, Guid categoryId)
        {
            var topics = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                            .Where(x => x.Category.Id == categoryId)
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.LastPost.DateCreated)
                            .Take(amountToTake)
                            .ToList();

            return topics;
        }

        public PagedList<Topic> GetPagedTopicsByTag(int pageIndex, int pageSize, int amountToTake, string tag, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.AsNoTracking().Count(e => e.Tags.Any(t => t.Slug == tag) && allowedCatIds.Contains(e.Category.Id));
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .Include(x => x.Tags)
                                .AsNoTracking()
                                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                                .OrderByDescending(x => x.IsSticky)
                                .ThenByDescending(x => x.LastPost.DateCreated)
                                .Where(e => e.Tags.Any(t => t.Slug == tag))
                                .Take(pageSize)
                                .Skip((pageIndex - 1) * pageSize)
                                .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public Topic GetTopicBySlug(string slug)
        {
            return _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                .FirstOrDefault(x => x.Slug == slug);
        }

        public IList<Topic> GetTopicBySlugLike(string slug)
        {
            return _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                            .Where(x => x.Slug.Contains(slug))
                            .ToList();
        }

        public int TopicCount(List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Topic
                .Include(x => x.Category)
                .AsNoTracking()
                .Count(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id));
        }

        /// <summary>
        /// Get all posts that are solutions, by user
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Topic> GetSolvedTopicsByMember(Guid memberId, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .Include(x => x.Posts)
                                .AsNoTracking()
                            .Where(x => x.User.Id == memberId)
                            .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                            .ToList();

            return results.Where(x => x.Posts.Select(p => p.IsSolution).Contains(true)).ToList();
        }
    }
}