using System;
using System.Collections.Generic;
using System.Text;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using System.Linq;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly ITopicService _topicService;
        private readonly IRoleService _roleService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService _localizationService;
        private readonly IVoteService _voteService;
        private readonly IUploadedFileService _uploadedFileService;
        private readonly IFavouriteService _favouriteService;

        public PostService(IMembershipUserPointsService membershipUserPointsService,
            ISettingsService settingsService, IRoleService roleService, IPostRepository postRepository, ITopicService topicService,
            ILocalizationService localizationService, IVoteService voteService, IUploadedFileService uploadedFileService, IFavouriteService favouriteService)
        {
            _postRepository = postRepository;
            _topicService = topicService;
            _roleService = roleService;
            _membershipUserPointsService = membershipUserPointsService;
            _settingsService = settingsService;
            _localizationService = localizationService;
            _voteService = voteService;
            _uploadedFileService = uploadedFileService;
            _favouriteService = favouriteService;
        }


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
                post.PostContent = EmoticonUtils.Emotify(post.PostContent);   
            }

            return post;
        }

        public Post GetTopicStarterPost(Guid topicId)
        {
            return _postRepository.GetTopicStarterPost(topicId);
        }

        /// <summary>
        /// Return all posts
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Post> GetAll(List<Category> allowedCategories)
        {
            return _postRepository.GetAll(allowedCategories);
        }

        /// <summary>
        /// Returns a list of posts ordered by the lowest vote
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetLowestVotedPost(int amountToTake)
        {
            return _postRepository.GetLowestVotedPost(amountToTake);
        }

        /// <summary>
        /// Returns a list of posts ordered by the highest vote
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public IList<Post> GetHighestVotedPost(int amountToTake)
        {
            return _postRepository.GetHighestVotedPost(amountToTake);
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
            return _postRepository.GetByMember(memberId, amountToTake, allowedCategories);
        }

        public IEnumerable<Post> GetPostsByFavouriteCount(Guid postsByMemberId, int minAmountOfFavourites)
        {
            return _postRepository.GetPostsByFavouriteCount(postsByMemberId, minAmountOfFavourites);
        }

        public IEnumerable<Post> GetPostsFavouritedByOtherMembers(Guid postsByMemberId)
        {
            return _postRepository.GetPostsFavouritedByOtherMembers(postsByMemberId);
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
            var splitSearch = search.Split(' ').ToList();

            return _postRepository.SearchPosts(pageIndex, pageSize, amountToTake, splitSearch, allowedCategories);
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
            return _postRepository.GetPagedPostsByTopic(pageIndex, pageSize, amountToTake, topicId, order);
        }

        public PagedList<Post> GetPagedPendingPosts(int pageIndex, int pageSize)
        {
            return _postRepository.GetPagedPendingPosts(pageIndex, pageSize);
        }

        public int GetPendingPostsCount()
        {
            return _postRepository.GetPendingPostsCount();
        }

        /// <summary>
        /// Return all posts by a specified member that are marked as solution
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Post> GetSolutionsByMember(Guid memberId, List<Category> allowedCategories)
        {
            return _postRepository.GetSolutionsByMember(memberId, allowedCategories);
        }

        /// <summary>
        /// Returns a count of all posts
        /// </summary>
        /// <returns></returns>
        public int PostCount(List<Category> allowedCategories)
        {
            return _postRepository.PostCount(allowedCategories);
        }

        /// <summary>
        /// Create a new post
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public Post Add(Post post)
        {
            post = SanitizePost(post);
            return _postRepository.Add(post);
        }

        /// <summary>
        /// Return a post by Id
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public Post Get(Guid postId)
        {
            return _postRepository.Get(postId);
        }

        public IList<Post> GetPostsByTopics(List<Guid> topicIds, List<Category> allowedCategories)
        {
            return _postRepository.GetPostsByTopics(topicIds, allowedCategories);
        }

        /// <summary>
        /// Save / Update a post
        /// </summary>
        /// <param name="post"></param>
        public void SaveOrUpdate(Post post)
        {
            _postRepository.Update(post);
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="post"></param>
        /// <param name="unitOfWork"></param>
        /// <returns>Returns true if can delete, returns false if it's a topic</returns>
        public bool Delete(Post post, IUnitOfWork unitOfWork)
        {
            // Get the topic
            var topic = post.Topic;

            // If this is coming from a call that is deleting the entire topic, then just delete post
            if (post.IsTopicStarter)
            {
                // Pass it to be deleted
                _topicService.Delete(topic, unitOfWork);

                // Return true as the topic was deleted as well as this post
                return true;
            }

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

            #region Deleting Votes

            var votesToDelete = new List<Vote>();
            votesToDelete.AddRange(votes);
            foreach (var vote in votesToDelete)
            {
                _voteService.Delete(vote);
            }
            post.Votes.Clear();

            #endregion

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

            #region Favourites

            var postFavourites = new List<Favourite>();
            postFavourites.AddRange(post.Favourites);
            foreach (var postFavourite in postFavourites)
            {
                _favouriteService.Delete(postFavourite);
            }
            post.Favourites.Clear();

            #endregion

            // Before we delete the post, we need to check if this is the last post in the topic
            // and if so update the topic
                
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

            topic.Posts.Remove(post);

            // now delete the post
            _postRepository.Delete(post);

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
            if (permissions[AppConstants.PermissionDenyAccess].IsTicked || permissions[AppConstants.PermissionReadOnly].IsTicked)
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
    }
}
