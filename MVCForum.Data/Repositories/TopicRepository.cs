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
        public IList<Topic> GetAll()
        {
            return _context.Topic.ToList();
        }

        /// <summary>
        /// Get the highest viewed topics
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Topic> GetHighestViewedTopics(int amountToTake)
        {
            return _context.Topic
                    .Include(x => x.User)
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.Views)
                            .Take(amountToTake)
                            .ToList();
        }

        public IList<Topic> GetTodaysTopics(int amountToTake)
        {
            return _context.Topic
                        .Include(x => x.User)
                        .Where(c => c.CreateDate >= DateTime.Today && c.Pending != true)
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
            return _context.Topic
                            .Include(x => x.Posts)
                            .Include(x => x.LastPost)
                            .Include(x => x.User)
                            .FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Topic item)
        {
            _context.Topic.Remove(item);
        }

        public void Update(Topic item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.Topic.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;
        }

        public PagedList<Topic> GetRecentTopics(int pageIndex, int pageSize, int amountToTake)
        {

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _context.Topic
                                //.Include(x => x.Posts)
                                .Include(x => x.LastPost)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.Category)
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Where(x => x.Pending != true)
                                .OrderByDescending(x => x.LastPost.DateCreated)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public IList<Topic> GetRecentRssTopics(int amountToTake)
        {
            // Get the topics using an efficient
            var results = _context.Topic
                                //.Include(x => x.Posts)
                                .Include(x => x.LastPost)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.Category)
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Where(x => x.Pending != true)
                                .OrderByDescending(s => s.CreateDate)
                                .Take(amountToTake)
                                .ToList();
            return results;
        }

        public IList<Topic> GetTopicsByUser(Guid memberId)
        {
            // Get the topics using an efficient
            var results = _context.Topic
                                .Where(x => x.User.Id == memberId)
                                .Where(x => x.Pending != true)
                                .ToList();
            return results;
        }

        public IList<Topic> GetAllTopicsByCategory(Guid categoryId)
        {
            var results = _context.Topic
                                //.Include(x => x.Posts)
                                .Include(x => x.LastPost)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.Category)
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Where(x => x.Category.Id == categoryId)
                                .Where(x => x.Pending != true)
                                .ToList();
            return results;
        }

        public PagedList<Topic> GetPagedPendingTopics(int pageIndex, int pageSize)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.Count(x => x.Pending == true);

            // Get the topics using an efficient
            var results = _context.Topic
                                .Include(x => x.LastPost)
                                .Include(x => x.LastPost.User)
                                .Where(x => x.Pending == true)
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
                                //.Include(x => x.Posts)
                                .Include(x => x.LastPost)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.Category)
                                .Include(x => x.Posts.Select(v => v.Votes))
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

        public PagedList<Topic> GetPagedTopicsAll(int pageIndex, int pageSize, int amountToTake)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.Count();
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _context.Topic
                                //.Include(x => x.Posts)
                                .Include(x => x.LastPost)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.Category)
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Where(x => x.Pending != true)
                                .OrderByDescending(x => x.IsSticky)
                                .ThenByDescending(x => x.LastPost.DateCreated)
                                .Take(pageSize)
                                .Skip((pageIndex - 1) * pageSize)
                                .ToList();


            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public PagedList<Topic> SearchTopics(int pageIndex, int pageSize, int amountToTake, string searchTerm)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Post.Count(x => x.PostContent.Contains(searchTerm) | x.Topic.Name.Contains(searchTerm));
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the Posts and then get the topics from the post
            // This is an interim solution, as its flawed due to multiple posts in one topic so the paging might
            // be incorrect if all posts are from one topic.
            var results = _context.Post
                            .Include(x => x.Topic)
                            .Include(x => x.User)
                            .Include(x => x.Votes)
                            .Where(x => x.PostContent.Contains(searchTerm) | x.Topic.Name.Contains(searchTerm))
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.DateCreated)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(x => x.Topic)
                            .ToList();


            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public PagedList<Topic> GetTopicsByCsv(int pageIndex, int pageSize, int amountToTake, List<Guid> csv)
        {
            // Get the count
            var total = _context.Topic
                                .Join(csv,
                                      topic => topic.Id,
                                      guidFromCsv => guidFromCsv,
                                      (topic, guidFromCsv) => new { topic, guidFromCsv }
                                      ).Count(x => x.guidFromCsv == x.topic.Id);

            // Now get the paged stuff
            var results = _context.Topic
                .Join(csv,
                        topic => topic.Id,
                        guidFromCsv => guidFromCsv,
                        (topic, guidFromCsv) => new { topic, guidFromCsv }
                    )
                    .Where(x => x.guidFromCsv == x.topic.Id)
                    .Where(x => x.topic.Pending != true)
                    .OrderByDescending(x => x.topic.LastPost.DateCreated)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.topic)
                    .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public IList<Topic> GetTopicsByCsv(int amountToTake, List<Guid> topicIds)
        {

            return _context.Topic
                            .Include(x => x.LastPost)
                            .Include(x => x.LastPost.User)
                            .Include(x => x.Category)
                            .Include(x => x.Posts.Select(v => v.Votes))
                            .Where(x => topicIds.Contains(x.Id))
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.LastPost.DateCreated)
                            .Take(amountToTake)
                            .ToList();
        }

        public IList<Topic> GetRssTopicsByCategory(int amountToTake, Guid categoryId)
        {
            return _context.Topic
                            .Include(x => x.LastPost)
                            .Include(x => x.LastPost.User)
                            .Include(x => x.Category)
                            .Include(x => x.Posts.Select(v => v.Votes))
                            .Where(x => x.Category.Id == categoryId)
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.LastPost.DateCreated)
                            .Take(amountToTake)
                            .ToList();
        }

        public PagedList<Topic> GetPagedTopicsByTag(int pageIndex, int pageSize, int amountToTake, string tag)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.Count(e => e.Tags.Any(t => t.Slug == tag));
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the topics using an efficient
            var results = _context.Topic
                                //.Include(x => x.Posts)
                                .Include(x => x.LastPost)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.Category)
                                .Include(x => x.Posts.Select(v => v.Votes))
                                .Include(x => x.Tags)
                                .Where(x => x.Pending != true)
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
                .Include(x => x.Poll)
                .Include(x => x.Poll.PollAnswers)
                .Include(x => x.User)
                .FirstOrDefault(x => x.Slug == slug);
        }

        public IList<Topic> GetTopicBySlugLike(string slug)
        {
            return _context.Topic
                            //.Include(x => x.Posts)
                            .Include(x => x.LastPost)
                            .Include(x => x.LastPost.User)
                            .Include(x => x.Category)
                            .Where(x => x.Slug.Contains(slug))
                            .ToList();
        }

        public int TopicCount()
        {
            return _context.Topic.Count();
        }

        /// <summary>
        /// Get all posts that are solutions, by user
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public IList<Topic> GetSolvedTopicsByMember(Guid memberId)
        {
            return _context.Topic
                            //.Include(x => x.Posts)
                            .Include(x => x.LastPost)
                            .Include(x => x.LastPost.User)
                            .Include(x => x.Category)
                            .Include(x => x.Posts.Select(v => v.Votes))
                            .Where(x => x.User.Id == memberId)
                            .Where(x => x.Pending != true)
                            .Where(x => x.Posts.Select(p => p.IsSolution).Contains(true))
                            .ToList();
        }
    }
}