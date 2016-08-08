namespace MVCForum.Website.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Domain.DomainModel;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;
    using ViewModels;
    using ViewModels.Mapping;

    public partial class FavouriteController : BaseController
    {
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;
        private readonly IFavouriteService _favouriteService;

        public FavouriteController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService,
            IRoleService roleService, ITopicService topicService, IPostService postService,
            ILocalizationService localizationService, ISettingsService settingsService, 
            IFavouriteService favouriteService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService, cacheService)
        {
            _topicService = topicService;
            _postService = postService;
            _favouriteService = favouriteService;
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