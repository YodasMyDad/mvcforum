using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
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
    public partial class TopicController : BaseController
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

        private readonly MembershipUser LoggedOnUser;
        private readonly MembershipRole UsersRole;

        public TopicController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, IRoleService roleService, ITopicService topicService, IPostService postService,
            ICategoryService categoryService, ILocalizationService localizationService, ISettingsService settingsService, ITopicTagService topicTagService, IMembershipUserPointsService membershipUserPointsService,
            ICategoryNotificationService categoryNotificationService, IEmailService emailService, ITopicNotificationService topicNotificationService, IPollService pollService,
            IPollAnswerService pollAnswerService, IBannedWordService bannedWordService)
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

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
            UsersRole = LoggedOnUser == null ? RoleService.GetRole(AppConstants.GuestRoleName) : LoggedOnUser.Roles.FirstOrDefault();
        }

        [ChildActionOnly]
        public PartialViewResult GetTopicBreadcrumb(Topic topic)
        {
            var category = topic.Category;
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new BreadcrumbViewModel
                {
                    Categories = _categoryService.GetCategoryParents(category).ToList(),
                    Topic = topic
                };
                if (!viewModel.Categories.Any())
                {
                    viewModel.Categories.Add(topic.Category);
                }
                return PartialView("GetCategoryBreadcrumb", viewModel);
            }
        }

        public PartialViewResult CreateTopicButton()
        {
            var viewModel = new CreateTopicButtonViewModel
                {
                    LoggedOnUser = LoggedOnUser
                };

            if (LoggedOnUser != null)
            {
                // Add all categories to a permission set
                var allCategories = _categoryService.GetAll();
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    foreach (var category in allCategories)
                    {
                        // Now check to see if they have access to any categories
                        // if so, check they are allowed to create topics - If no to either set to false
                        viewModel.UserCanPostTopics = false;
                        var permissionSet = RoleService.GetPermissions(category, UsersRole);
                        if (permissionSet[AppConstants.PermissionCreateTopics].IsTicked)
                        {                            
                            viewModel.UserCanPostTopics = true;
                            break;
                        }
                    }
                }
            }
            return PartialView(viewModel);
        }

        [Authorize]
        public ActionResult Create()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                    var allowedCategories = _categoryService.GetAllowedCategories(UsersRole).ToList();
                    if (allowedCategories.Any() && LoggedOnUser.DisablePosting != true)
                    {
                        var viewModel = new CreateTopicViewModel
                        {
                            Categories = allowedCategories,
                            LoggedOnUser = LoggedOnUser
                        };

                        return View(viewModel);
                    }
                    return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }            
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateTopicViewModel topicViewModel)
        {
            if (ModelState.IsValid)
            {
                // Quick check to see if user is locked out, when logged in
                if (LoggedOnUser.IsLockedOut || LoggedOnUser.DisablePosting == true || !LoggedOnUser.IsApproved)
                {
                    FormsAuthentication.SignOut();
                    return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoAccess"));
                }

                var successfullyCreated = false;
                var moderate = false;
                Category category;
                var topic = new Topic();

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    // Not using automapper for this one only, as a topic is a post and topic in one
                    category = _categoryService.Get(topicViewModel.Category);

                    // First check this user is allowed to create topics in this category
                    var permissions = RoleService.GetPermissions(category, UsersRole);

                    // Check this users role has permission to create a post
                    if (permissions[AppConstants.PermissionDenyAccess].IsTicked || permissions[AppConstants.PermissionReadOnly].IsTicked || !permissions[AppConstants.PermissionCreateTopics].IsTicked)
                    {
                        // Throw exception so Ajax caller picks it up
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.NoPermission"));
                    }
                    else
                    {
                        // We get the banned words here and pass them in, so its just one call
                        // instead of calling it several times and each call getting all the words back
                        var bannedWordsList = _bannedWordService.GetAll();
                        List<string> bannedWords = null;
                        if (bannedWordsList.Any())
                        {
                            bannedWords = bannedWordsList.Select(x => x.Word).ToList();
                        }

                        topic = new Topic
                        {
                            Name = _bannedWordService.SanitiseBannedWords(topicViewModel.Name, bannedWords),
                            Category = category,
                            User = LoggedOnUser
                        };                       
                        
                        // See if the user has actually added some content to the topic
                        if (!string.IsNullOrEmpty(topicViewModel.Content))
                        {
                            // Check for any banned words
                            topicViewModel.Content = _bannedWordService.SanitiseBannedWords(topicViewModel.Content, bannedWords);

                            // See if this is a poll and add it to the topic
                            if (topicViewModel.PollAnswers != null && topicViewModel.PollAnswers.Count > 0)
                            {
                                // Do they have permission to create a new poll
                                if (permissions[AppConstants.PermissionCreatePolls].IsTicked)
                                {
                                    // Create a new Poll
                                    var newPoll = new Poll
                                    {
                                        User = LoggedOnUser
                                    };

                                    // Create the poll
                                    _pollService.Add(newPoll);

                                    // Save the poll in the context so we can add answers
                                    unitOfWork.SaveChanges();

                                    // Now sort the answers
                                    var newPollAnswers = new List<PollAnswer>();
                                    foreach (var pollAnswer in topicViewModel.PollAnswers)
                                    {
                                        // Attach newly created poll to each answer
                                        pollAnswer.Poll = newPoll;
                                        _pollAnswerService.Add(pollAnswer);
                                        newPollAnswers.Add(pollAnswer);
                                    }
                                    // Attach answers to poll
                                    newPoll.PollAnswers = newPollAnswers;

                                    // Save the new answers in the context
                                    unitOfWork.SaveChanges();

                                    // Add the poll to the topic
                                    topic.Poll = newPoll;   
                                }
                                else
                                {
                                   //No permission to create a Poll so show a message but create the topic
                                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                    {
                                        Message = LocalizationService.GetResourceString("Errors.NoPermissionPolls"),
                                        MessageType = GenericMessages.info
                                    };
                                }
                            }

                            // Update the users points score for posting
                            _membershipUserPointsService.Add(new MembershipUserPoints
                            {
                                Points = SettingsService.GetSettings().PointsAddedPerPost,
                                User = LoggedOnUser
                            });

                            // Check for moderation
                            if (category.ModerateTopics == true)
                            {
                                topic.Pending = true;
                                moderate = true;
                            }

                            // Create the topic
                            topic = _topicService.Add(topic);

                            // Save the changes
                            unitOfWork.SaveChanges();

                            // Now create and add the post to the topic
                            _topicService.AddLastPost(topic, topicViewModel.Content);

                            // Now check its not spam
                            var akismetHelper = new AkismetHelper(SettingsService);
                            if(!akismetHelper.IsSpam(topic))
                            {
                                // Add the tags if any too
                                if (!string.IsNullOrEmpty(topicViewModel.Tags))
                                {
                                    // Sanitise the tags
                                    topicViewModel.Tags = _bannedWordService.SanitiseBannedWords(topicViewModel.Tags, bannedWords);

                                    // Now add the tags
                                    _topicTagService.Add(topicViewModel.Tags.ToLower(), topic);
                                }

                                // Subscribe the user to the topic as they have checked the checkbox
                                if (topicViewModel.SubscribeToTopic)
                                {
                                    // Create the notification
                                    var topicNotification = new TopicNotification
                                    {
                                        Topic = topic,
                                        User = LoggedOnUser
                                    };
                                    //save
                                    _topicNotificationService.Add(topicNotification);
                                }

                                try
                                {
                                    unitOfWork.Commit();
                                    if (!moderate)
                                    {                                    
                                        successfullyCreated = true; 
                                    }
                                }
                                catch (Exception ex)
                                {
                                    unitOfWork.Rollback();
                                    LoggingService.Error(ex);
                                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                                }
                            }
                            else
                            {
                                unitOfWork.Rollback();
                                ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.PossibleSpam"));
                            }

                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                        }
                    }
                }

                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    if (successfullyCreated)
                    {
                        // Success so now send the emails
                        NotifyNewTopics(category);

                        // Redirect to the newly created topic
                        return Redirect(string.Format("{0}?postbadges=true", topic.NiceUrl));
                    }
                    if (moderate)
                    {
                        // Moderation needed
                        // Tell the user the topic is awaiting moderation
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Moderate.AwaitingModeration"),
                            MessageType = GenericMessages.info
                        };

                        return RedirectToAction("Index", "Home");
                    }

                    var allowedCategories = _categoryService.GetAllowedCategories(UsersRole).ToList();
                    if (allowedCategories.Any())
                    {
                        topicViewModel.Categories = allowedCategories;
                    }
                }
                return View(topicViewModel);
            }

            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
        }

        public ActionResult Show(string slug, int? p)
        {
            // Set the page index
            var pageIndex = p ?? 1;

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                // Get the topic
                var topic = _topicService.GetTopicBySlug(slug);

                if (topic != null)
                {
                    // Note: Don't use topic.Posts as its not a very efficient SQL statement
                    // Use the post service to get them as it includes other used entities in one
                    // statement rather than loads of sql selects

                    var sortQuerystring = Request.QueryString[SiteConstants.PostOrderBy];
                    var orderBy = !string.IsNullOrEmpty(sortQuerystring) ? 
                                              EnumUtils.ReturnEnumValueFromString<PostOrderBy>(sortQuerystring) : PostOrderBy.Standard;

                    // Store the amount per page
                    var amountPerPage = SettingsService.GetSettings().PostsPerPage;

                    if (sortQuerystring == SiteConstants.AllPosts)
                    {
                        // Overide to show all posts
                        amountPerPage = int.MaxValue;
                    }

                    // Get the posts
                    var posts = _postService.GetPagedPostsByTopic(pageIndex,
                                                                  amountPerPage,
                                                                  int.MaxValue, 
                                                                  topic.Id,
                                                                  orderBy);

                    // Get the topic starter post
                    var topicStarter = _postService.GetTopicStarterPost(topic.Id);

                    // Get the permissions for the category that this topic is in
                    var permissions = RoleService.GetPermissions(topic.Category, UsersRole);

                    // If this user doesn't have access to this topic then
                    // redirect with message
                    if (permissions[AppConstants.PermissionDenyAccess].IsTicked)
                    {
                        return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
                    }
                    
                    // See if the user has subscribed to this topic or not
                    var isSubscribed = UserIsAuthenticated && (_topicNotificationService.GetByUserAndTopic(LoggedOnUser, topic).Any());

                    // Populate the view model for this page
                    var viewModel = new ShowTopicViewModel
                    {
                        Topic = topic,
                        Category = topic.Category,
                        Posts = posts,
                        PageIndex = posts.PageIndex,
                        TotalCount = posts.TotalCount,
                        Permissions = permissions,
                        User = LoggedOnUser,
                        IsSubscribed = isSubscribed,
                        UserHasAlreadyVotedInPoll = false,
                        TopicStarterPost = topicStarter
                    };

                    // If there is a quote querystring
                    var quote = Request["quote"];
                    if (!string.IsNullOrEmpty(quote))
                    {
                        try
                        {
                            // Got a quote
                            var postToQuote = _postService.Get(new Guid(quote));
                            viewModel.PostContent = postToQuote.PostContent;
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Error(ex);
                        }
                    }

                    // See if the topic has a poll, and if so see if this user viewing has already voted
                    if (topic.Poll != null)
                    {
                        // There is a poll and a user
                        // see if the user has voted or not
                        var votes = topic.Poll.PollAnswers.SelectMany(x => x.PollVotes).ToList();
                        if (UserIsAuthenticated)
                        {
                            viewModel.UserHasAlreadyVotedInPoll = (votes.Count(x => x.User.Id == LoggedOnUser.Id) > 0);                            
                        }
                        viewModel.TotalVotesInPoll = votes.Count();
                    }

                    // User has permission lets update the topic view count
                    // but only if this topic doesn't belong to the user looking at it
                    var addView = !(UserIsAuthenticated && LoggedOnUser.Id == topic.User.Id);

                    if (!BotUtils.UserIsBot() && addView)
                    {
                        // Cool, user doesn't own this topic
                        topic.Views = (topic.Views + 1);
                        try
                        {
                            unitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            LoggingService.Error(ex);
                        }
                    }

                    return View(viewModel);
                }

            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        [HttpPost]
        public PartialViewResult AjaxMorePosts(GetMorePostsViewModel getMorePostsViewModel)
        {
            // Get the topic
            var topic = _topicService.Get(getMorePostsViewModel.TopicId);

            // Get the permissions for the category that this topic is in
            var permissions = RoleService.GetPermissions(topic.Category, UsersRole);

            // If this user doesn't have access to this topic then just return nothing
            if (permissions[AppConstants.PermissionDenyAccess].IsTicked)
            {
                return null;
            }

            var orderBy = !string.IsNullOrEmpty(getMorePostsViewModel.Order) ?
                                      EnumUtils.ReturnEnumValueFromString<PostOrderBy>(getMorePostsViewModel.Order) : PostOrderBy.Standard;


            var viewModel = new ShowMorePostsViewModel
                {
                    Posts = _postService.GetPagedPostsByTopic(getMorePostsViewModel.PageIndex, SettingsService.GetSettings().PostsPerPage, int.MaxValue, topic.Id, orderBy),
                    Topic = topic,
                    Permissions = permissions,
                    User = LoggedOnUser
                };
                        
            return PartialView(viewModel);
        }

        public ActionResult TopicsByTag(string tag, int? p)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Set the page index
                var pageIndex = p ?? 1;

                // Get the topics
                var topics = _topicService.GetPagedTopicsByTag(pageIndex,
                                                           SettingsService.GetSettings().TopicsPerPage,
                                                           SiteConstants.ActiveTopicsListSize,
                                                           tag);

                // Get the Topic View Models
                var topicViewModels = CreateTopicViewModels(topics);

                // create the view model
                var viewModel = new TagTopicsViewModel
                {
                    Topics = topicViewModels,
                    PageIndex = pageIndex,
                    TotalCount = topics.TotalCount,
                    TotalPages = topics.TotalPages,
                    Tag = tag
                };
                
                return View(viewModel);
            }
        }

        [HttpPost]
        public PartialViewResult GetSimilarTopics(string searchTerm)
        {
            // Returns the formatted string to search on
            var formattedSearchTerm = StringUtils.ReturnSearchString(searchTerm);

            List<Topic> topics = null;
            try
            {
                var searchResults = _topicService.SearchTopics(0, SiteConstants.SimilarTopicsListSize, SiteConstants.SimilarTopicsListSize, formattedSearchTerm);
                if (searchResults != null)
                {
                    topics = searchResults.ToList();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Error(ex);
            }

            // Pass the list to the partial view
            return PartialView(topics);
        }

        [ChildActionOnly]
        public ActionResult LatestTopics(int? p)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Set the page index
                var pageIndex = p ?? 1;

                // Get the topics
                var topics = _topicService.GetRecentTopics(pageIndex,
                                                           SettingsService.GetSettings().TopicsPerPage,
                                                           SiteConstants.ActiveTopicsListSize);

                // Get the Topic View Models
                var topicViewModels = CreateTopicViewModels(topics);

                // create the view model
                var viewModel = new ActiveTopicsViewModel
                {
                    Topics = topicViewModels,
                    PageIndex = pageIndex,
                    TotalCount = topics.TotalCount,
                    TotalPages = topics.TotalPages
                };
                
                return PartialView(viewModel);
            }
        }

        private Dictionary<Category, PermissionSet> GetPermissionsForTopics(List<Topic> topics)
        {
            // Get all the categories for this topic collection
                var categories = topics.Select(x => x.Category).Distinct();

                // Permissions
                // loop through the categories and get the permissions
            var permissions = new Dictionary<Category, PermissionSet>();
                foreach (var category in categories)
                {
                    var permissionSet = RoleService.GetPermissions(category, UsersRole);
                    permissions.Add(category, permissionSet);
                }
            return permissions;
        }

        private List<TopicViewModel> CreateTopicViewModels(List<Topic> topics, bool showCategoryName = true)
        {
            // Get Permissions
            var permissions = GetPermissionsForTopics(topics);
            var viewModels = new List<TopicViewModel>();
            foreach (var topic in topics)
            {
                var permission = permissions[topic.Category];
                var posts = topic.Posts.ToList();
                var starterPost = posts.FirstOrDefault(x => x.IsTopicStarter);
                viewModels.Add(new TopicViewModel
                {
                    Permissions = permission, 
                    Topic = topic, 
                    ShowCategoryName = showCategoryName,
                    StarterPost = starterPost,
                    VotesUp = starterPost.Votes.Count(x => x.Amount > 0),
                    VotesDown = starterPost.Votes.Count(x => x.Amount < 0),
                    Views = topic.Views,
                    Answers = (posts.Count() - 1),
                    Posts = posts
                });   
            }
            return viewModels;
        } 

        private void NotifyNewTopics(Category cat)
        {
                // *CHANGE THIS TO BE CALLED LIKE THE BADGES VIA AN AJAX Method* 
                // TODO: This really needs to be an async call so it doesn't hang when a user creates  
                // TODO: a topic if there are 1000's of users

                // Get all notifications for this category
                var notifications = _categoryNotificationService.GetByCategory(cat).Select(x => x.User.Id).ToList();

                if(notifications.Any())
                {
                    // remove the current user from the notification, don't want to notify yourself that you 
                    // have just made a topic!
                    notifications.Remove(LoggedOnUser.Id);

                    if(notifications.Count > 0)
                    {
                        // Now get all the users that need notifying
                        var usersToNotify = MembershipService.GetUsersById(notifications);

                        // Create the email
                        var sb = new StringBuilder();
                        sb.AppendFormat("<p>{0}</p>", string.Format(LocalizationService.GetResourceString("Topic.Notification.NewTopics"), cat.Name));
                        sb.AppendFormat("<p>{0}</p>", string.Concat(SettingsService.GetSettings().ForumUrl, cat.NiceUrl));

                        // create the emails and only send them to people who have not had notifications disabled
                        var emails = usersToNotify.Where(x => x.DisableEmailNotifications != true).Select(user => new Email
                        {
                            Body = _emailService.EmailTemplate(user.UserName, sb.ToString()),
                            EmailFrom = SettingsService.GetSettings().NotificationReplyEmail,
                            EmailTo = user.Email,
                            NameTo = user.UserName,
                            Subject = string.Concat(LocalizationService.GetResourceString("Topic.Notification.Subject"), SettingsService.GetSettings().ForumName)
                        }).ToList();

                        // and now pass the emails in to be sent
                        _emailService.SendMail(emails); 
                    }
                }
        }

    }
}
