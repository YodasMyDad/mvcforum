namespace MvcForum.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Areas.Admin.ViewModels;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.Enums;
    using Core.Models.General;
    using Core.Utilities;
    using ViewModels.Breadcrumb;
    using ViewModels.ExtensionMethods;
    using ViewModels.Mapping;
    using ViewModels.Post;
    using ViewModels.Topic;

    public partial class TopicController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IFavouriteService _favouriteService;
        private readonly INotificationService _notificationService;
        private readonly IPollService _pollService;
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private readonly ITopicTagService _topicTagService;
        private readonly IVoteService _voteService;

        public TopicController(ILoggingService loggingService, IMembershipService membershipService,
            IRoleService roleService, ITopicService topicService, IPostService postService,
            ICategoryService categoryService, ILocalizationService localizationService,
            ISettingsService settingsService, ITopicTagService topicTagService,
            IPollService pollService, IVoteService voteService, IFavouriteService favouriteService, ICacheService cacheService,
            IMvcForumContext context, INotificationService notificationService)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _topicService = topicService;
            _postService = postService;
            _categoryService = categoryService;
            _topicTagService = topicTagService;
            _pollService = pollService;
            _voteService = voteService;
            _favouriteService = favouriteService;
            _notificationService = notificationService;
        }


        [ChildActionOnly]
        [Authorize]
        public PartialViewResult TopicsMemberHasPostedIn(int? p)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnloggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnloggedOnUsersRole);
            var settings = SettingsService.GetSettings();
            // Set the page index
            var pageIndex = p ?? 1;

            // Get the topics
            var topics = Task.Run(() => _topicService.GetMembersActivity(pageIndex,
                settings.TopicsPerPage,
                ForumConfiguration.Instance.MembersActivityListSize,
                loggedOnReadOnlyUser.Id,
                allowedCategories)).Result;

            // Get the Topic View Models
            var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics, RoleService, loggedOnloggedOnUsersRole,
                loggedOnReadOnlyUser, allowedCategories, settings, _postService, _notificationService,
                _pollService, _voteService, _favouriteService);

            // create the view model
            var viewModel = new PostedInViewModel
            {
                Topics = topicViewModels,
                PageIndex = pageIndex,
                TotalCount = topics.TotalCount,
                TotalPages = topics.TotalPages
            };

            return PartialView("TopicsMemberHasPostedIn", viewModel);
        }

        [ChildActionOnly]
        [Authorize]
        public PartialViewResult GetSubscribedTopics()
        {
            var viewModel = new List<TopicViewModel>();

            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnloggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);


            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnloggedOnUsersRole);
            var topicIds = loggedOnReadOnlyUser.TopicNotifications.Select(x => x.Topic.Id).ToList();
            if (topicIds.Any())
            {
                var topics = _topicService.Get(topicIds, allowedCategories);

                // Get the Topic View Models
                viewModel = ViewModelMapping.CreateTopicViewModels(topics, RoleService, loggedOnloggedOnUsersRole,
                    loggedOnReadOnlyUser, allowedCategories, SettingsService.GetSettings(), _postService,
                    _notificationService, _pollService, _voteService, _favouriteService);

                // Show the unsubscribe link
                foreach (var topicViewModel in viewModel)
                {
                    topicViewModel.ShowUnSubscribedLink = true;
                }
            }

            return PartialView("GetSubscribedTopics", viewModel);
        }

        [ChildActionOnly]
        public PartialViewResult GetTopicBreadcrumb(Topic topic)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnloggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            var category = topic.Category;
            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnloggedOnUsersRole);

            var viewModel = new BreadcrumbViewModel
            {
                Categories = _categoryService.GetCategoryParents(category, allowedCategories),
                Topic = topic
            };
            if (!viewModel.Categories.Any())
            {
                viewModel.Categories.Add(topic.Category);
            }
            return PartialView("GetCategoryBreadcrumb", viewModel);
        }

        public PartialViewResult CreateTopicButton()
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnloggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            var viewModel = new CreateTopicButtonViewModel
            {
                LoggedOnUser = loggedOnReadOnlyUser
            };

            if (loggedOnReadOnlyUser != null)
            {
                // Add all categories to a permission set
                var allCategories = _categoryService.GetAll();

                foreach (var category in allCategories)
                {
                    // Now check to see if they have access to any categories
                    // if so, check they are allowed to create topics - If no to either set to false
                    viewModel.UserCanPostTopics = false;
                    var permissionSet = RoleService.GetPermissions(category, loggedOnloggedOnUsersRole);
                    if (permissionSet[ForumConfiguration.Instance.PermissionCreateTopics].IsTicked)
                    {
                        viewModel.UserCanPostTopics = true;
                        break;
                    }
                }
            }
            return PartialView(viewModel);
        }

        [HttpPost]
        [Authorize]
        public JsonResult CheckTopicCreatePermissions(Guid catId)
        {
            if (Request.IsAjaxRequest())
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnloggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
                var category = _categoryService.Get(catId);
                var permissionSet = RoleService.GetPermissions(category, loggedOnloggedOnUsersRole);
                var model = GetCheckCreateTopicPermissions(permissionSet);
                return Json(model);
            }
            return null;
        }

        #region Create / Edit Helper Methods

        private static CheckCreateTopicPermissions GetCheckCreateTopicPermissions(PermissionSet permissionSet)
        {
            var model = new CheckCreateTopicPermissions();

            if (permissionSet[ForumConfiguration.Instance.PermissionCreateStickyTopics].IsTicked)
            {
                model.CanStickyTopic = true;
            }

            if (permissionSet[ForumConfiguration.Instance.PermissionLockTopics].IsTicked)
            {
                model.CanLockTopic = true;
            }

            if (permissionSet[ForumConfiguration.Instance.PermissionAttachFiles].IsTicked)
            {
                model.CanUploadFiles = true;
            }

            if (permissionSet[ForumConfiguration.Instance.PermissionCreatePolls].IsTicked)
            {
                model.CanCreatePolls = true;
            }

            if (permissionSet[ForumConfiguration.Instance.PermissionInsertEditorImages].IsTicked)
            {
                model.CanInsertImages = true;
            }

            if (permissionSet[ForumConfiguration.Instance.PermissionCreateTags].IsTicked)
            {
                model.CanCreateTags = true;
            }
            return model;
        }

        #endregion

        private CreateEditTopicViewModel PrePareCreateEditTopicViewModel(List<Category> allowedCategories)
        {
            var userIsAdmin = User.IsInRole(Constants.AdminRoleName);
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            var permissions = RoleService.GetPermissions(null, loggedOnUsersRole);
            var canInsertImages = userIsAdmin;
            if (!canInsertImages)
            {
                canInsertImages = permissions[ForumConfiguration.Instance.PermissionInsertEditorImages].IsTicked;
            }
            return new CreateEditTopicViewModel
            {
                SubscribeToTopic = true,
                Categories = _categoryService.GetBaseSelectListCategories(allowedCategories),
                OptionalPermissions = new CheckCreateTopicPermissions
                {
                    CanLockTopic = userIsAdmin,
                    CanStickyTopic = userIsAdmin,
                    CanUploadFiles = userIsAdmin,
                    CanCreatePolls = userIsAdmin,
                    CanInsertImages = canInsertImages,
                    CanCreateTags = userIsAdmin
                },
                PollAnswers = new List<PollAnswer>(),
                IsTopicStarter = true,
                PollCloseAfterDays = 0
            };
        }

        private List<Category> AllowedCreateCategories(MembershipRole loggedOnUsersRole)
        {
            var allowedAccessCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);
            var allowedCreateTopicCategories =
                _categoryService.GetAllowedCategories(loggedOnUsersRole,
                    ForumConfiguration.Instance.PermissionCreateTopics);
            var allowedCreateTopicCategoryIds = allowedCreateTopicCategories.Select(x => x.Id);
            if (allowedAccessCategories.Any())
            {
                allowedAccessCategories.RemoveAll(x => allowedCreateTopicCategoryIds.Contains(x.Id));
                allowedAccessCategories.RemoveAll(x =>
                    loggedOnUsersRole.RoleName != Constants.AdminRoleName && x.IsLocked);
            }
            return allowedAccessCategories;
        }

        /// <summary>
        ///     Create topic view
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Create()
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            var allowedAccessCategories = AllowedCreateCategories(loggedOnUsersRole);

            if (allowedAccessCategories.Any() && loggedOnReadOnlyUser.DisablePosting != true)
            {
                var viewModel = PrePareCreateEditTopicViewModel(allowedAccessCategories);
                return View(viewModel);
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
        }

        /// <summary>
        ///     Creates a topic via the pipeline system
        /// </summary>
        /// <param name="topicViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateEditTopicViewModel topicViewModel)
        {
            // Get the user and roles
            var loggedOnUser = User.GetMembershipUser(MembershipService, false);
            var loggedOnUsersRole = loggedOnUser.GetRole(RoleService);

            // Get the category
            var category = _categoryService.Get(topicViewModel.Category);

            // First check this user is allowed to create topics in this category
            var permissions = RoleService.GetPermissions(category, loggedOnUsersRole);

            // Now we have the category and permissionSet - Populate the optional permissions 
            // This is just in case the viewModel is return back to the view also sort the allowedCategories
            topicViewModel.OptionalPermissions = GetCheckCreateTopicPermissions(permissions);
            topicViewModel.Categories =
                _categoryService.GetBaseSelectListCategories(AllowedCreateCategories(loggedOnUsersRole));
            topicViewModel.IsTopicStarter = true;
            if (topicViewModel.PollAnswers == null)
            {
                topicViewModel.PollAnswers = new List<PollAnswer>();
            }

            if (ModelState.IsValid)
            {
                // See if the user has actually added some content to the topic
                if (string.IsNullOrWhiteSpace(topicViewModel.Content))
                {
                    ModelState.AddModelError(string.Empty,
                        LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
                else
                {
                    // Map the new topic (Pass null for new topic)
                    var topic = topicViewModel.ToTopic(category, loggedOnUser, null);

                    // Run the create pipeline
                    var createPipeLine = await _topicService.Create(topic, topicViewModel.Files, topicViewModel.Tags,
                        topicViewModel.SubscribeToTopic, topicViewModel.Content, null);
                    if (createPipeLine.Successful == false)
                    {
                        // Remove the topic if unsuccessful, as we may have saved some items.
                        _topicService.Delete(createPipeLine.EntityToProcess);
                        await _topicService.SaveChanges();

                        // Tell the user the topic is awaiting moderation
                        ModelState.AddModelError(string.Empty, createPipeLine.ProcessLog.FirstOrDefault());
                        return View(topicViewModel);
                    }

                    if (createPipeLine.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.Moderate))
                    {
                        var moderate = createPipeLine.ExtendedData[Constants.ExtendedDataKeys.Moderate] as bool?;
                        if (moderate == true)
                        {
                            // Tell the user the topic is awaiting moderation
                            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Moderate.AwaitingModeration"),
                                MessageType = GenericMessages.info
                            };

                            return RedirectToAction("Index", "Home");
                        }
                    }

                    // Redirect to the newly created topic
                    return Redirect($"{topic.NiceUrl}?postbadges=true");
                }
            }

            return View(topicViewModel);
        }


        [Authorize]
        public ActionResult EditPostTopic(Guid id)
        {
            // Get the post
            var post = _postService.Get(id);

            // Get the topic
            var topic = post.Topic;

            // Get the current logged on user
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnloggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            // get the users permissions
            var permissions = RoleService.GetPermissions(topic.Category, loggedOnloggedOnUsersRole);

            // Is the user allowed to edit this post
            if (post.User.Id == loggedOnReadOnlyUser.Id ||
                permissions[ForumConfiguration.Instance.PermissionEditPosts].IsTicked)
            {
                // Get the allowed categories for this user
                var allowedAccessCategories = _categoryService.GetAllowedCategories(loggedOnloggedOnUsersRole);
                var allowedCreateTopicCategories =
                    _categoryService.GetAllowedCategories(loggedOnloggedOnUsersRole,
                        ForumConfiguration.Instance.PermissionCreateTopics);
                var allowedCreateTopicCategoryIds = allowedCreateTopicCategories.Select(x => x.Id);

                // If this user hasn't got any allowed cats OR they are not allowed to post then abandon
                if (allowedAccessCategories.Any() && loggedOnReadOnlyUser.DisablePosting != true)
                {
                    // Create the model for just the post
                    var viewModel = new CreateEditTopicViewModel
                    {
                        Content = post.PostContent,
                        Id = post.Id,
                        Category = topic.Category.Id,
                        Name = topic.Name,
                        TopicId = topic.Id,
                        OptionalPermissions = GetCheckCreateTopicPermissions(permissions)
                    };

                    // Now check if this is a topic starter, if so add the rest of the field
                    if (post.IsTopicStarter)
                    {
                        // Remove all Categories that don't have create topic permission
                        allowedAccessCategories.RemoveAll(x => allowedCreateTopicCategoryIds.Contains(x.Id));

                        // See if this user is subscribed to this topic
                        var topicNotifications =
                            _notificationService.GetTopicNotificationsByUserAndTopic(loggedOnReadOnlyUser, topic);

                        // Populate the properties we can
                        viewModel.IsLocked = topic.IsLocked;
                        viewModel.IsSticky = topic.IsSticky;
                        viewModel.IsTopicStarter = post.IsTopicStarter;
                        viewModel.SubscribeToTopic = topicNotifications.Any();
                        viewModel.Categories =
                            _categoryService.GetBaseSelectListCategories(allowedAccessCategories);

                        // Tags - Populate from the topic
                        if (topic.Tags.Any())
                        {
                            viewModel.Tags = string.Join<string>(",", topic.Tags.Select(x => x.Tag));
                        }

                        // Populate the poll answers
                        if (topic.Poll != null && topic.Poll.PollAnswers.Any())
                        {
                            // Has a poll so add it to the view model
                            viewModel.PollAnswers = topic.Poll.PollAnswers;
                            viewModel.PollCloseAfterDays = topic.Poll.ClosePollAfterDays ?? 0;
                        }
                    }

                    // Return the edit view
                    return View(viewModel);
                }
            }

            // If we get here the user has no permission to try and edit the post
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditPostTopic(CreateEditTopicViewModel editPostViewModel)
        {
            // Get the current user and role
            var loggedOnUser = User.GetMembershipUser(MembershipService, false);
            var loggedOnUsersRole = loggedOnUser.GetRole(RoleService, false);

            // Get the category
            var category = _categoryService.Get(editPostViewModel.Category);

            // Get all the permissions for this user
            var permissions = RoleService.GetPermissions(category, loggedOnUsersRole);

            // Now we have the category and permissionSet - Populate the optional permissions 
            // This is just in case the viewModel is return back to the view also sort the allowedCategories
            // Get the allowed categories for this user
            var allowedAccessCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);
            var allowedCreateTopicCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole,
                ForumConfiguration.Instance.PermissionCreateTopics);
            var allowedCreateTopicCategoryIds = allowedCreateTopicCategories.Select(x => x.Id);

            // TODO ??? Is this correct ??
            allowedAccessCategories.RemoveAll(x => allowedCreateTopicCategoryIds.Contains(x.Id));

            // Set the categories
            editPostViewModel.Categories = _categoryService.GetBaseSelectListCategories(allowedAccessCategories);

            // Get the users permissions for the topic
            editPostViewModel.OptionalPermissions = GetCheckCreateTopicPermissions(permissions);

            // See if this is a topic starter or not
            editPostViewModel.IsTopicStarter = editPostViewModel.Id == Guid.Empty;

            // IS the model valid
            if (ModelState.IsValid)
            {
                // Got to get a lot of things here as we have to check permissions
                // Get the post
                var originalPost = _postService.Get(editPostViewModel.Id);

                // Get the topic
                var originalTopic = originalPost.Topic;


                // See if the user has actually added some content to the topic
                if (string.IsNullOrWhiteSpace(editPostViewModel.Content))
                {
                    ModelState.AddModelError(string.Empty,
                        LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
                else
                {
                    // Map the new topic (Pass null for new topic)
                    var topic = editPostViewModel.ToTopic(category, loggedOnUser, originalTopic);

                    // Run the create pipeline
                    var editPipeLine = await _topicService.Edit(originalTopic, editPostViewModel.Files,
                        editPostViewModel.Tags, editPostViewModel.SubscribeToTopic, editPostViewModel.Content, 
                        editPostViewModel.Name, editPostViewModel.PollAnswers, editPostViewModel.PollCloseAfterDays);

                    // Check if successful
                    if (editPipeLine.Successful == false)
                    {
                        // Tell the user the topic is awaiting moderation
                        ModelState.AddModelError(string.Empty, editPipeLine.ProcessLog.FirstOrDefault());
                        return View(editPostViewModel);
                    }

                    var moderate = editPipeLine.ExtendedData[Constants.ExtendedDataKeys.Moderate] as bool?;
                    if (moderate == true)
                    {
                        // Tell the user the topic is awaiting moderation
                        TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Moderate.AwaitingModeration"),
                            MessageType = GenericMessages.info
                        };

                        return RedirectToAction("Index", "Home");
                    }

                    // Redirect to the newly created topic
                    return Redirect($"{topic.NiceUrl}?postbadges=true");
                }
            }


            //        // Is this user allowed to edit this post/topic
            //        if (originalPost.User.Id == loggedOnUser.Id ||
            //            permissions[ForumConfiguration.Instance.PermissionEditPosts].IsTicked)
            //        {

            //            // Is this topic or post awaiting moderations
            //            var topicPostInModeration = false;

            //            // Check stop words
            //            var stopWords = _bannedWordService.GetAll(true);
            //            foreach (var stopWord in stopWords)
            //            {
            //                if (editPostViewModel.Content.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
            //                    editPostViewModel.Name.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0)
            //                {
            //                    ShowMessage(new GenericMessageViewModel
            //                    {
            //                        Message = LocalizationService.GetResourceString("StopWord.Error"),
            //                        MessageType = GenericMessages.danger
            //                    });

            //                    var p = _postService.Get(editPostViewModel.Id);
            //                    var t = p.Topic;

            //                    // Ahhh found a stop word. Abandon operation captain.
            //                    return Redirect(t.NiceUrl);
            //                }
            //            }

            //            // Want the same edit date on both post and postedit
            //            var dateEdited = DateTime.UtcNow;

            //            // Create a post edit
            //            var postEdit = new PostEdit
            //            {
            //                Post = originalPost,
            //                DateEdited = dateEdited,
            //                EditedBy = loggedOnUser,
            //                OriginalPostContent = originalPost.PostContent,
            //                OriginalPostTitle = originalPost.IsTopicStarter ? originalTopic.Name : string.Empty
            //            };

            //            // User has permission so update the post
            //            originalPost.PostContent = _bannedWordService.SanitiseBannedWords(editPostViewModel.Content);
            //            originalPost.DateEdited = dateEdited;

            //            originalPost = _postService.SanitizePost(originalPost);

            //            // if topic starter update the topic
            //            if (originalPost.IsTopicStarter)
            //            {
            //                // Now save the post changes and the post edit
            //                Context.SaveChanges();

            //                // Sort the Tags
            //                if (!string.IsNullOrWhiteSpace(editPostViewModel.Tags))
            //                {
            //                    _topicTagService.Add(editPostViewModel.Tags.ToLower(), originalTopic, permissions[ForumConfiguration.Instance.PermissionCreateTags].IsTicked);
            //                }

            //                // Now save the tag changes
            //                Context.SaveChanges();

            //                // if category has changed then update it
            //                if (originalTopic.Category.Id != editPostViewModel.Category)
            //                {
            //                    var cat = _categoryService.Get(editPostViewModel.Category);
            //                    originalTopic.Category = cat;
            //                }
            //                originalTopic.IsLocked = editPostViewModel.IsLocked;
            //                originalTopic.IsSticky = editPostViewModel.IsSticky;
            //                originalTopic.Name = StringUtils.GetSafeHtml(_bannedWordService.SanitiseBannedWords(editPostViewModel.Name));

            //                // if the Category has moderation marked then the topic needs to 
            //                // go back into moderation
            //                if (originalTopic.Category.ModerateTopics == true)
            //                {
            //                    originalTopic.Pending = true;
            //                    topicPostInModeration = true;
            //                }

            //                // Now save the main topic content changes
            //                Context.SaveChanges();

            //                // See if there is a poll and can we edit/update it
            //                if (editPostViewModel.PollAnswers != null &&
            //                    editPostViewModel.PollAnswers.Count(x => !string.IsNullOrWhiteSpace(x?.Answer)) > 1 &&
            //                    permissions[ForumConfiguration.Instance.PermissionCreatePolls].IsTicked)
            //                {

            //                    // Now sort the poll answers, what to add and what to remove
            //                    // Poll answers already in this poll.
            //                    var newPollAnswerIds = editPostViewModel.PollAnswers.Where(x => !string.IsNullOrWhiteSpace(x?.Answer)).Select(x => x.Id);

            //                    // This post might not have a poll on it, if not they are creating a poll for the first time
            //                    var originalPollAnswerIds = new List<Guid>();
            //                    var pollAnswersToRemove = new List<PollAnswer>();
            //                    if (originalTopic.Poll == null)
            //                    {
            //                        // Create a new Poll
            //                        var newPoll = new Poll
            //                        {
            //                            User = loggedOnUser
            //                        };

            //                        // Create the poll
            //                        _pollService.Add(newPoll);

            //                        // Save the poll in the context so we can add answers
            //                        Context.SaveChanges();

            //                        // Add the poll to the topic
            //                        originalTopic.Poll = newPoll;
            //                    }
            //                    else
            //                    {
            //                        originalPollAnswerIds = originalTopic.Poll.PollAnswers.Select(p => p.Id).ToList();
            //                        pollAnswersToRemove = originalTopic.Poll.PollAnswers.Where(x => !newPollAnswerIds.Contains(x.Id)).ToList();
            //                    }

            //                    // Set the amount of days to close the poll
            //                    originalTopic.Poll.ClosePollAfterDays = editPostViewModel.PollCloseAfterDays;

            //                    // Get existing answers
            //                    var existingAnswers = editPostViewModel.PollAnswers.Where(x => !string.IsNullOrWhiteSpace(x.Answer) && originalPollAnswerIds.Contains(x.Id)).ToList();

            //                    // Get new poll answers to add
            //                    var newPollAnswers = editPostViewModel.PollAnswers.Where(x => !string.IsNullOrWhiteSpace(x.Answer) && !originalPollAnswerIds.Contains(x.Id)).ToList();

            //                    // Loop through existing and update names if need be
            //                    // If name changes remove the poll
            //                    foreach (var existPollAnswer in existingAnswers)
            //                    {
            //                        // Get the existing answer from the current topic
            //                        var pa = originalTopic.Poll.PollAnswers.FirstOrDefault(x => x.Id == existPollAnswer.Id);
            //                        if (pa != null && pa.Answer != existPollAnswer.Answer)
            //                        {
            //                            var pollVotestToRemove = new List<PollVote>();
            //                            pollVotestToRemove.AddRange(pa.PollVotes);
            //                            // Remove all the poll votes, as the answer has changed
            //                            foreach (var answerPollVote in pollVotestToRemove)
            //                            {
            //                                pa.PollVotes.Remove(answerPollVote);
            //                                _pollVoteService.Delete(answerPollVote);
            //                            }
            //                            pa.PollVotes.Clear();
            //                            Context.SaveChanges();

            //                            // If the answer has changed then update it
            //                            pa.Answer = existPollAnswer.Answer;
            //                        }
            //                    }

            //                    // Save existing
            //                    Context.SaveChanges();

            //                    // Loop through and remove the old poll answers and delete
            //                    foreach (var oldPollAnswer in pollAnswersToRemove)
            //                    {
            //                        // Clear poll votes if it's changed
            //                        var pollVotestToRemove = new List<PollVote>();
            //                        pollVotestToRemove.AddRange(oldPollAnswer.PollVotes);
            //                        foreach (var answerPollVote in pollVotestToRemove)
            //                        {
            //                            oldPollAnswer.PollVotes.Remove(answerPollVote);
            //                            _pollVoteService.Delete(answerPollVote);
            //                        }
            //                        oldPollAnswer.PollVotes.Clear();
            //                        Context.SaveChanges();

            //                        // Remove from Poll
            //                        originalTopic.Poll.PollAnswers.Remove(oldPollAnswer);

            //                        // Delete
            //                        _pollAnswerService.Delete(oldPollAnswer);
            //                    }

            //                    // Save removed
            //                    Context.SaveChanges();

            //                    // Poll answers to add
            //                    foreach (var newPollAnswer in newPollAnswers)
            //                    {
            //                        if (newPollAnswer != null)
            //                        {
            //                            var npa = new PollAnswer
            //                            {
            //                                Poll = originalTopic.Poll,
            //                                Answer = newPollAnswer.Answer
            //                            };
            //                            _pollAnswerService.Add(npa);
            //                            originalTopic.Poll.PollAnswers.Add(npa);
            //                        }
            //                    }
            //                }
            //                else if (originalTopic.Poll != null)
            //                {
            //                    // Remove from topic.
            //                    originalTopic.Poll = null;

            //                    // Now delete the poll
            //                    _pollService.Delete(originalTopic.Poll);
            //                }
            //            }
            //            else
            //            {
            //                // if the Category has moderation marked then the post needs to 
            //                // go back into moderation
            //                if (originalTopic.Category.ModeratePosts == true)
            //                {
            //                    originalPost.Pending = true;
            //                    topicPostInModeration = true;
            //                }
            //            }

            //            // Create a post edit
            //            postEdit.EditedPostTitle = originalTopic.Name;
            //            postEdit.EditedPostContent = originalPost.PostContent;

            //            // Add the post edit too
            //            _postEditService.Add(postEdit);

            //            // Commit the changes
            //            Context.SaveChanges();

            //            if (topicPostInModeration)
            //            {
            //                // If in moderation then let the user now
            //                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            //                {
            //                    Message = LocalizationService.GetResourceString("Moderate.AwaitingModeration"),
            //                    MessageType = GenericMessages.info
            //                };
            //            }
            //            else
            //            {
            //                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            //                {
            //                    Message = LocalizationService.GetResourceString("Post.Updated"),
            //                    MessageType = GenericMessages.success
            //                };
            //            }

            //            // redirect back to topic
            //            return Redirect($"{originalTopic.NiceUrl}?postbadges=true");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Context.RollBack();
            //        LoggingService.Error(ex);
            //        TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            //        {
            //            Message = LocalizationService.GetResourceString("Errors.GenericError"),
            //            MessageType = GenericMessages.danger
            //        };
            //    }


            //    return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            //}

            return View(editPostViewModel);
        }

        public async Task<ActionResult> Show(string slug, int? p)
        {
            // Set the page index
            var pageIndex = p ?? 1;

            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            // Get the topic
            var topic = _topicService.GetTopicBySlug(slug);

            if (topic != null)
            {
                var settings = SettingsService.GetSettings();

                // Note: Don't use topic.Posts as its not a very efficient SQL statement
                // Use the post service to get them as it includes other used entities in one
                // statement rather than loads of sql selects

                var sortQuerystring = Request.QueryString[Constants.PostOrderBy];
                var orderBy = !string.IsNullOrWhiteSpace(sortQuerystring)
                    ? EnumUtils.ReturnEnumValueFromString<PostOrderBy>(sortQuerystring)
                    : PostOrderBy.Standard;

                // Store the amount per page
                var amountPerPage = settings.PostsPerPage;

                if (sortQuerystring == Constants.AllPosts)
                {
                    // Overide to show all posts
                    amountPerPage = int.MaxValue;
                }

                // Get the posts
                var posts = await _postService.GetPagedPostsByTopic(pageIndex,
                    amountPerPage,
                    int.MaxValue,
                    topic.Id,
                    orderBy);

                // Get the topic starter post
                var starterPost = _postService.GetTopicStarterPost(topic.Id);

                // Get the permissions for the category that this topic is in
                var permissions = RoleService.GetPermissions(topic.Category, loggedOnUsersRole);

                // If this user doesn't have access to this topic then
                // redirect with message
                if (permissions[ForumConfiguration.Instance.PermissionDenyAccess].IsTicked)
                {
                    return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
                }

                // Set editor permissions
                ViewBag.ImageUploadType = permissions[ForumConfiguration.Instance.PermissionInsertEditorImages].IsTicked
                    ? "forumimageinsert"
                    : "image";

                var viewModel = ViewModelMapping.CreateTopicViewModel(topic, permissions, posts.ToList(),
                    starterPost, posts.PageIndex, posts.TotalCount, posts.TotalPages, loggedOnReadOnlyUser,
                    settings, _notificationService, _pollService, _voteService, _favouriteService, true);

                // If there is a quote querystring
                var quote = Request["quote"];
                if (!string.IsNullOrWhiteSpace(quote))
                {
                    try
                    {
                        // Got a quote
                        var postToQuote = _postService.Get(new Guid(quote));
                        viewModel.QuotedPost = postToQuote.PostContent;
                        viewModel.ReplyTo = postToQuote.Id;
                        viewModel.ReplyToUsername = postToQuote.User.UserName;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(ex);
                    }
                }

                var reply = Request["reply"];
                if (!string.IsNullOrWhiteSpace(reply))
                {
                    try
                    {
                        // Set the reply
                        var toReply = _postService.Get(new Guid(reply));
                        viewModel.ReplyTo = toReply.Id;
                        viewModel.ReplyToUsername = toReply.User.UserName;
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(ex);
                    }
                }

                var updateDatabase = false;

                // User has permission lets update the topic view count
                // but only if this topic doesn't belong to the user looking at it
                var addView = !(User.Identity.IsAuthenticated && loggedOnReadOnlyUser.Id == topic.User.Id);
                if (addView)
                {
                    updateDatabase = true;
                }

                // Check the poll - To see if it has one, and whether it needs to be closed.
                if (viewModel.Poll?.Poll?.ClosePollAfterDays != null &&
                    viewModel.Poll.Poll.ClosePollAfterDays > 0 &&
                    !viewModel.Poll.Poll.IsClosed)
                {
                    // Check the date the topic was created
                    var endDate =
                        viewModel.Poll.Poll.DateCreated.AddDays((int) viewModel.Poll.Poll.ClosePollAfterDays);
                    if (DateTime.UtcNow > endDate)
                    {
                        topic.Poll.IsClosed = true;
                        viewModel.Topic.Poll.IsClosed = true;
                        updateDatabase = true;
                    }
                }

                if (!BotUtils.UserIsBot() && updateDatabase)
                {
                    if (addView)
                    {
                        // Increase the topic views
                        topic.Views = topic.Views + 1;
                    }

                    try
                    {
                        Context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(ex);
                    }
                }

                return View(viewModel);
            }

            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        [HttpPost]
        public PartialViewResult AjaxMorePosts(GetMorePostsViewModel getMorePostsViewModel)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            // Get the topic
            var topic = _topicService.Get(getMorePostsViewModel.TopicId);
            var settings = SettingsService.GetSettings();

            // Get the permissions for the category that this topic is in
            var permissions = RoleService.GetPermissions(topic.Category, loggedOnUsersRole);

            // If this user doesn't have access to this topic then just return nothing
            if (permissions[ForumConfiguration.Instance.PermissionDenyAccess].IsTicked)
            {
                return null;
            }

            var orderBy = !string.IsNullOrWhiteSpace(getMorePostsViewModel.Order)
                ? EnumUtils.ReturnEnumValueFromString<PostOrderBy>(getMorePostsViewModel.Order)
                : PostOrderBy.Standard;

            var posts = Task.Run(() => _postService.GetPagedPostsByTopic(getMorePostsViewModel.PageIndex,
                settings.PostsPerPage, int.MaxValue, topic.Id, orderBy)).Result;
            var postIds = posts.Select(x => x.Id).ToList();
            var votes = _voteService.GetVotesByPosts(postIds);
            var favs = _favouriteService.GetAllPostFavourites(postIds);
            var viewModel = new ShowMorePostsViewModel
            {
                Posts = ViewModelMapping.CreatePostViewModels(posts, votes, permissions, topic,
                    loggedOnReadOnlyUser, settings, favs),
                Topic = topic,
                Permissions = permissions
            };

            return PartialView(viewModel);
        }

        public async Task<ActionResult> TopicsByTag(string tag, int? p)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);
            var settings = SettingsService.GetSettings();
            var tagModel = _topicTagService.Get(tag);

            // Set the page index
            var pageIndex = p ?? 1;

            // Get the topics
            var topics = await _topicService.GetPagedTopicsByTag(pageIndex,
                settings.TopicsPerPage,
                int.MaxValue,
                tag, allowedCategories);

            // See if the user has subscribed to this topic or not
            var isSubscribed = User.Identity.IsAuthenticated &&
                               _notificationService.GetTagNotificationsByUserAndTag(loggedOnReadOnlyUser, tagModel)
                                   .Any();

            // Get the Topic View Models
            var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics, RoleService, loggedOnUsersRole,
                loggedOnReadOnlyUser, allowedCategories, settings, _postService, _notificationService,
                _pollService, _voteService, _favouriteService);

            // create the view model
            var viewModel = new TagTopicsViewModel
            {
                Topics = topicViewModels,
                PageIndex = pageIndex,
                TotalCount = topics.TotalCount,
                TotalPages = topics.TotalPages,
                Tag = tag,
                IsSubscribed = isSubscribed,
                TagId = tagModel.Id
            };

            return View(viewModel);
        }

        [HttpPost]
        public PartialViewResult GetSimilarTopics(string searchTerm)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            // Returns the formatted string to search on
            var formattedSearchTerm = StringUtils.ReturnSearchString(searchTerm);
            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);
            IList<Topic> topics = null;
            try
            {
                var searchResults = _topicService.SearchTopics(ForumConfiguration.Instance.SimilarTopicsListSize,
                    formattedSearchTerm, allowedCategories);
                if (searchResults != null)
                {
                    topics = searchResults;
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
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);
            var settings = SettingsService.GetSettings();

            // Set the page index
            var pageIndex = p ?? 1;

            // Get the topics
            var topics = Task.Run(() => _topicService.GetRecentTopics(pageIndex,
                settings.TopicsPerPage,
                ForumConfiguration.Instance.ActiveTopicsListSize,
                allowedCategories)).Result;

            // Get the Topic View Models
            var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics, RoleService, loggedOnUsersRole,
                loggedOnReadOnlyUser, allowedCategories, settings, _postService, _notificationService,
                _pollService, _voteService, _favouriteService);

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

        [ChildActionOnly]
        public ActionResult HotTopics(DateTime? from, DateTime? to, int? amountToShow)
        {
            if (amountToShow == null)
            {
                amountToShow = 5;
            }

            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            var fromString = from != null ? Convert.ToDateTime(from).ToShortDateString() : null;
            var toString = to != null ? Convert.ToDateTime(to).ToShortDateString() : null;

            var cacheKey = string.Concat("HotTopics", loggedOnUsersRole.Id, fromString, toString, amountToShow);
            var viewModel = CacheService.Get<HotTopicsViewModel>(cacheKey);
            if (viewModel == null)
            {
                // Allowed Categories
                var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);

                // Get the topics
                var topics = _topicService.GetPopularTopics(from, to, allowedCategories, (int) amountToShow);

                // Get the Topic View Models
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService,
                    loggedOnUsersRole, loggedOnReadOnlyUser, allowedCategories, SettingsService.GetSettings(),
                    _postService, _notificationService, _pollService, _voteService, _favouriteService);

                // create the view model
                viewModel = new HotTopicsViewModel
                {
                    Topics = topicViewModels
                };
                CacheService.Set(cacheKey, viewModel, CacheTimes.TwoHours);
            }

            return PartialView(viewModel);
        }
    }
}