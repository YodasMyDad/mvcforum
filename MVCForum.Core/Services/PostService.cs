namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Domain.Constants;
    using Domain.DomainModel;
    using Domain.Events;
    using Domain.Interfaces.Services;
    using System.Linq;
    using System.Data.Entity;
    using Domain.DomainModel.Entities;
    using Domain.DomainModel.LinqKit;
    using Domain.Interfaces;
    using Domain.Interfaces.UnitOfWork;
    using Data.Context;
    using Utilities;

    public partial class PostService : IPostService
    {
        private readonly IRoleService _roleService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService _localizationService;
        private readonly IVoteService _voteService;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly IFavouriteService _favouriteService;
        private readonly IConfigService _configService;
        private readonly MVCForumContext _context;
        private readonly IPostEditService _postEditService;
        private readonly ICacheService _cacheService;

        public PostService(IMVCForumContext context,IMembershipUserPointsService membershipUserPointsService,
            ISettingsService settingsService, IRoleService roleService,
            ILocalizationService localizationService, IVoteService voteService, IUploadedFileService uploadedFileService, IFavouriteService favouriteService, IConfigService configService, IPostEditService postEditService, ICacheService cacheService)
        {
            _roleService = roleService;
            _membershipUserPointsService = membershipUserPointsService;
            _settingsService = settingsService;
            _localizationService = localizationService;
            _voteService = voteService;
            _uploadedFileService = uploadedFileService;
            _favouriteService = favouriteService;
            _configService = configService;
            _postEditService = postEditService;
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }


        #region Private / Helpers Methods
        private MembershipRole UsersRole(MembershipUser user)
        {
            return user == null ? _roleService.GetRole(AppConstants.GuestRoleName) : user.Roles.FirstOrDefault();
        }

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
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetTopicStarterPost-", topicId);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Post
                                                                        .Include(x => x.Topic.Category)
                                                                        .Include(x => x.User)
                                                                        .FirstOrDefault(x => x.Topic.Id == topicId && x.IsTopicStarter));
        }

        /// <summary>
        /// Return all posts
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Post> GetAll(List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetAll-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                // get the category ids
                var allowedCatIds = allowedCategories.Select(x => x.Id);
                return _context.Post
                        .Include(x => x.Topic.Category)
                        .Where(x => allowedCatIds.Contains(x.Topic.Category.Id));
            });
        }

        /// <summary>
        /// Returns a list of posts ordered by the lowest vote
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetLowestVotedPost(int amountToTake)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetLowestVotedPost-", amountToTake);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                return _context.Post
                    .Include(x => x.Topic)
                    .Include(x => x.User)
                    .Where(x => x.VoteCount < 0 && x.Pending != true)
                    .OrderBy(x => x.VoteCount)
                    .Take(amountToTake)
                    .ToList();
            });
        }

        /// <summary>
        /// Returns a list of posts ordered by the highest vote
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetHighestVotedPost(int amountToTake)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetHighestVotedPost-", amountToTake);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                return _context.Post
                    .Include(x => x.Topic)
                    .Include(x => x.User)
                    .Where(x => x.VoteCount > 0 && x.Pending != true)
                    .OrderByDescending(x => x.VoteCount)
                    .Take(amountToTake)
                    .ToList();
            });

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
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetByMember-", amountToTake, "-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
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
            });
        }

        public IList<Post> GetReplyToPosts(Post post)
        {
            return GetReplyToPosts(post.Id);
        }

        public IList<Post> GetReplyToPosts(Guid postId)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetReplyToPosts-", postId);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                // We don't allow topic starters in the list OR solutions. As if it's marked as a solution, it's a solution for that topic
                // and moving it wouldn't make sense.
                return _context.Post.Where(x => x.InReplyTo != null & x.InReplyTo == postId && !x.IsTopicStarter && !x.IsSolution).ToList();
            });
        }

        public IEnumerable<Post> GetPostsByFavouriteCount(Guid postsByMemberId, int minAmountOfFavourites)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetPostsByFavouriteCount-", postsByMemberId, "-", minAmountOfFavourites);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                return _context.Post
                       .Include(x => x.Topic.LastPost.User)
                       .Include(x => x.Topic.Category)
                       .Include(x => x.User)
                       .Include(x => x.Favourites.Select(f => f.Member))
                       .Where(x => x.User.Id == postsByMemberId && x.Favourites.Count(c => c.Member.Id != postsByMemberId) >= minAmountOfFavourites);
            });
        }

        public IEnumerable<Post> GetPostsFavouritedByOtherMembers(Guid postsByMemberId)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetPostsFavouritedByOtherMembers-", postsByMemberId);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                return _context.Post
                            .Include(x => x.Topic.LastPost.User)
                            .Include(x => x.Topic.Category)
                            .Include(x => x.User)
                            .Include(x => x.Favourites.Select(f => f.Member))
                            .Where(x => x.User.Id == postsByMemberId && x.Favourites.Any(c => c.Member.Id != postsByMemberId));
            });

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
        public PagedList<Post> SearchPosts(int pageIndex, int pageSize, int amountToTake, string searchTerm, List<Category> allowedCategories)
        {
            // Create search term
            var search = StringUtils.ReturnSearchString(searchTerm);

            // Now split the words
            var splitSearch = search.Trim().Split(' ').ToList();

            // get the category ids
            var allowedCatIds = allowedCategories.Select(x => x.Id);

            var query = _context.Post.AsExpandable()
                            .Include(x => x.Topic.Category)
                            .Include(x => x.User)
                            .AsNoTracking()
                            .Where(x => x.Pending != true)
                            .Where(x => allowedCatIds.Contains(x.Topic.Category.Id));

            // Start the predicate builder
            var postFilter = PredicateBuilder.False<Post>();

            // Loop through each word and see if it's in the post
            foreach (var term in splitSearch)
            {
                var sTerm = term.Trim();
                //query = query.Where(x => x.PostContent.ToUpper().Contains(sTerm) || x.SearchField.ToUpper().Contains(sTerm));
                postFilter = postFilter.Or(x => x.PostContent.ToUpper().Contains(sTerm) || x.SearchField.ToUpper().Contains(sTerm));
            }

            // Add the predicate builder to the query
            query = query.Where(postFilter);

            // Get the count
            var total = query.Count();

            if (amountToTake < total)
            {
                total = amountToTake;
            }

            // Get the Posts and then get the topics from the post
            // This is an interim solution, as its flawed due to multiple posts in one topic so the paging might
            // be incorrect if all posts are from one topic.
            var results = query
                        .OrderByDescending(x => x.DateCreated)
                        .Skip((pageIndex - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();

            // Return a paged list
            return new PagedList<Post>(results, pageIndex, pageSize, total);
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
        public PagedList<Post> GetPagedPostsByTopic(int pageIndex, int pageSize, int amountToTake, Guid topicId, PostOrderBy order)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetPagedPostsByTopic-", pageIndex, "-", pageSize, "-", amountToTake, "-",topicId, "-", order);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                // We might only want to display the top 100
                // but there might not be 100 topics
                var total = _context.Post.AsNoTracking().Count(x => x.Topic.Id == topicId && !x.IsTopicStarter && x.Pending != true);
                if (amountToTake < total)
                {
                    total = amountToTake;
                }

                // Get the topics using an efficient
                var results = _context.Post
                                      .Include(x => x.User)
                                      .Include(x => x.Topic)
                                      .AsNoTracking()
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

                // sort the paging out
                var posts = results.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                // Return a paged list
                return new PagedList<Post>(posts, pageIndex, pageSize, total);
            });


        }

        public PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize, List<Category> allowedCategories)
        {
            var allowedCatIds = allowedCategories.Select(x => x.Id);
            var total = _context.Post.Count(x => x.Pending == true && allowedCatIds.Contains(x.Topic.Category.Id));
            var results = _context.Post
                .Include(x => x.Topic.Category)
                .Include(x => x.User)
                .AsNoTracking()
                .Where(x => x.Pending == true && allowedCatIds.Contains(x.Topic.Category.Id))
                .OrderBy(x => x.DateCreated)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize);

            return new PagedList<Post>(results.ToList(), pageIndex, pageSize, total);
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
                        if (permissions[SiteConstants.Instance.PermissionEditPosts].IsTicked)
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
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetPendingPostsCount-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var allowedCatIds = allowedCategories.Select(x => x.Id);
                return _context.Post.AsNoTracking().Include(x => x.Topic.Category).Count(x => x.Pending == true && allowedCatIds.Contains(x.Topic.Category.Id));
            });
        }

        /// <summary>
        /// Return all posts by a specified member that are marked as solution
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Post> GetSolutionsByMember(Guid memberId, List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetSolutionsByMember-", memberId, "-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
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
            });
        }

        /// <summary>
        /// Returns a count of all posts
        /// </summary>
        /// <returns></returns>
        public int PostCount(List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "PostCount-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                // get the category ids
                var allowedCatIds = allowedCategories.Select(x => x.Id);
                return _context.Post
                    .Include(x => x.Topic)
                    .AsNoTracking()
                    .Count(x => x.Pending != true && x.Topic.Pending != true && allowedCatIds.Contains(x.Topic.Category.Id));
            });
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public Post Add(Post post)
        {
            post = SanitizePost(post);
            return _context.Post.Add(post);
        }

        /// <summary>
        /// Return a post by Id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public Post Get(Guid postId)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "Get-", postId);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                return _context.Post
                    .Include(x => x.Topic.Category)
                    .Include(x => x.Topic.LastPost.User)
                    .Include(x => x.User)
                    .FirstOrDefault(x => x.Id == postId);
            });
        }

        public IList<Post> GetPostsByTopics(List<Guid> topicIds, List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetPostsByTopics-", topicIds.GetHashCode(), "-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
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
            });
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="post"></param>
        /// <param name="unitOfWork"></param>
        /// <param name="ignoreLastPost"></param>
        /// <returns>Returns true if can delete</returns>
        public bool Delete(Post post, IUnitOfWork unitOfWork, bool ignoreLastPost)
        {
            // Get the topic
            var topic = post.Topic;

            var votes = _voteService.GetVotesByPost(post.Id);

            #region Deleting Points

            // Remove the points the user got for this post
            _membershipUserPointsService.Delete(post.User, PointsFor.Post, post.Id);

            // Also get all the votes and delete anything to do with those
            foreach (var postVote in votes)
            {
                _membershipUserPointsService.Delete(PointsFor.Vote, postVote.Id);
            }

            // Also the mark as solution
            _membershipUserPointsService.Delete(PointsFor.Solution, post.Id);

            #endregion

            unitOfWork.SaveChanges();

            #region Deleting Votes

            var votesToDelete = new List<Vote>();
            votesToDelete.AddRange(votes);
            foreach (var vote in votesToDelete)
            {
                _voteService.Delete(vote);
            }
            post.Votes.Clear();

            #endregion

            unitOfWork.SaveChanges();

            #region Files

            // Clear files attached to post
            var filesToDelete = new List<UploadedFile>();
            filesToDelete.AddRange(post.Files);
            foreach (var uploadedFile in filesToDelete)
            {
                _uploadedFileService.Delete(uploadedFile);
            }
            post.Files.Clear();

            #endregion

            unitOfWork.SaveChanges();

            #region Favourites

            var postFavourites = new List<Favourite>();
            postFavourites.AddRange(post.Favourites);
            foreach (var postFavourite in postFavourites)
            {
                _favouriteService.Delete(postFavourite);
            }
            post.Favourites.Clear();

            #endregion

            unitOfWork.SaveChanges();

            #region Post Edits

            var postEdits = new List<PostEdit>();
            postEdits.AddRange(post.PostEdits); 
            _postEditService.Delete(postEdits);        
            post.PostEdits.Clear();

            #endregion

            unitOfWork.SaveChanges();

            // Before we delete the post, we need to check if this is the last post in the topic
            // and if so update the topic
            if (!ignoreLastPost)
            {
                var lastPost = topic.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();

                if (lastPost != null && lastPost.Id == post.Id)
                {
                    // Get the new last post and update the topic
                    topic.LastPost = topic.Posts.Where(x => x.Id != post.Id).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                }

                if (topic.Solved && post.IsSolution)
                {
                    topic.Solved = false;
                }
            }

            // Remove from the topic
            topic.Posts.Remove(post);

            // now delete the post
            _context.Post.Remove(post);

            // Save changes
            unitOfWork.SaveChanges();

            // Only the post was deleted, not the entire topic
            return false;
        }


        /// <summary>
        /// Add a new post
        /// </summary>
        /// <param name="postContent"> </param>
        /// <param name="topic"> </param>
        /// <param name="user"></param>
        /// <param name="permissions"> </param>
        /// <returns>True if post added</returns>
        public Post AddNewPost(string postContent, Topic topic, MembershipUser user, out PermissionSet permissions)
        {
            // Get the permissions for the category that this topic is in
            permissions = _roleService.GetPermissions(topic.Category, UsersRole(user));

            // Check this users role has permission to create a post
            if (permissions[SiteConstants.Instance.PermissionDenyAccess].IsTicked || permissions[SiteConstants.Instance.PermissionReadOnly].IsTicked)
            {
                // Throw exception so Ajax caller picks it up
                throw new ApplicationException(_localizationService.GetResourceString("Errors.NoPermission"));
            }

            // Has permission so create the post
            var newPost = new Post
                               {
                                   PostContent = postContent,
                                   User = user,
                                   Topic = topic,
                                   IpAddress = StringUtils.GetUsersIpAddress(),
                                   DateCreated = DateTime.UtcNow,
                                   DateEdited = DateTime.UtcNow
                               };

            // Sort the search field out

            var category = topic.Category;
            if (category.ModeratePosts == true)
            {
                newPost.Pending = true;
            }

            var e = new PostMadeEventArgs { Post = newPost };
            EventManager.Instance.FireBeforePostMade(this, e);

            if (!e.Cancel)
            {
                // create the post
                Add(newPost);

                // Update the users points score and post count for posting
                _membershipUserPointsService.Add(new MembershipUserPoints
                                                     {
                                                         Points = _settingsService.GetSettings().PointsAddedPerPost,
                                                         User = user,
                                                         PointsFor = PointsFor.Post,
                                                         PointsForId = newPost.Id
                                                     });

                // add the last post to the topic
                topic.LastPost = newPost;

                EventManager.Instance.FireAfterPostMade(this, new PostMadeEventArgs { Post = newPost });

                return newPost;
            }

            return newPost;
        }

        public string SortSearchField(bool isTopicStarter, Topic topic, IList<TopicTag> tags)
        {
            var formattedSearchField = string.Empty;
            if (isTopicStarter)
            {
                formattedSearchField = topic.Name;
            }
            if (tags != null && tags.Any())
            {
                var sb = new StringBuilder();
                foreach (var topicTag in tags)
                {
                    sb.Append(string.Concat(topicTag.Tag, " "));
                }
                formattedSearchField = !string.IsNullOrEmpty(formattedSearchField) ? string.Concat(formattedSearchField, " ", sb.ToString()) : sb.ToString();
            }
            return formattedSearchField.Trim();
        }

        public IList<Post> GetPostsByMember(Guid memberId, List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetPostsByMember-", memberId, "-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
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
            });
        }

        public IList<Post> GetAllSolutionPosts(List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetAllSolutionPosts-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
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
            });

        }

        public IList<Post> GetPostsByTopic(Guid topicId)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetPostsByTopic-", topicId);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                return _context.Post
                    .Include(x => x.Topic)
                    .Include(x => x.User)
                    .Where(x => x.Topic.Id == topicId && x.Pending != true)
                    .OrderByDescending(x => x.DateCreated)
                    .ToList();
            });
        }

        public IEnumerable<Post> GetAllWithTopics(List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Post.StartsWith, "GetAllWithTopics-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                // get the category ids
                var allowedCatIds = allowedCategories.Select(x => x.Id);
                return _context.Post
                    .Include(x => x.Topic.Category)
                    .Include(x => x.User)
                    .Where(x => x.Pending != true)
                    .Where(x => allowedCatIds.Contains(x.Topic.Category.Id));
            });

        }

        public bool PassedPostFloodTest(MembershipUser user)
        {
            var timeNow = DateTime.UtcNow;
            var floodWindow = timeNow.AddSeconds(-SiteConstants.Instance.PostSecondsWaitBeforeNewPost);

            return _context.Post
                    .Include(x => x.User)
                    .Count(x => x.User.Id == user.Id && x.DateCreated >= floodWindow && !x.IsTopicStarter) <= 0;
        }
    }
}
