using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
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

        #region Populate Collections

        //private void PopulatePostsVotes(List<Topic> results)
        //{
        //    // Get Topics Ids
        //    var topicIds = results.Select(x => x.Id);

        //    // Get all posts for these topics in one hit
        //    var posts = _context.Post.Include(x => x.Topic).AsNoTracking().Where(x => topicIds.Contains(x.Topic.Id)).ToList();

        //    // Get the ids of the posts
        //    var postIds = posts.Select(x => x.Id);

        //    // Use the ids to get all the post votes
        //    var votes = _context.Vote.Include(x => x.Post).AsNoTracking().Where(x => postIds.Contains(x.Post.Id)).ToList();

        //    foreach (var topic in results)
        //    {
        //        var topicPosts = posts.Where(x => x.Topic.Id == topic.Id).ToList();
        //        topic.Posts = topicPosts;
        //        if (topic.Posts.Any())
        //        {
        //            PopulateVotes(topic.Posts, voteGroups);
        //        }
        //    }
        //}

        //private void PopulateVotes(IList<Post> posts, List<IGrouping<Guid, Vote>> votes)
        //{
        //    foreach (var post in posts)
        //    {
        //        var voteGroup = votes.FirstOrDefault(x => x.Key == post.Id);
        //        post.Votes = voteGroup == null ? new List<Vote>() : voteGroup.ToList();
        //    }
        //}

        #endregion

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
            return _context.Topic.Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll).ToList();
        }

        /// <summary>
        /// Get the highest viewed topics
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Topic> GetHighestViewedTopics(int amountToTake)
        {
            return _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.Views)
                            .Take(amountToTake)
                            .ToList();
        }

        public IList<Topic> GetPopularTopics(DateTime from, DateTime to, int amountToShow)
        {
            var topics = _context.Post
                .Include(x => x.Topic)
                .Include(x => x.User)
                .AsNoTracking()
                .Where(x => x.IsTopicStarter)
                .Where(x => x.DateCreated >= from)
                .Where(x => x.DateCreated <= to)
                .OrderBy(x => x.VoteCount)
                .Take(amountToShow)
                .Select(x => x.Topic);

            if (!topics.Any())
            {
                topics = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                            .AsNoTracking()
                            .Where(x => x.CreateDate >= from)
                            .Where(x => x.CreateDate <= to)
                            .OrderBy(x => x.Views)
                            .Take(amountToShow);
            }
                     
            return topics.ToList();
        }

        public IList<Topic> GetTodaysTopics(int amountToTake)
        {
            return _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
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
            var topic = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                            .FirstOrDefault(x => x.Id == id);

            return topic;
        }

        public List<Topic> Get(List<Guid> ids)
        {
            return _context.Topic
                .Include(x => x.Category)
                .Include(x => x.LastPost.User)
                .Include(x => x.User)
                .Include(x => x.Poll)
                .Where(x => ids.Contains(x.Id)).OrderByDescending(x => x.LastPost.DateCreated).ToList();
        }

        public void Delete(Topic item)
        {
            _context.Topic.Remove(item);
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
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
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
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
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
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                                .Where(x => x.User.Id == memberId)
                                .Where(x => x.Pending != true)
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

        public PagedList<Topic> GetPagedPendingTopics(int pageIndex, int pageSize)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Topic.Count(x => x.Pending == true);

            // Get the topics using an efficient
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
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
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
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
                            .Where(x => x.PostContent.Contains(searchTerm) | x.Topic.Name.Contains(searchTerm))
                            .Where(x => x.Pending != true)
                            .OrderByDescending(x => x.DateCreated)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Return a paged list
            return new PagedList<Topic>(results.Select(x => x.Topic), pageIndex, pageSize, total);
        }

        public PagedList<Topic> GetMembersActivity(int pageIndex, int pageSize, int amountToTake, Guid memberGuid)
        {
            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Post.Where(x => x.User.Id == memberGuid && x.Pending != true).DistinctBy(x => x.Topic.Id).Count();
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
                            .Where(x => x.User.Id == memberGuid && x.Pending != true)
                            .DistinctBy(x => x.Topic.Id)
                            .OrderByDescending(x => x.DateCreated)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();

            // Return a paged list
            return new PagedList<Topic>(results.Select(x => x.Topic), pageIndex, pageSize, total);
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
            var topics = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                            .Where(x => topicIds.Contains(x.Id))
                            .Where(x => x.Pending != true)
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
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
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
            var results = _context.Topic
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .AsNoTracking()
                            .Where(x => x.User.Id == memberId)
                            .Where(x => x.Pending != true)
                            .ToList();

            return results.Where(x => x.Posts.Select(p => p.IsSolution).Contains(true)).ToList();
        }
    }
}