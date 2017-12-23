namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data.Entity;
    using System.Web.Mvc;
    using Domain.Constants;
    using Domain.DomainModel;
    using Domain.DomainModel.General;
    using Domain.Events;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;
    using Data.Context;
    using Utilities;

    public partial class TopicService : ITopicService
    {
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly MVCForumContext _context;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ISettingsService _settingsService;
        private readonly IPostService _postService;
        private readonly IFavouriteService _favouriteService;
        private readonly IRoleService _roleService;
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly ICacheService _cacheService;

        public TopicService(IMVCForumContext context, IMembershipUserPointsService membershipUserPointsService,
            ISettingsService settingsService, ITopicNotificationService topicNotificationService,
            IFavouriteService favouriteService,
            IPostService postService, IRoleService roleService, IPollService pollService, IPollAnswerService pollAnswerService, ICacheService cacheService)
        {
            _membershipUserPointsService = membershipUserPointsService;
            _settingsService = settingsService;
            _topicNotificationService = topicNotificationService;
            _favouriteService = favouriteService;
            _postService = postService;
            _roleService = roleService;
            _pollService = pollService;
            _pollAnswerService = pollAnswerService;
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        public Topic SanitizeTopic(Topic topic)
        {
            topic.Name = StringUtils.SafePlainText(topic.Name);
            return topic;
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
                                .Where(x => allowedCatIds.Contains(x.Category.Id) && x.Pending != true)
                                .ToList();
        }

        public IList<SelectListItem> GetAllSelectList(List<Category> allowedCategories, int amount)
        {
            var cacheKey = string.Concat(CacheKeys.Topic.StartsWith, "GetAllSelectList-", allowedCategories.GetHashCode(), "-", amount);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                // get the category ids
                var allowedCatIds = allowedCategories.Select(x => x.Id);
                return _context.Topic.AsNoTracking()
                                    .Include(x => x.Category)
                                    .Where(x => allowedCatIds.Contains(x.Category.Id) && x.Pending != true)
                                    .OrderByDescending(x => x.CreateDate)
                                    .Take(amount)
                                    .Select(x => new SelectListItem
                                    {
                                        Text = x.Name,
                                        Value = x.Id.ToString()
                                    }).ToList();
            });
        }

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

        public IList<Topic> GetPopularTopics(DateTime? from, DateTime? to, List<Category> allowedCategories, int amountToShow = 20)
        {
            if (from == null)
            {
                from = DateTime.UtcNow.AddDays(-14);
            }

            if (to == null)
            {
                to = DateTime.UtcNow;
            }

            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            var topics = _context.Topic
                .Include(x => x.Category)
                .Include(x => x.LastPost)
                .Include(x => x.Posts)
                .Include(x => x.User)
                .Where(x => allowedCatIds.Contains(x.Category.Id))
                .OrderByDescending(x => x.Posts.Count(c => c.DateCreated >= from && c.DateCreated <= to))
                .ThenByDescending(x => x.Posts.Select(v => v.VoteCount).Sum())
                .ThenByDescending(x => x.Views)
                .Take(amountToShow)
                .ToList();

            return topics;
        }

        /// <summary>
        /// Create a new topic and also the topic starter post
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Topic Add(Topic topic)
        {
            topic = SanitizeTopic(topic);

            topic.CreateDate = DateTime.UtcNow;

            // url slug generator
            topic.Slug = ServiceHelpers.GenerateSlug(topic.Name, GetTopicBySlugLike(ServiceHelpers.CreateUrl(topic.Name)), null);

            return _context.Topic.Add(topic);
        }

        /// <summary>
        /// Get todays topics
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Add a last post to a topic. Must be part of a separate database update
        /// in EF because of circular dependencies. So save the topic before calling this.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="postContent"></param>
        /// <returns></returns>
        public Post AddLastPost(Topic topic, string postContent)
        {

            topic = SanitizeTopic(topic);

            // Create the post
            var post = new Post
            {
                DateCreated = DateTime.UtcNow,
                IsTopicStarter = true,
                DateEdited = DateTime.UtcNow,
                PostContent = postContent,
                User = topic.User,
                Topic = topic
            };

            // Add the post
            _postService.Add(post);

            topic.LastPost = post;

            return post;
        }

        public List<MarkAsSolutionReminder> GetMarkAsSolutionReminderList(int days)
        {
            var datefrom = DateTime.UtcNow.AddDays(-days);
            return _context.Topic
                .Include(x => x.Category)
                .Include(x => x.User)
                .Include(x => x.Posts)
                .Where(x => x.CreateDate <= datefrom && !x.Solved && x.Posts.Count > 1 && x.SolvedReminderSent != true)
                .Select(x => new MarkAsSolutionReminder
                {
                    Topic = x,
                    PostCount = x.Posts.Count
                })
                .ToList();
        }

        /// <summary>
        /// Returns a paged list of topics, ordered by most recent
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
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
                                .AsNoTracking()
                                .Include(x => x.Category)
                                .Include(x => x.LastPost.User)
                                .Include(x => x.User)
                                .Include(x => x.Poll)
                                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                                .OrderByDescending(x => x.LastPost.DateCreated)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        /// <summary>
        /// Returns a specified amount of most recent topics in a list used for RSS feeds
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns all topics by a specified user
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a paged list of topics from a specified category
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Gets all the pending topics in a paged list
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
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

        public IList<Topic> GetPendingTopics(List<Category> allowedCategories, MembershipRole usersRole)
        {
            var cacheKey = string.Concat(CacheKeys.Topic.StartsWith, "GetPendingTopics-", allowedCategories.GetHashCode(), "-", usersRole.Id);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var allowedCatIds = allowedCategories.Select(x => x.Id);
                var allPendingTopics = _context.Topic.AsNoTracking().Include(x => x.Category).Where(x => x.Pending == true && allowedCatIds.Contains(x.Category.Id)).ToList();
                if (usersRole != null)
                {
                    var pendingTopics = new List<Topic>();
                    var permissionSets = new Dictionary<Guid, PermissionSet>();
                    foreach (var category in allowedCategories)
                    {
                        var permissionSet = _roleService.GetPermissions(category, usersRole);
                        permissionSets.Add(category.Id, permissionSet);
                    }

                    foreach (var pendingTopic in allPendingTopics)
                    {
                        if (permissionSets.ContainsKey(pendingTopic.Category.Id))
                        {
                            var permissions = permissionSets[pendingTopic.Category.Id];
                            if (permissions[SiteConstants.Instance.PermissionEditPosts].IsTicked)
                            {
                                pendingTopics.Add(pendingTopic);
                            }
                        }
                    }
                    return pendingTopics;
                }
                return allPendingTopics;
            });
        }

        public int GetPendingTopicsCount(List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Topic.StartsWith, "GetPendingTopicsCount-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var allowedCatIds = allowedCategories.Select(x => x.Id);
                return _context.Topic.AsNoTracking().Include(x => x.Category).Count(x => x.Pending == true && allowedCatIds.Contains(x.Category.Id));
            });

        }

        /// <summary>
        /// Returns a specified amount of most recent topics in a category used for RSS feeds
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns a paged amount of topics in a list filtered via tag
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="tag"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
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
                                .Where(e => e.Tags.Any(t => t.Slug.Equals(tag)))
                                .Take(pageSize)
                                .Skip((pageIndex - 1) * pageSize)
                                .ToList();

            // Return a paged list
            return new PagedList<Topic>(results, pageIndex, pageSize, total);
        }

        /// <summary>
        /// Returns a paged amount of searched topics by a string search value
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="searchTerm"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Topic> SearchTopics(int amountToTake, string searchTerm, List<Category> allowedCategories)
        {
            // Create search term
            var search = StringUtils.ReturnSearchString(searchTerm);

            // Now split the words
            var splitSearch = search.Split(' ').ToList();

            // Pass the sanitised split words to the repo
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // We might only want to display the top 100
            // but there might not be 100 topics

            var topics = _context.Topic
                            .Include(x => x.Posts)
                            .Include(x => x.Category)
                            .Include(x => x.LastPost.User)
                            .Include(x => x.User)
                            .AsNoTracking()
                            .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                            .Where(x => x.Posts.Any(p => p.Pending != true));

            // Loop through each word and see if it's in the post
            foreach (var term in splitSearch)
            {
                var sTerm = term.Trim().ToUpper();
                topics = topics.Where(x => x.Posts.Any(p => p.PostContent.ToUpper().Contains(sTerm)) || x.Name.ToUpper().Contains(sTerm));
            }

            //// Return a paged list
            return topics.Take(amountToTake).ToList();
        }

        public PagedList<Topic> GetTopicsByCsv(int pageIndex, int pageSize, int amountToTake, List<Guid> topicIds, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // Get the count
            var total = _context.Topic
                                .Join(topicIds,
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
                .Join(topicIds,
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

        public PagedList<Topic> GetMembersActivity(int pageIndex, int pageSize, int amountToTake, Guid memberGuid, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // We might only want to display the top 100
            // but there might not be 100 topics
            var total = _context.Post
                            .Include(x => x.Topic.Category)
                            .Include(x => x.Topic.LastPost.User)
                            .Include(x => x.Topic.Poll)
                            .Include(x => x.User)
                            .AsNoTracking().Where(x => x.User.Id == memberGuid && x.Pending != true && allowedCatIds.Contains(x.Topic.Category.Id)).DistinctBy(x => x.Topic.Id).Count();
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

        /// <summary>
        /// Return a topic by url slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public Topic GetTopicBySlug(string slug)
        {
            slug = StringUtils.GetSafeHtml(slug);
            return _context.Topic
                    .Include(x => x.Category)
                    .Include(x => x.LastPost.User)
                    .Include(x => x.User)
                    .Include(x => x.Poll)
                    .FirstOrDefault(x => x.Slug == slug);
        }

        /// <summary>
        /// Return a topic by Id
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public Topic Get(Guid topicId)
        {
            var cacheKey = string.Concat(CacheKeys.Topic.StartsWith, "Get-", topicId);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var topic = _context.Topic
                                    .Include(x => x.Category)
                                    .Include(x => x.LastPost.User)
                                    .Include(x => x.User)
                                    .Include(x => x.Poll)
                                .FirstOrDefault(x => x.Id == topicId);

                return topic;
            });
        }

        public List<Topic> Get(List<Guid> topicIds, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Topic
                .Include(x => x.Category)
                .Include(x => x.LastPost.User)
                .Include(x => x.User)
                .Include(x => x.Poll)
                .Where(x => topicIds.Contains(x.Id) && allowedCatIds.Contains(x.Category.Id))
                .OrderByDescending(x => x.LastPost.DateCreated)
                .ToList();
        }

        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="unitOfWork"></param>
        public void Delete(Topic topic, IUnitOfWork unitOfWork)
        {
            // First thing - Set the last post as null and clear tags
            topic.LastPost = null;
            topic.Tags.Clear();

            // Save here to clear the last post
            unitOfWork.SaveChanges();

            // TODO - Need to refactor as some of the code below is duplicated in the post delete

            // Loop through all the posts and clear the associated entities
            // then delete the posts
            if (topic.Posts != null)
            {
                var postsToDelete = new List<Post>();
                postsToDelete.AddRange(topic.Posts);
                foreach (var post in postsToDelete)
                {
                    // Posts should only be deleted from this method as it clears
                    // associated data
                    _postService.Delete(post, unitOfWork, true);
                }

                // Final clear
                topic.Posts.Clear();
            }

            unitOfWork.SaveChanges();

            // Remove all notifications on this topic too
            if (topic.TopicNotifications != null)
            {
                var notificationsToDelete = new List<TopicNotification>();
                notificationsToDelete.AddRange(topic.TopicNotifications);
                foreach (var topicNotification in notificationsToDelete)
                {
                    _topicNotificationService.Delete(topicNotification);
                }

                // Final Clear
                topic.TopicNotifications.Clear();
            }

            // Remove all favourites on this topic too
            if (topic.Favourites != null)
            {
                var toDelete = new List<Favourite>();
                toDelete.AddRange(topic.Favourites);
                foreach (var entity in toDelete)
                {
                    _favouriteService.Delete(entity);
                }

                // Final Clear
                topic.Favourites.Clear();
            }

            // Poll
            if (topic.Poll != null)
            {
                //Delete the poll answers
                var pollAnswers = topic.Poll.PollAnswers;
                if (pollAnswers.Any())
                {
                    var pollAnswersList = new List<PollAnswer>();
                    pollAnswersList.AddRange(pollAnswers);
                    foreach (var answer in pollAnswersList)
                    {
                        answer.Poll = null;
                        _pollAnswerService.Delete(answer);
                    }
                }

                topic.Poll.PollAnswers.Clear();
                topic.Poll.User = null;
                _pollService.Delete(topic.Poll);

                // Final Clear
                topic.Poll = null;
            }

            // Finally delete the topic
            _context.Topic.Remove(topic);
        }

        public int TopicCount(List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Topic.StartsWith, "TopicCount-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                // get the category ids
                var allowedCatIds = allowedCategories.Select(x => x.Id);
                return _context.Topic
                    .Include(x => x.Category)
                    .AsNoTracking()
                    .Count(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id));
            });

        }

        /// <summary>
        /// Return topics by a specified user that are marked as solved
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

        /// <summary>
        /// Mark a topic as solved
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="post"></param>
        /// <param name="marker"></param>
        /// <param name="solutionWriter"></param>
        /// <returns>True if topic has been marked as solved</returns>
        public bool SolveTopic(Topic topic, Post post, MembershipUser marker, MembershipUser solutionWriter)
        {
            var solved = false;

            var e = new MarkedAsSolutionEventArgs
            {
                Topic = topic,
                Post = post,
                Marker = marker,
                SolutionWriter = solutionWriter
            };
            EventManager.Instance.FireBeforeMarkedAsSolution(this, e);

            if (!e.Cancel)
            {
                // Make sure this user owns the topic or this is an admin, if not do nothing

                if (topic.User.Id == marker.Id || marker.Roles.Any(x => x.RoleName == AppConstants.AdminRoleName))
                {
                    // Update the post
                    post.IsSolution = true;
                    //_postRepository.Update(post);

                    // Update the topic
                    topic.Solved = true;
                    //SaveOrUpdate(topic);

                    // Assign points
                    // Do not give points to the user if they are marking their own post as the solution
                    if (marker.Id != solutionWriter.Id)
                    {
                        _membershipUserPointsService.Add(new MembershipUserPoints
                        {
                            Points = _settingsService.GetSettings().PointsAddedForSolution,
                            User = solutionWriter,
                            PointsFor = PointsFor.Solution,
                            PointsForId = post.Id
                        });
                    }

                    EventManager.Instance.FireAfterMarkedAsSolution(this, new MarkedAsSolutionEventArgs
                    {
                        Topic = topic,
                        Post = post,
                        Marker = marker,
                        SolutionWriter = solutionWriter
                    });
                    solved = true;
                }
            }

            return solved;
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

        public bool PassedTopicFloodTest(string topicTitle, MembershipUser user)
        {
            topicTitle = StringUtils.SafePlainText(topicTitle);

            var timeNow = DateTime.UtcNow;
            var floodWindow = timeNow.AddMinutes(-2);

            // Firstly check to see if they have posted the same topic title already in the past 2 minutes
            var matchingTopicTitles = _context.Topic
                .Include(x => x.User)
                .AsNoTracking()
                .Count(x => x.User.Id == user.Id && x.Name.Equals(topicTitle) && x.CreateDate >= floodWindow);

            return matchingTopicTitles <= 0;
        }
    }
}
