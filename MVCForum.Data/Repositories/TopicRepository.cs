using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Utilities;

namespace MVCForum.Data.Repositories
{
    public class TopicRepository : ITopicRepository
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
                            .OrderByDescending(x => x.Views)
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
                                .Where(x => x.Category.Id == categoryId)
                                .ToList();
            return results;
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
                                .Where(x => x.Category.Id == categoryId)
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
                            .Where(x => x.PostContent.Contains(searchTerm) | x.Topic.Name.Contains(searchTerm))
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
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = csv.Count;
            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the Posts and then get the topics from the post
            // This is an interim solution, as its flawed due to multiple posts in one topic so the paging might
            // be incorrect if all posts are from one topic.
            var results = _context.Topic
                            //.Include(x => x.Posts)
                            .Include(x => x.LastPost)
                            .Include(x => x.LastPost.User)
                            .Include(x => x.Category)
                            .Where(x => csv.Contains(x.Id))
                            .OrderByDescending(x => x.LastPost.DateCreated)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        public IList<Topic> GetRssTopicsByCategory(int amountToTake, Guid categoryId)
        {
            return _context.Topic
                            //.Include(x => x.Posts)
                            .Include(x => x.LastPost)
                            .Include(x => x.LastPost.User)
                            .Include(x => x.Category)    
                            .Where(x => x.Category.Id == categoryId)
                            .OrderByDescending(x => x.LastPost.DateCreated)
                            .Take(amountToTake)
                            .ToList();
        }

        public PagedList<Topic> GetPagedTopicsByTag(int pageIndex, int pageSize, int amountToTake, string tag)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.Count(e => e.Tags.Select(t => t.Tag).Contains(tag));
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
                                .Include(x => x.Tags.Select(t => t.Tag))
                                .OrderByDescending(x => x.IsSticky)
                                .ThenByDescending(x => x.LastPost.DateCreated)
                                .Where(e => e.Tags.Select(t => t.Tag).Contains(tag))
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
                .SingleOrDefault(x => x.Slug == slug);
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
                            .Where(x => x.User.Id == memberId)
                            .Where(x => x.Posts.Select(p => p.IsSolution).Contains(true))
                            .ToList();
        }
    }
}