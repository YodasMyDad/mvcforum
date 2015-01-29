using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using System.Linq;
using System.Web.Security;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using MVCForum.Website.ViewModels.Mapping;
using MembershipUser = MVCForum.Domain.DomainModel.MembershipUser;

namespace MVCForum.Website.Controllers
{
    [Authorize]
    public partial class PostController : BaseController
    {
        private readonly ITopicService _topicService;
        private readonly ITopicTagService _topicTagService;
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;
        private readonly IEmailService _emailService;
        private readonly IReportService _reportService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly IPollService _pollService;
        private readonly IBannedWordService _bannedWordService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;

        private MembershipUser LoggedOnUser;
        private MembershipRole UsersRole;

        public PostController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ITopicService topicService, IPostService postService,
            ISettingsService settingsService, ICategoryService categoryService, ITopicTagService topicTagService,
            ITopicNotificationService topicNotificationService, IEmailService emailService, IReportService reportService, IPollAnswerService pollAnswerService,
            IPollService pollService, IBannedWordService bannedWordService, IMembershipUserPointsService membershipUserPointsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _topicService = topicService;
            _postService = postService;
            _categoryService = categoryService;
            _topicTagService = topicTagService;
            _topicNotificationService = topicNotificationService;
            _emailService = emailService;
            _reportService = reportService;
            _pollAnswerService = pollAnswerService;
            _pollService = pollService;
            _bannedWordService = bannedWordService;
            _membershipUserPointsService = membershipUserPointsService;

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
            UsersRole = LoggedOnUser == null ? RoleService.GetRole(AppConstants.GuestRoleName) : LoggedOnUser.Roles.FirstOrDefault();
        }


        [HttpPost]
        public ActionResult CreatePost(CreateAjaxPostViewModel post)
        {
            PermissionSet permissions;
            Post newPost;
            Topic topic;

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                // Check stop words
                var stopWords = _bannedWordService.GetAll(true);
                foreach (var stopWord in stopWords)
                {
                    if (post.PostContent.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        throw new Exception(LocalizationService.GetResourceString("StopWord.Error"));
                    }
                }

                // Quick check to see if user is locked out, when logged in
                if (LoggedOnUser.IsLockedOut | !LoggedOnUser.IsApproved)
                {
                    FormsAuthentication.SignOut();
                    throw new Exception(LocalizationService.GetResourceString("Errors.NoAccess"));
                }

                topic = _topicService.Get(post.Topic);

                var postContent = _bannedWordService.SanitiseBannedWords(post.PostContent);

                var akismetHelper = new AkismetHelper(SettingsService);

                newPost = _postService.AddNewPost(postContent, topic, LoggedOnUser, out permissions);

                if (!akismetHelper.IsSpam(newPost))
                {
                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
                else
                {
                    unitOfWork.Rollback();
                    throw new Exception(LocalizationService.GetResourceString("Errors.PossibleSpam"));
                }


            }

            //Check for moderation
            if (newPost.Pending == true)
            {
                return PartialView("_PostModeration");
            }

            // All good send the notifications and send the post back
            using (UnitOfWorkManager.NewUnitOfWork())
            {

                // Create the view model
                var viewModel = ViewModelMapping.CreatePostViewModel(newPost, new List<Vote>(), permissions, topic, LoggedOnUser, SettingsService.GetSettings(), new List<Favourite>());

                // Success send any notifications
                NotifyNewTopics(topic);

                // Return view
                return PartialView("_Post", viewModel);
            }
        }

        public ActionResult DeletePost(Guid id)
        {
            bool isTopicStarter;
            Topic topic;

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                // Got to get a lot of things here as we have to check permissions
                // Get the post
                var post = _postService.Get(id);

                // get this so we know where to redirect after
                isTopicStarter = post.IsTopicStarter;

                // Get the topic
                topic = post.Topic;

                // get the users permissions
                var permissions = RoleService.GetPermissions(topic.Category, UsersRole);

                if (post.User.Id == LoggedOnUser.Id || permissions[AppConstants.PermissionDeletePosts].IsTicked)
                {
                    var postUser = post.User;

                    var deleteTopic = _postService.Delete(post);
                    unitOfWork.SaveChanges();

                    var postIdList = new List<Guid>();
                    if (deleteTopic)
                    {
                        postIdList = topic.Posts.Select(x => x.Id).ToList();
                        _topicService.Delete(topic);
                    }

                    // Remove the points the user got for this post
                    _membershipUserPointsService.Delete(SettingsService.GetSettings().PointsAddedPerPost, postUser);

                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }

            // Deleted successfully
            if (isTopicStarter)
            {
                // Redirect to root as this was a topic and deleted
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Topic.Deleted"),
                    MessageType = GenericMessages.success
                };
                return RedirectToAction("Index", "Home");
            }

            // Show message that post is deleted
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = LocalizationService.GetResourceString("Post.Deleted"),
                MessageType = GenericMessages.success
            };

            return Redirect(topic.NiceUrl);
        }

        private ActionResult NoPermission(Topic topic)
        {
            // Trying to be a sneaky mo fo, so tell them
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = LocalizationService.GetResourceString("Errors.NoPermission"),
                MessageType = GenericMessages.danger
            };
            return Redirect(topic.NiceUrl);
        }

        private void NotifyNewTopics(Topic topic)
        {
            // Get all notifications for this category
            var notifications = _topicNotificationService.GetByTopic(topic).Select(x => x.User.Id).ToList();

            if (notifications.Any())
            {
                // remove the current user from the notification, don't want to notify yourself that you 
                // have just made a topic!
                notifications.Remove(LoggedOnUser.Id);

                if (notifications.Count > 0)
                {
                    // Now get all the users that need notifying
                    var usersToNotify = MembershipService.GetUsersById(notifications);

                    // Create the email
                    var sb = new StringBuilder();
                    sb.AppendFormat("<p>{0}</p>", string.Format(LocalizationService.GetResourceString("Post.Notification.NewPosts"), topic.Name));
                    sb.AppendFormat("<p>{0}</p>", string.Concat(SettingsService.GetSettings().ForumUrl, topic.NiceUrl));

                    // create the emails only to people who haven't had notifications disabled
                    var emails = usersToNotify.Where(x => x.DisableEmailNotifications != true).Select(user => new Email
                    {
                        Body = _emailService.EmailTemplate(user.UserName, sb.ToString()),
                        EmailFrom = SettingsService.GetSettings().NotificationReplyEmail,
                        EmailTo = user.Email,
                        NameTo = user.UserName,
                        Subject = string.Concat(LocalizationService.GetResourceString("Post.Notification.Subject"), SettingsService.GetSettings().ForumName)
                    }).ToList();

                    // and now pass the emails in to be sent
                    _emailService.SendMail(emails);
                }
            }
        }

        [Authorize]
        public ActionResult Report(Guid id)
        {
            if (SettingsService.GetSettings().EnableSpamReporting)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var post = _postService.Get(id);
                    return View(new ReportPostViewModel { PostId = post.Id, PostCreatorUsername = post.User.UserName });
                }
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        [HttpPost]
        [Authorize]
        public ActionResult Report(ReportPostViewModel viewModel)
        {
            if (SettingsService.GetSettings().EnableSpamReporting)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var post = _postService.Get(viewModel.PostId);
                    var report = new Report
                    {
                        Reason = viewModel.Reason,
                        ReportedPost = post,
                        Reporter = LoggedOnUser
                    };
                    _reportService.PostReport(report);

                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Report.ReportSent"),
                        MessageType = GenericMessages.success
                    };
                    return View(new ReportPostViewModel { PostId = post.Id, PostCreatorUsername = post.User.UserName });
                }
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

    }
}
