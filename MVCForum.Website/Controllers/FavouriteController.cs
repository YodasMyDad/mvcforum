using System;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;
using MVCForum.Website.ViewModels.Mapping;

namespace MVCForum.Website.Controllers
{
    public partial class FavouriteController : BaseController
    {
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;
        private readonly ITopicTagService _topicTagService;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryNotificationService _categoryNotificationService;
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly IEmailService _emailService;
        private readonly IPollService _pollService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly IBannedWordService _bannedWordService;
        private readonly IVoteService _voteService;
        private readonly IFavouriteService _favouriteService;
        private readonly IBadgeService _badgeService;

        public FavouriteController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, IRoleService roleService, ITopicService topicService, IPostService postService,
            ICategoryService categoryService, ILocalizationService localizationService, ISettingsService settingsService, ITopicTagService topicTagService, IMembershipUserPointsService membershipUserPointsService,
            ICategoryNotificationService categoryNotificationService, IEmailService emailService, ITopicNotificationService topicNotificationService, IPollService pollService,
            IPollAnswerService pollAnswerService, IBannedWordService bannedWordService, IVoteService voteService, IFavouriteService favouriteService, IBadgeService badgeService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _topicService = topicService;
            _postService = postService;
            _categoryService = categoryService;
            _topicTagService = topicTagService;
            _membershipUserPointsService = membershipUserPointsService;
            _categoryNotificationService = categoryNotificationService;
            _emailService = emailService;
            _topicNotificationService = topicNotificationService;
            _pollService = pollService;
            _pollAnswerService = pollAnswerService;
            _bannedWordService = bannedWordService;
            _voteService = voteService;
            _favouriteService = favouriteService;
            _badgeService = badgeService;
        }

        [Authorize]
        public ActionResult Index()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Get the favourites
                var favourites = _favouriteService.GetAllByMember(LoggedOnReadOnlyUser.Id);

                // Pull out the posts
                var posts = favourites.Select(x => x.Post);

                // Create the view Model
                var viewModel = new MyFavouritesViewModel();

                // Map the view models
                // TODO - Need to improve performance of this
                foreach (var post in posts)
                {
                    var permissions = RoleService.GetPermissions(post.Topic.Category, UsersRole);
                    var postViewModel = ViewModelMapping.CreatePostViewModel(post, post.Votes.ToList(), permissions, post.Topic, LoggedOnReadOnlyUser, SettingsService.GetSettings(), post.Favourites.ToList());
                    postViewModel.ShowTopicName = true;
                    viewModel.Posts.Add(postViewModel);
                }

                return View(viewModel);
            }
        }


        [HttpPost]
        [Authorize]
        public JsonResult FavouritePost(FavouritePostViewModel viewModel)
        {
            var returnValue = new FavouriteJsonReturnModel();
            if (Request.IsAjaxRequest() && LoggedOnReadOnlyUser != null)
            {
                using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var post = _postService.Get(viewModel.PostId);
                        var topic = _topicService.Get(post.Topic.Id);

                        // See if this is a user adding or removing the favourite
                        var loggedOnUser = MembershipService.GetUser(LoggedOnReadOnlyUser.Id);
                        var existingFavourite = _favouriteService.GetByMemberAndPost(loggedOnUser.Id, post.Id);
                        if (existingFavourite != null)
                        {
                            _favouriteService.Delete(existingFavourite);
                            returnValue.Message = LocalizationService.GetResourceString("Post.Favourite");
                        }
                        else
                        {
                            var favourite = new Favourite
                            {
                                DateCreated = DateTime.UtcNow,
                                Member = loggedOnUser,
                                Post = post,
                                Topic = topic
                            };
                            _favouriteService.Add(favourite);
                            returnValue.Message = LocalizationService.GetResourceString("Post.Favourited");
                            returnValue.Id = favourite.Id;
                        }

                        unitOfwork.Commit();
                        return Json(returnValue, JsonRequestBehavior.DenyGet);
                    }
                    catch (Exception ex)
                    {
                        unitOfwork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.Generic"));
                    }
                }
            }
            return Json(returnValue);
        }
    }
}