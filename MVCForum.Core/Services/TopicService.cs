namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Constants;
    using Events;
    using Interfaces;
    using Interfaces.Pipeline;
    using Interfaces.Services;
    using Models.Entities;
    using Models.Enums;
    using Models.General;
    using Pipeline;
    using Reflection;
    using Utilities;

    public partial class TopicService : ITopicService
    {
        private readonly INotificationService _notificationService;
        private IMvcForumContext _context;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ISettingsService _settingsService;
        private readonly IPostService _postService;
        private readonly IFavouriteService _favouriteService;
        private readonly IRoleService _roleService;
        private readonly IPollService _pollService;
        private readonly ICacheService _cacheService;
        private readonly ILoggingService _loggingService;

        public TopicService(IMvcForumContext context, IMembershipUserPointsService membershipUserPointsService,
            ISettingsService settingsService, INotificationService notificationService,
            IFavouriteService favouriteService,
            IPostService postService, IRoleService roleService, IPollService pollService, ICacheService cacheService, ILoggingService loggingService)
        {
            _membershipUserPointsService = membershipUserPointsService;
            _settingsService = settingsService;
            _notificationService = notificationService;
            _favouriteService = favouriteService;
            _postService = postService;
            _roleService = roleService;
            _pollService = pollService;
            _cacheService = cacheService;
            _loggingService = loggingService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
            _membershipUserPointsService.RefreshContext(context);
            _settingsService.RefreshContext(context);
            _notificationService.RefreshContext(context);
            _favouriteService.RefreshContext(context);
            _postService.RefreshContext(context);
            _roleService.RefreshContext(context);
            _pollService.RefreshContext(context);
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
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
                                .AsNoTracking()
                                .Where(x => allowedCatIds.Contains(x.Category.Id) && x.Pending != true)
                                .ToList();
        }

        public IList<SelectListItem> GetAllSelectList(List<Category> allowedCategories, int amount)
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
        /// <param name="files"></param>
        /// <param name="tags"></param>
        /// <param name="subscribe"></param>
        /// <param name="postContent"></param>
        /// <param name="post">Optional Post: Used for moving a existing post into a new topic</param>
        /// <returns></returns>
        public async Task<IPipelineProcess<Topic>> Create(Topic topic, HttpPostedFileBase[] files, string tags, bool subscribe, string postContent, Post post)
        {
            // url slug generator
            topic.Slug = ServiceHelpers.GenerateSlug(topic.Name, 
                                    GetTopicBySlugLike(ServiceHelpers.CreateUrl(topic.Name))
                                    .Select(x => x.Slug).ToList(), null);

            // Get the pipelines
            var topicCreatePipes = ForumConfiguration.Instance.PipelinesTopicCreate;

            // The model to process
            var piplineModel = new PipelineProcess<Topic>(topic);

            // See if we have any files
            if (files != null && files.Any(x => x != null))
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.PostedFiles, files);
            }

            // See if we have any tags
            if (!string.IsNullOrWhiteSpace(tags))
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Tags, tags);
            }

            // See if we have a post
            if (post != null)
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Post, post);
            }

            // Add the extended data we need
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Subscribe, subscribe);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.IsEdit, false);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Content, postContent);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Topic>, Topic>(_context);

            // Register the pipes 
            var allTopicPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Topic>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in topicCreatePipes)
            {
                if (allTopicPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allTopicPipes[pipe]);
                }
            }

            // Process the pipeline
            return await pipeline.Process(piplineModel);
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Edit(Topic topic, HttpPostedFileBase[] files, string tags, bool subscribe, 
            string postContent, string topicName, List<PollAnswer> pollAnswers, int closePollAfterDays)
        {
            // url slug generator
            topic.Slug = ServiceHelpers.GenerateSlug(topic.Name,
                GetTopicBySlugLike(ServiceHelpers.CreateUrl(topic.Name))
                    .Select(x => x.Slug).ToList(), null);

            // Get the pipelines
            var topicPipes = ForumConfiguration.Instance.PipelinesTopicUpdate;

            // The model to process
            var piplineModel = new PipelineProcess<Topic>(topic);

            // Add the extended data we need
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Subscribe, subscribe);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.PostedFiles, files);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Tags, tags);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.IsEdit, true);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Content, postContent);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.PollNewAnswers, pollAnswers);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.PollCloseAfterDays, closePollAfterDays);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Name, topicName);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Topic>, Topic>(_context);

            // Register the pipes 
            var allTopicPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Topic>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in topicPipes)
            {
                if (allTopicPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allTopicPipes[pipe]);
                }
            }

            // Process the pipeline
            return await pipeline.Process(piplineModel);
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
        public async Task<PaginatedList<Topic>> GetRecentTopics(int pageIndex, int pageSize, int amountToTake, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // Get the topics using an efficient
            var query = _context.Topic
                .Include(x => x.Category)
                .Include(x => x.LastPost.User)
                .Include(x => x.User)
                .Include(x => x.Poll)
                .Include(x => x.Tags)
                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                .OrderByDescending(x => x.LastPost.DateCreated);

            // Return a paged list
            return await PaginatedList<Topic>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
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
                                .Include(x => x.Tags)
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
        public async Task<PaginatedList<Topic>> GetPagedTopicsByCategory(int pageIndex, int pageSize, int amountToTake, Guid categoryId)
        {
            // Get the topics using an efficient
            var query = _context.Topic
                        .Include(x => x.Category)
                        .Include(x => x.LastPost.User)
                        .Include(x => x.User)
                        .Include(x => x.Poll)
                        .Include(x => x.Tags)
                        .Where(x => x.Category.Id == categoryId)
                        .Where(x => x.Pending != true)
                        .OrderByDescending(x => x.IsSticky)
                        .ThenByDescending(x => x.LastPost.DateCreated);

            // Return a paged list
            return await PaginatedList<Topic>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets all the pending topics in a paged list
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public async Task<PaginatedList<Topic>> GetPagedPendingTopics(int pageIndex, int pageSize, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // Get the topics using an efficient
            var query = _context.Topic
                            .Include(x => x.Category)
                            .Include(x => x.LastPost.User)
                            .Include(x => x.User)
                            .Include(x => x.Poll)
                            .Include(x => x.Tags)
                            .AsNoTracking()
                            .Where(x => x.Pending == true && allowedCatIds.Contains(x.Category.Id))
                            .OrderBy(x => x.LastPost.DateCreated);

            // Return a paged list
            return await PaginatedList<Topic>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        public IList<Topic> GetPendingTopics(List<Category> allowedCategories, MembershipRole usersRole)
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
                            if (permissions[ForumConfiguration.Instance.PermissionEditPosts].IsTicked)
                            {
                                pendingTopics.Add(pendingTopic);
                            }
                        }
                    }
                    return pendingTopics;
                }
                return allPendingTopics;
           
        }

        public int GetPendingTopicsCount(List<Category> allowedCategories)
        {

                var allowedCatIds = allowedCategories.Select(x => x.Id);
                return _context.Topic.AsNoTracking().Include(x => x.Category).Count(x => x.Pending == true && allowedCatIds.Contains(x.Category.Id));
        

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
        public async Task<PaginatedList<Topic>> GetPagedTopicsByTag(int pageIndex, int pageSize, int amountToTake, string tag, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // Get the topics using an efficient
            var query = _context.Topic
                .Include(x => x.Category)
                .Include(x => x.LastPost.User)
                .Include(x => x.User)
                .Include(x => x.Poll)
                .Include(x => x.Tags)
                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                .OrderByDescending(x => x.IsSticky)
                .ThenByDescending(x => x.LastPost.DateCreated)
                .Where(e => e.Tags.Any(t => t.Slug.Equals(tag)));
                           

            // Return a paged list
            return await PaginatedList<Topic>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
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
                            .Include(x => x.Tags)
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

        public async Task<PaginatedList<Topic>> GetTopicsByCsv(int pageIndex, int pageSize, int amountToTake, List<Guid> topicIds, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // Now get the paged stuff
            var query = _context.Topic
                .Include(x => x.Category)
                .Include(x => x.LastPost.User)
                .Include(x => x.User)
                .Include(x => x.Poll)
                .Include(x => x.Tags)
                .Where(x => topicIds.Contains(x.Id))
                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                .OrderByDescending(x => x.LastPost.DateCreated);

            // Return a paged list
            return await PaginatedList<Topic>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        public async Task<PaginatedList<Topic>> GetMembersActivity(int pageIndex, int pageSize, int amountToTake, Guid memberGuid, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            // Get the Posts and then get the topics from the post
            // This is an interim solution, as its flawed due to multiple posts in one topic so the paging might
            // be incorrect if all posts are from one topic.
            var query = _context.Topic
                .Include(x => x.Category)
                .Include(x => x.LastPost.User)
                .Include(x => x.Poll)
                .Include(x => x.User)
                .Include(x => x.Posts)
                .Include(x => x.Tags)
                .Where(x => x.Posts.Any(u => u.User.Id == memberGuid && u.Pending != true) && allowedCatIds.Contains(x.Category.Id))
                .OrderByDescending(x => x.LastPost.DateEdited);

            // Return a paged list
            return await PaginatedList<Topic>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
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
                                .Include(x => x.Tags)
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
                    .Include(x => x.Tags)
                    .FirstOrDefault(x => x.Slug == slug);
        }

        /// <summary>
        /// Return a topic by Id
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public Topic Get(Guid topicId)
        {

                var topic = _context.Topic
                                    .Include(x => x.Category)
                                    .Include(x => x.LastPost.User)
                                    .Include(x => x.User)
                                    .Include(x => x.Poll)
                                    .Include(x => x.Tags)
                                .FirstOrDefault(x => x.Id == topicId);

                return topic;
         
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
                .Include(x => x.Tags)
                .Where(x => topicIds.Contains(x.Id) && allowedCatIds.Contains(x.Category.Id))
                .OrderByDescending(x => x.LastPost.DateCreated)
                .ToList();
        }

        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        public async Task<IPipelineProcess<Topic>> Delete(Topic topic)
        {
            // Get the pipelines
            var topicPipes = ForumConfiguration.Instance.PipelinesTopicDelete;

            // The model to process
            var piplineModel = new PipelineProcess<Topic>(topic);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Topic>, Topic>(_context);

            // Register the pipes 
            var allTopicPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Topic>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in topicPipes)
            {
                if (allTopicPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allTopicPipes[pipe]);
                }
            }

            return await pipeline.Process(piplineModel);
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
                                .Include(x => x.Tags)
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
        public async Task<bool> SolveTopic(Topic topic, Post post, MembershipUser marker, MembershipUser solutionWriter)
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

                if (topic.User.Id == marker.Id || marker.Roles.Any(x => x.RoleName == Constants.AdminRoleName))
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
                        var result = await _membershipUserPointsService.Add(new MembershipUserPoints
                        {
                            Points = _settingsService.GetSettings().PointsAddedForSolution,
                            User = solutionWriter,
                            PointsFor = PointsFor.Solution,
                            PointsForId = post.Id
                        });
                        if (!result.Successful)
                        {
                            // Just log don't throw
                            _loggingService.Error(result.ProcessLog.FirstOrDefault());
                        }
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

        public async Task<PaginatedList<Topic>> GetPagedTopicsAll(int pageIndex, int pageSize, int amountToTake, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);


            // Get the topics using an efficient
            var query = _context.Topic
                .Include(x => x.Category)
                .Include(x => x.LastPost.User)
                .Include(x => x.User)
                .Include(x => x.Poll)
                .Where(x => x.Pending != true && allowedCatIds.Contains(x.Category.Id))
                .OrderByDescending(x => x.IsSticky)
                .ThenByDescending(x => x.LastPost.DateCreated);

            // Return a paged list
            return await PaginatedList<Topic>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
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

        //public IEnumerable<Record> PerformBatchJoinWithIds(IEnumerable<int> ids)
        //{
        //    var context = GetContext<MyDatabaseContext>();
        //    // Disable auto detection of changes; much faster for batch edits/inserts
        //    context.Configuration.AutoDetectChangesEnabled = false;
        //    // A GUID will keep track of this batch operation
        //    var uniqueId = Guid.NewGuid();
        //    // Insert the batchquery objects for each id
        //    foreach (var id in ids)
        //    {
        //        context.BatchQueries.Add(new BatchQuery { Id = uniqueId, IdToQuery = id });
        //    }
        //    // Detect all changes in one shot and then save them
        //    context.ChangeTracker.DetectChanges();
        //    context.SaveChanges();
        //    // Now we can re-enable auto detection of changes (in case we use this context elsewhere)
        //    context.Configuration.AutoDetectChangesEnabled = true;
        //    // Join the batch queries table with the records we're trying to get
        //    var entities = context.Records.Join(context.BatchQueries, x => x.Id, y => y.IdToQuery, (x, y) => x)
        //        .ToList();
        //    // Finally, we can delete all of the BatchQuery records matching the GUID
        //    context.Database.ExecuteSqlCommand("DELETE FROM BatchQueries WHERE ID = {0}", uniqueId);
        //    return entities;
        //}

    }
}
