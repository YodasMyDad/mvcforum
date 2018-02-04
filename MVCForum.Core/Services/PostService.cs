namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;
    using Constants;
    using Interfaces;
    using Interfaces.Pipeline;
    using Interfaces.Services;
    using LinqKit;
    using Models.Entities;
    using Models.Enums;
    using Models.General;
    using Pipeline;
    using Reflection;
    using Utilities;

    public partial class PostService : IPostService
    {
        private readonly IRoleService _roleService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ISettingsService _settingsService;
        private readonly IVoteService _voteService;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly IFavouriteService _favouriteService;
        private readonly IConfigService _configService;
        private IMvcForumContext _context;
        private readonly IPostEditService _postEditService;

        public PostService(IMvcForumContext context, IMembershipUserPointsService membershipUserPointsService,
            ISettingsService settingsService, IRoleService roleService,
            IVoteService voteService, 
            IUploadedFileService uploadedFileService, IFavouriteService favouriteService, 
            IConfigService configService, IPostEditService postEditService)
        {
            _roleService = roleService;
            _membershipUserPointsService = membershipUserPointsService;
            _settingsService = settingsService;
            _voteService = voteService;
            _uploadedFileService = uploadedFileService;
            _favouriteService = favouriteService;
            _configService = configService;
            _postEditService = postEditService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
            _roleService.RefreshContext(context);
            _membershipUserPointsService.RefreshContext(context);
            _settingsService.RefreshContext(context);
            _voteService.RefreshContext(context);
            _uploadedFileService.RefreshContext(context);
            _favouriteService.RefreshContext(context);
            _postEditService.RefreshContext(context);
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        #region Private / Helpers Methods

        public Post SanitizePost(Post post)
        {
            post.PostContent = StringUtils.GetSafeHtml(post.PostContent);

            // Check settings
            if (_settingsService.GetSettings().EnableEmoticons == true)
            {
                post.PostContent = _configService.Emotify(post.PostContent);
            }

            return post;
        }

        #endregion


        public Post GetTopicStarterPost(Guid topicId)
        {
            return _context.Post
                            .Include(x => x.Topic.Category)
                            .Include(x => x.User)
                            .FirstOrDefault(x => x.Topic.Id == topicId && x.IsTopicStarter);
        }

        /// <summary>
        /// Return all posts
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Post> GetAll(List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Post
                    .Include(x => x.Topic.Category)
                    .Where(x => allowedCatIds.Contains(x.Topic.Category.Id));

        }

        /// <summary>
        /// Returns a list of posts ordered by the lowest vote
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetLowestVotedPost(int amountToTake)
        {
            return _context.Post
                .Include(x => x.Topic)
                .Include(x => x.User)
                .Where(x => x.VoteCount < 0 && x.Pending != true)
                .OrderBy(x => x.VoteCount)
                .Take(amountToTake)
                .ToList();

        }

        /// <summary>
        /// Returns a list of posts ordered by the highest vote
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetHighestVotedPost(int amountToTake)
        {

            return _context.Post
                .Include(x => x.Topic)
                .Include(x => x.User)
                .Where(x => x.VoteCount > 0 && x.Pending != true)
                .OrderByDescending(x => x.VoteCount)
                .Take(amountToTake)
                .ToList();

        }

        /// <summary>
        /// Return all posts by a specified member
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="amountToTake"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Post> GetByMember(Guid memberId, int amountToTake, List<Category> allowedCategories)
        {

            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Post
                    .Include(x => x.Topic.LastPost.User)
                    .Include(x => x.Topic.Category)
                    .Include(x => x.User)
                    .Where(x => x.User.Id == memberId && x.Pending != true)
                    .Where(x => allowedCatIds.Contains(x.Topic.Category.Id))
                    .OrderByDescending(x => x.DateCreated)
                    .Take(amountToTake)
                    .ToList();

        }

        public IList<Post> GetReplyToPosts(Post post)
        {
            return GetReplyToPosts(post.Id);
        }

        public IList<Post> GetReplyToPosts(Guid postId)
        {

            // We don't allow topic starters in the list OR solutions. As if it's marked as a solution, it's a solution for that topic
            // and moving it wouldn't make sense.
            return _context.Post.Where(x => x.InReplyTo != null & x.InReplyTo == postId && !x.IsTopicStarter && !x.IsSolution).ToList();

        }

        public IEnumerable<Post> GetPostsByFavouriteCount(Guid postsByMemberId, int minAmountOfFavourites)
        {

            return _context.Post
                   .Include(x => x.Topic.LastPost.User)
                   .Include(x => x.Topic.Category)
                   .Include(x => x.User)
                   .Include(x => x.Favourites.Select(f => f.Member))
                   .Where(x => x.User.Id == postsByMemberId && x.Favourites.Count(c => c.Member.Id != postsByMemberId) >= minAmountOfFavourites);

        }

        public IEnumerable<Post> GetPostsFavouritedByOtherMembers(Guid postsByMemberId)
        {

            return _context.Post
                        .Include(x => x.Topic.LastPost.User)
                        .Include(x => x.Topic.Category)
                        .Include(x => x.User)
                        .Include(x => x.Favourites.Select(f => f.Member))
                        .Where(x => x.User.Id == postsByMemberId && x.Favourites.Any(c => c.Member.Id != postsByMemberId));


        }

        /// <summary>
        /// Returns a paged list of posts by a search term
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="searchTerm"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public async Task<PaginatedList<Post>> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm, List<Category> allowedCategories)
        {
            // Create search term
            var search = StringUtils.ReturnSearchString(searchTerm);

            // Now split the words
            var splitSearch = search.Trim().Split(' ').ToList();

            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            var query = _context.Post
                            .Include(x => x.Topic.Category)
                            .Include(x => x.User)
                            .Where(x => x.Pending != true)
                            .Where(x => allowedCatIds.Contains(x.Topic.Category.Id));

            // Start the predicate builder
            var postFilter = PredicateBuilder.New<Post>(false);

            // Loop through each word and see if it's in the post
            foreach (var term in splitSearch)
            {
                var sTerm = term.Trim();
                //query = query.Where(x => x.PostContent.ToUpper().Contains(sTerm) || x.SearchField.ToUpper().Contains(sTerm));
                postFilter = postFilter.Or(x => x.PostContent.ToUpper().Contains(sTerm) || x.IsTopicStarter && x.Topic.Name.ToUpper().Contains(sTerm));
            }

            // Add the predicate builder to the query
            query = query.Where(postFilter);


            // Get the Posts and then get the topics from the post
            // This is an interim solution, as its flawed due to multiple posts in one topic so the paging might
            // be incorrect if all posts are from one topic.
            var results = query
                .OrderByDescending(x => x.DateCreated);

            return await PaginatedList<Post>.CreateAsync(results.AsNoTracking(), pageIndex, pageSize);
        }

        /// <summary>
        /// Returns a paged list of posts by a topic id
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="topicId"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<PaginatedList<Post>> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order)
        {
            // Get the topics using an efficient
            var results = _context.Post
                                  .Include(x => x.User)
                                  .Include(x => x.Topic)
                                  .Where(x => x.Topic.Id == topicId && !x.IsTopicStarter && x.Pending != true);

            // Sort what order the posts are sorted in
            switch (order)
            {
                case PostOrderBy.Newest:
                    results = results.OrderByDescending(x => x.DateCreated);
                    break;

                case PostOrderBy.Votes:
                    results = results.OrderByDescending(x => x.VoteCount).ThenBy(x => x.DateCreated);
                    break;

                default:
                    results = results.OrderBy(x => x.DateCreated);
                    break;
            }

            return await PaginatedList<Post>.CreateAsync(results.AsNoTracking(), pageIndex, pageSize);
        }

        public async Task<PaginatedList<Post>> GetPagedPendingPosts(int pageIndex, int pageSize, List<Category> allowedCategories)
        {
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            var query = _context.Post
                .Include(x => x.Topic.Category)
                .Include(x => x.User)
                .Where(x => x.Pending == true && allowedCatIds.Contains(x.Topic.Category.Id))
                .OrderBy(x => x.DateCreated);
            return await PaginatedList<Post>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        public IList<Post> GetPendingPosts(List<Category> allowedCategories, MembershipRole usersRole)
        {
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            var allPendingPosts = _context.Post.AsNoTracking().Include(x => x.Topic.Category).Where(x => x.Pending == true && allowedCatIds.Contains(x.Topic.Category.Id)).ToList();
            if (usersRole != null)
            {
                var pendingPosts = new List<Post>();
                var permissionSets = new Dictionary<Guid, PermissionSet>();
                foreach (var category in allowedCategories)
                {
                    var permissionSet = _roleService.GetPermissions(category, usersRole);
                    permissionSets.Add(category.Id, permissionSet);
                }

                foreach (var pendingPost in allPendingPosts)
                {
                    if (permissionSets.ContainsKey(pendingPost.Topic.Category.Id))
                    {
                        var permissions = permissionSets[pendingPost.Topic.Category.Id];
                        if (permissions[ForumConfiguration.Instance.PermissionEditPosts].IsTicked)
                        {
                            pendingPosts.Add(pendingPost);
                        }
                    }
                }
                return pendingPosts;
            }
            return allPendingPosts;
        }

        public int GetPendingPostsCount(List<Category> allowedCategories)
        {
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Post.AsNoTracking().Include(x => x.Topic.Category).Count(x => x.Pending == true && allowedCatIds.Contains(x.Topic.Category.Id));

        }

        /// <summary>
        /// Return all posts by a specified member that are marked as solution
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Post> GetSolutionsByMember(Guid memberId, List<Category> allowedCategories)
        {

            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Post
                .Include(x => x.Topic.Category)
                .Include(x => x.Topic.LastPost.User)
                .Include(x => x.User)
                .Where(x => x.User.Id == memberId)
                .Where(x => x.IsSolution && x.Pending != true)
                .Where(x => allowedCatIds.Contains(x.Topic.Category.Id))
                .OrderByDescending(x => x.DateCreated)
                .ToList();

        }

        /// <summary>
        /// Returns a count of all posts
        /// </summary>
        /// <returns></returns>
        public int PostCount(List<Category> allowedCategories)
        {

            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Post
                .Include(x => x.Topic)
                .AsNoTracking()
                .Count(x => x.Pending != true && x.Topic.Pending != true && allowedCatIds.Contains(x.Topic.Category.Id));

        }

        /// <summary>
        /// Create a new post
        /// </summary>
        /// <param name="postContent"></param>
        /// <param name="topic"></param>
        /// <param name="user"></param>
        /// <param name="files"></param>
        /// <param name="isTopicStarter"></param>
        /// <param name="replyTo"></param>
        /// <returns></returns>
        public async Task<IPipelineProcess<Post>> Create(string postContent, Topic topic, MembershipUser user, HttpPostedFileBase[] files, bool isTopicStarter, Guid? replyTo)
        {
            var post = Initialise(postContent, topic, user);
            return await Create(post, files, isTopicStarter, replyTo);
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        /// <param name="post"></param>
        /// <param name="files"></param>
        /// <param name="isTopicStarter"></param>
        /// <param name="replyTo"></param>
        /// <returns></returns>
        public async Task<IPipelineProcess<Post>> Create(Post post, HttpPostedFileBase[] files, bool isTopicStarter, Guid? replyTo)
        {
            // Get the pipelines
            var postCreatePipes = ForumConfiguration.Instance.PipelinesPostCreate;

            // Set the post to topic starter
            post.IsTopicStarter = isTopicStarter;

            // If this is a reply to someone
            if (replyTo != null)
            {
                post.InReplyTo = replyTo;
            }

            // The model to process
            var piplineModel = new PipelineProcess<Post>(post);

            // Add the files for the post
            if(files != null)
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.PostedFiles, files);
            }

            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Post>, Post>(_context);

            // Register the pipes 
            var allPostPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Post>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in postCreatePipes)
            {
                if (allPostPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allPostPipes[pipe]);
                }
            }

            // Process the pipeline
            return await pipeline.Process(piplineModel);
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Edit(Post post, HttpPostedFileBase[] files, bool isTopicStarter, string postedTopicName, string postedContent)
        {
            // Get the pipelines
            var postCreatePipes = ForumConfiguration.Instance.PipelinesPostUpdate;

            // Set the post to topic starter
            post.IsTopicStarter = isTopicStarter;

            // The model to process
            var piplineModel = new PipelineProcess<Post>(post);

            // Add the files for the post
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.PostedFiles, files);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Name, postedTopicName);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Content, postedContent);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Post>, Post>(_context);

            // Register the pipes 
            var allPostPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Post>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in postCreatePipes)
            {
                if (allPostPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allPostPipes[pipe]);
                }
            }

            // Process the pipeline
            return await pipeline.Process(piplineModel);
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Move(Post post, Guid? newTopicId, string newTopicTitle, bool moveReplyToPosts)
        {
            // Get the pipelines
            var postPipes = ForumConfiguration.Instance.PipelinesPostMove;

            // The model to process
            var piplineModel = new PipelineProcess<Post>(post);

            // Add the files for the post
            if (newTopicId != null)
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.TopicId, newTopicId);
            }
            if (!string.IsNullOrWhiteSpace(newTopicTitle))
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Name, newTopicTitle);
            }
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.MovePosts, moveReplyToPosts);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Post>, Post>(_context);

            // Register the pipes 
            var allPostPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Post>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in postPipes)
            {
                if (allPostPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allPostPipes[pipe]);
                }
            }

            // Process the pipeline
            return await pipeline.Process(piplineModel);
        }

        /// <inheritdoc />
        public Post Initialise(string postContent, Topic topic, MembershipUser user)
        {
            // Has permission so create the post
            var newPost = new Post
            {
                PostContent = postContent,
                User = user,
                Topic = topic,
                IpAddress = StringUtils.GetUsersIpAddress(),
                DateCreated = DateTime.UtcNow,
                DateEdited = DateTime.UtcNow,
                Pending = topic.Category.ModeratePosts == true
            };

            return SanitizePost(newPost);
        }

        /// <summary>
        /// Return a post by Id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public Post Get(Guid postId)
        {
            return _context.Post
                .Include(x => x.Topic.Category)
                .Include(x => x.Topic.LastPost.User)
                .Include(x => x.User)
                .FirstOrDefault(x => x.Id == postId);
        }

        public IList<Post> GetPostsByTopics(List<Guid> topicIds, List<Category> allowedCategories)
        {

            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Post
                .Include(x => x.Topic.Category)
                .Include(x => x.Topic.LastPost)
                .Include(x => x.User)
                .AsNoTracking()
                .Where(x => topicIds.Contains(x.Topic.Id) && x.Pending != true)
                .Where(x => allowedCatIds.Contains(x.Topic.Category.Id))
                .OrderByDescending(x => x.DateCreated)
                .ToList();

        }

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="post"></param>
        /// <param name="ignoreLastPost"></param>
        /// <returns>Returns true if can delete</returns>
        public async Task<IPipelineProcess<Post>> Delete(Post post, bool ignoreLastPost)
        {
            // Get the pipelines
            var postPipes = ForumConfiguration.Instance.PipelinesPostDelete;

            // The model to process
            var piplineModel = new PipelineProcess<Post>(post);

            // Add extended data
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.IgnoreLastPost, ignoreLastPost);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Post>, Post>(_context);

            // Register the pipes 
            var allPostPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Post>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in postPipes)
            {
                if (allPostPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allPostPipes[pipe]);
                }
            }

            return await pipeline.Process(piplineModel);
        }

        public IList<Post> GetPostsByMember(Guid memberId, List<Category> allowedCategories)
        {
            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Post
                .Include(x => x.Topic.Category)
                .Include(x => x.User)
                .AsNoTracking()
                .Where(x => x.User.Id == memberId && x.Pending != true)
                .Where(x => allowedCatIds.Contains(x.Topic.Category.Id))
                .OrderByDescending(x => x.DateCreated)
                .ToList();

        }

        public IList<Post> GetAllSolutionPosts(List<Category> allowedCategories)
        {

            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Post
                .Include(x => x.Topic.Category)
                .Include(x => x.User)
                .AsNoTracking()
                .Where(x => x.IsSolution && x.Pending != true)
                .Where(x => allowedCatIds.Contains(x.Topic.Category.Id))
                .OrderByDescending(x => x.DateCreated)
                .ToList();


        }

        public IList<Post> GetPostsByTopic(Guid topicId)
        {

            return _context.Post
                .Include(x => x.Topic)
                .Include(x => x.User)
                .Where(x => x.Topic.Id == topicId && x.Pending != true)
                .OrderByDescending(x => x.DateCreated)
                .ToList();

        }

        public IEnumerable<Post> GetAllWithTopics(List<Category> allowedCategories)
        {

            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            return _context.Post
                .Include(x => x.Topic.Category)
                .Include(x => x.User)
                .Where(x => x.Pending != true)
                .Where(x => allowedCatIds.Contains(x.Topic.Category.Id));


        }

        public bool PassedPostFloodTest(MembershipUser user)
        {
            var timeNow = DateTime.UtcNow;
            var floodWindow = timeNow.AddSeconds(-ForumConfiguration.Instance.PostSecondsWaitBeforeNewPost);

            return _context.Post.AsNoTracking()
                    .Include(x => x.User)
                    .Count(x => x.User.Id == user.Id && x.DateCreated >= floodWindow && !x.IsTopicStarter) <= 0;
        }
    }
}
