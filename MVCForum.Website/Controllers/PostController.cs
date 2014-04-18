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
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using MembershipUser = MVCForum.Domain.DomainModel.MembershipUser;

namespace MVCForum.Website.Controllers
{
    [Authorize]
    public class PostController : BaseController
    {
        private readonly ITopicService _topicService;
        private readonly ITopicTagService _topicTagService;
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;
        private readonly IEmailService _emailService;
        private readonly IReportService _reportService;
        private readonly ILuceneService _luceneService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly IPollService _pollService;
        private readonly IBannedWordService _bannedWordService;

        private MembershipUser LoggedOnUser;
        private MembershipRole UsersRole;

        public PostController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ITopicService topicService, IPostService postService, 
            ISettingsService settingsService, ICategoryService categoryService, ITopicTagService topicTagService, 
            ITopicNotificationService topicNotificationService, IEmailService emailService, IReportService reportService, ILuceneService luceneService, IPollAnswerService pollAnswerService, 
            IPollService pollService, IBannedWordService bannedWordService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _topicService = topicService;
            _postService = postService;
            _categoryService = categoryService;
            _topicTagService = topicTagService;
            _topicNotificationService = topicNotificationService;
            _emailService = emailService;
            _reportService = reportService;
            _luceneService = luceneService;
            _pollAnswerService = pollAnswerService;
            _pollService = pollService;
            _bannedWordService = bannedWordService;

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
            UsersRole = LoggedOnUser == null ? RoleService.GetRole(AppConstants.GuestRoleName) : LoggedOnUser.Roles.FirstOrDefault();
        }


        [HttpPost]
        public ActionResult CreatePost(CreateAjaxPostViewModel post)
        {
            PermissionSet permissions;
            Post newPost;
            Topic topic;
            var moderation = false;

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {   
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

                if(!akismetHelper.IsSpam(newPost))
                {
                    try
                    {
                        unitOfWork.Commit();

                        // Successful, add this post to the Lucene index
                        if (_luceneService.CheckIndexExists())
                        {
                            _luceneService.AddUpdate(_luceneService.MapToModel(newPost));
                        }

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
            else
            {

                // All good send the notifications and send the post back
                using (UnitOfWorkManager.NewUnitOfWork())
                {

                    // Create the view model
                    var viewModel = new ViewPostViewModel
                    {
                        Permissions = permissions,
                        Post = newPost,
                        User = LoggedOnUser,
                        ParentTopic = topic
                    };

                    // Success send any notifications
                    NotifyNewTopics(topic);

                    return PartialView("_Post", viewModel);
                }   
            }
        }

        public ActionResult EditPost(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Got to get a lot of things here as we have to check permissions
                // Get the post
                var post = _postService.Get(id);

                // Get the topic
                var topic = post.Topic;

                // get the users permissions
                var permissions = RoleService.GetPermissions(topic.Category, UsersRole);

                if (post.User.Id == LoggedOnUser.Id || permissions[AppConstants.PermissionEditPosts].IsTicked)
                {
                    var viewModel = new EditPostViewModel { Content = Server.HtmlDecode(post.PostContent), Id = post.Id, Permissions = permissions };

                    // Now check if this is a topic starter, if so add the rest of the field
                    if (post.IsTopicStarter)
                    {
                        viewModel.Category = topic.Category.Id;
                        viewModel.IsLocked = topic.IsLocked;
                        viewModel.IsSticky = topic.IsSticky;
                        viewModel.IsTopicStarter = post.IsTopicStarter;
                        // Tags
                        if (topic.Tags.Any())
                        {
                            viewModel.Tags = string.Join<string>(",", topic.Tags.Select(x => x.Tag));
                        }
                        viewModel.Name = topic.Name;
                        viewModel.Categories = _categoryService.GetAllowedCategories(UsersRole).ToList();
                        if(topic.Poll != null && topic.Poll.PollAnswers.Any())
                        {
                            // Has a poll so add it to the view model
                            viewModel.PollAnswers = topic.Poll.PollAnswers;
                        }
                    }

                    return View(viewModel);
                }
                return NoPermission(topic);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPost(EditPostViewModel editPostViewModel)
        {

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                // Got to get a lot of things here as we have to check permissions
                // Get the post
                var post = _postService.Get(editPostViewModel.Id);

                // Get the topic
                var topic = post.Topic;

                // get the users permissions
                var permissions = RoleService.GetPermissions(topic.Category, UsersRole);

                if (post.User.Id == LoggedOnUser.Id || permissions[AppConstants.PermissionEditPosts].IsTicked)
                {
                    // User has permission so update the post
                    post.PostContent = StringUtils.GetSafeHtml(_bannedWordService.SanitiseBannedWords(editPostViewModel.Content));
                    post.DateEdited = DateTime.UtcNow;

                    // if topic starter update the topic
                    if (post.IsTopicStarter)
                    {
                        // if category has changed then update it
                        if (topic.Category.Id != editPostViewModel.Category)
                        {
                            var cat = _categoryService.Get(editPostViewModel.Category);
                            topic.Category = cat;
                        }

                        topic.IsLocked = editPostViewModel.IsLocked;
                        topic.IsSticky = editPostViewModel.IsSticky;
                        topic.Name = StringUtils.GetSafeHtml(_bannedWordService.SanitiseBannedWords(editPostViewModel.Name));

                        // See if there is a poll
                        if (editPostViewModel.PollAnswers != null && editPostViewModel.PollAnswers.Count > 0)
                        {
                            // Now sort the poll answers, what to add and what to remove
                            // Poll answers already in this poll.
                            var postedIds = editPostViewModel.PollAnswers.Select(x => x.Id);
                            //var existingAnswers = topic.Poll.PollAnswers.Where(x => postedIds.Contains(x.Id)).ToList();
                            var existingAnswers = editPostViewModel.PollAnswers.Where(x => topic.Poll.PollAnswers.Select(p => p.Id).Contains(x.Id)).ToList();
                            var newPollAnswers = editPostViewModel.PollAnswers.Where(x => !topic.Poll.PollAnswers.Select(p => p.Id).Contains(x.Id)).ToList();
                            var pollAnswersToRemove = topic.Poll.PollAnswers.Where(x => !postedIds.Contains(x.Id)).ToList();

                            // Loop through existing and update names if need be
                            //TODO: Need to think about this in future versions if they change the name
                            //TODO: As they could game the system by getting votes and changing name?
                            foreach (var existPollAnswer in existingAnswers)
                            {
                                // Get the existing answer from the current topic
                                var pa = topic.Poll.PollAnswers.FirstOrDefault(x => x.Id == existPollAnswer.Id);
                                if (pa != null && pa.Answer != existPollAnswer.Answer)
                                {
                                    // If the answer has changed then update it
                                    pa.Answer = existPollAnswer.Answer;
                                }
                            }

                            // Loop through and remove the old poll answers and delete
                            foreach (var oldPollAnswer in pollAnswersToRemove)
                            {
                                // Delete
                                _pollAnswerService.Delete(oldPollAnswer);

                                // Remove from Poll
                                topic.Poll.PollAnswers.Remove(oldPollAnswer);
                            }

                            // Poll answers to add
                            foreach (var newPollAnswer in newPollAnswers)
                            {
                                var npa = new PollAnswer
                                {
                                    Poll = topic.Poll,
                                    Answer = newPollAnswer.Answer
                                };
                                _pollAnswerService.Add(npa);
                                topic.Poll.PollAnswers.Add(npa);
                            } 
                        }
                        else
                        {
                            // Need to check if this topic has a poll, because if it does
                            // All the answers have now been removed so remove the poll.
                            if (topic.Poll != null)
                            {
                                //Firstly remove the answers if there are any
                                if (topic.Poll.PollAnswers != null && topic.Poll.PollAnswers.Any())
                                {
                                    var answersToDelete = new List<PollAnswer>();
                                    answersToDelete.AddRange(topic.Poll.PollAnswers);
                                    foreach (var answer in answersToDelete)
                                    {
                                        // Delete
                                        _pollAnswerService.Delete(answer);

                                        // Remove from Poll
                                        topic.Poll.PollAnswers.Remove(answer);
                                    }
                                }

                                // Now delete the poll
                                var pollToDelete = topic.Poll;
                                _pollService.Delete(pollToDelete);

                                // Remove from topic.
                                topic.Poll = null;
                            }
                        }

                        // Tags
                        topic.Tags.Clear();
                        if(!string.IsNullOrEmpty(editPostViewModel.Tags))
                        {
                            _topicTagService.Add(editPostViewModel.Tags.ToLower(), topic);
                        }
                    }

                    // redirect back to topic
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Post.Updated"),
                        MessageType = GenericMessages.success
                    };
                    try
                    {
                        unitOfWork.Commit();

                        // Successful, add this post to the Lucene index
                        if (_luceneService.CheckIndexExists())
                        {
                            _luceneService.AddUpdate(_luceneService.MapToModel(post));
                        }

                        return Redirect(topic.NiceUrl);
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.GenericError"));
                    }
                }

                return NoPermission(topic); 
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

                    var deleteTopic = _postService.Delete(post);

                    unitOfWork.SaveChanges();

                    var postIdList = new List<Guid>();
                    if (deleteTopic)
                    {
                        postIdList = topic.Posts.Select(x => x.Id).ToList();
                        _topicService.Delete(topic);
                    }

                    try
                    {
                        unitOfWork.Commit();

                        // Successful, delete post or posts if its a topic deleted
                        if (_luceneService.CheckIndexExists())
                        {
                            if (deleteTopic)
                            {
                                foreach (var guid in postIdList)
                                {
                                    _luceneService.Delete(guid);
                                }
                            }
                            else
                            {
                                _luceneService.Delete(post.Id);
                            }
                        }
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
                MessageType = GenericMessages.error
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
                    return View(new ReportPostViewModel { PostId = post.Id, PostCreatorUsername = post.User.UserName});
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
