namespace MvcForum.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.General;
    using ViewModels;
    using ViewModels.Mapping;
    using ViewModels.Post;

    [Authorize]
    public partial class PostController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IPostEditService _postEditService;
        private readonly IPostService _postService;
        private readonly IReportService _reportService;
        private readonly ITopicService _topicService;
        private readonly IVoteService _voteService;

        public PostController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ITopicService topicService,
            IPostService postService, ISettingsService settingsService, ICategoryService categoryService,
            IReportService reportService, IVoteService voteService,
            IPostEditService postEditService, ICacheService cacheService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _topicService = topicService;
            _postService = postService;
            _categoryService = categoryService;
            _reportService = reportService;
            _voteService = voteService;
            _postEditService = postEditService;
        }

        [HttpPost]
        public virtual async Task<ActionResult> CreatePost(CreateAjaxPostViewModel post)
        {
            var topic = _topicService.Get(post.Topic);
            var loggedOnUser = User.GetMembershipUser(MembershipService, false);
            var loggedOnUsersRole = loggedOnUser.GetRole(RoleService);
            var permissions = RoleService.GetPermissions(topic.Category, loggedOnUsersRole);

            var postPipelineResult = await _postService.Create(post.PostContent, topic, loggedOnUser, null, false, post.InReplyTo);
            if (!postPipelineResult.Successful)
            {
                // TODO - This is shit. We need to return an object to process
                throw new Exception(postPipelineResult.ProcessLog.FirstOrDefault());
            }

            //Check for moderation
            if (postPipelineResult.EntityToProcess.Pending == true)
            {
                return PartialView("_PostModeration");
            }

            // Create the view model
            var viewModel = ViewModelMapping.CreatePostViewModel(postPipelineResult.EntityToProcess, new List<Vote>(), permissions, topic,
                loggedOnUser, SettingsService.GetSettings(), new List<Favourite>());

            // Return view
            return PartialView("_Post", viewModel);
        }

        [HttpPost]
        public virtual async Task<ActionResult> DeletePost(Guid id)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            // Got to get a lot of things here as we have to check permissions
            // Get the post
            var post = _postService.Get(id);
            var postId = post.Id;

            // get this so we know where to redirect after
            var isTopicStarter = post.IsTopicStarter;

            // Get the topic
            var topic = post.Topic;
            var topicUrl = topic.NiceUrl;

            // get the users permissions
            var permissions = RoleService.GetPermissions(topic.Category, loggedOnUsersRole);

            if (post.User.Id == loggedOnReadOnlyUser.Id ||
                permissions[ForumConfiguration.Instance.PermissionDeletePosts].IsTicked)
            {
                try
                {
                    // Delete post / topic
                    if (post.IsTopicStarter)
                    {
                        // Delete entire topic
                        var result = await _topicService.Delete(topic);
                        if (!result.Successful)
                        {
                            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = result.ProcessLog.FirstOrDefault(),
                                MessageType = GenericMessages.success
                            };

                            return Redirect(topic.NiceUrl);
                        }
                    }
                    else
                    {
                        // Deletes single post and associated data
                        var result = await _postService.Delete(post, false);
                        if (!result.Successful)
                        {
                            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = result.ProcessLog.FirstOrDefault(),
                                MessageType = GenericMessages.success
                            };

                            return Redirect(topic.NiceUrl);
                        }

                        // Remove in replyto's
                        var relatedPosts = _postService.GetReplyToPosts(postId);
                        foreach (var relatedPost in relatedPosts)
                        {
                            relatedPost.InReplyTo = null;
                        }
                    }

                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
                        MessageType = GenericMessages.danger
                    });
                    return Redirect(topicUrl);
                }
            }

            // Deleted successfully
            if (isTopicStarter)
            {
                // Redirect to root as this was a topic and deleted
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Topic.Deleted"),
                    MessageType = GenericMessages.success
                };
                return RedirectToAction("Index", "Home");
            }

            // Show message that post is deleted
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = LocalizationService.GetResourceString("Post.Deleted"),
                MessageType = GenericMessages.success
            };

            return Redirect(topic.NiceUrl);
        }

        private ActionResult NoPermission(Topic topic)
        {
            // Trying to be a sneaky mo fo, so tell them
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = LocalizationService.GetResourceString("Errors.NoPermission"),
                MessageType = GenericMessages.danger
            };
            return Redirect(topic.NiceUrl);
        }

        public virtual ActionResult Report(Guid id)
        {
            if (SettingsService.GetSettings().EnableSpamReporting)
            {
                var post = _postService.Get(id);
                return View(new ReportPostViewModel { PostId = post.Id, PostCreatorUsername = post.User.UserName });
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        [HttpPost]
        public virtual ActionResult Report(ReportPostViewModel viewModel)
        {
            if (SettingsService.GetSettings().EnableSpamReporting)
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);

                var post = _postService.Get(viewModel.PostId);
                var report = new Report
                {
                    Reason = viewModel.Reason,
                    ReportedPost = post,
                    Reporter = loggedOnReadOnlyUser
                };
                _reportService.PostReport(report);

                try
                {
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                }

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Report.ReportSent"),
                    MessageType = GenericMessages.success
                };
                return View(new ReportPostViewModel { PostId = post.Id, PostCreatorUsername = post.User.UserName });
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }


        [HttpPost]
        [AllowAnonymous]
        public virtual ActionResult GetAllPostLikes(Guid id)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            var post = _postService.Get(id);
            var permissions = RoleService.GetPermissions(post.Topic.Category, loggedOnUsersRole);
            var votes = _voteService.GetVotesByPosts(new List<Guid> { id });
            var viewModel = ViewModelMapping.CreatePostViewModel(post, votes[id], permissions, post.Topic,
                loggedOnReadOnlyUser, SettingsService.GetSettings(), new List<Favourite>());
            var upVotes = viewModel.Votes.Where(x => x.Amount > 0).ToList();
            return View(upVotes);
        }


        public virtual ActionResult MovePost(Guid id)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            // Firstly check if this is a post and they are allowed to move it
            var post = _postService.Get(id);
            if (post == null)
            {
                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }

            var permissions = RoleService.GetPermissions(post.Topic.Category, loggedOnUsersRole);
            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);

            // Does the user have permission to this posts category
            var cat = allowedCategories.FirstOrDefault(x => x.Id == post.Topic.Category.Id);
            if (cat == null)
            {
                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }

            // Does this user have permission to move
            if (!permissions[ForumConfiguration.Instance.PermissionEditPosts].IsTicked)
            {
                return NoPermission(post.Topic);
            }

            var topics = _topicService.GetAllSelectList(allowedCategories, 30);
            topics.Insert(0, new SelectListItem
            {
                Text = LocalizationService.GetResourceString("Topic.Choose"),
                Value = ""
            });

            var postViewModel = ViewModelMapping.CreatePostViewModel(post, post.Votes.ToList(), permissions, post.Topic,
                loggedOnReadOnlyUser, SettingsService.GetSettings(), post.Favourites.ToList());
            postViewModel.MinimalPost = true;
            var viewModel = new MovePostViewModel
            {
                Post = postViewModel,
                PostId = post.Id,
                LatestTopics = topics,
                MoveReplyToPosts = true
            };
            return View(viewModel);
        }

        [HttpPost]
        public virtual async Task<ActionResult> MovePost(MovePostViewModel viewModel)
        {
            // Firstly check if this is a post and they are allowed to move it
            var post = _postService.Get(viewModel.PostId);
            if (post == null)
            {
                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }

            var moveResult = await _postService.Move(post, viewModel.TopicId, viewModel.TopicTitle,
                viewModel.MoveReplyToPosts);
            if (moveResult.Successful)
            {
                // On Update redirect to the topic
                return RedirectToAction("Show", "Topic", new { slug = moveResult.EntityToProcess.Topic.Slug });
            }

            // Add a model error to show issue
            ModelState.AddModelError("", moveResult.ProcessLog.FirstOrDefault());

            // Sort the view model before sending back
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            var permissions = RoleService.GetPermissions(post.Topic.Category, loggedOnUsersRole);
            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);

            // Repopulate the topics
            var topics = _topicService.GetAllSelectList(allowedCategories, 30);
            topics.Insert(0, new SelectListItem
            {
                Text = LocalizationService.GetResourceString("Topic.Choose"),
                Value = ""
            });

            viewModel.LatestTopics = topics;
            viewModel.Post = ViewModelMapping.CreatePostViewModel(post, post.Votes.ToList(), permissions, 
                            post.Topic, loggedOnReadOnlyUser, SettingsService.GetSettings(), post.Favourites.ToList());
            viewModel.Post.MinimalPost = true;
            viewModel.PostId = post.Id;

            return View(viewModel);
        }

        public virtual ActionResult GetPostEditHistory(Guid id)
        {
            var post = _postService.Get(id);
            if (post != null)
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                // Check permissions
                var permissions = RoleService.GetPermissions(post.Topic.Category, loggedOnUsersRole);
                if (permissions[ForumConfiguration.Instance.PermissionEditPosts].IsTicked)
                {
                    // Good to go
                    var postEdits = _postEditService.GetByPost(id);
                    var viewModel = new PostEditHistoryViewModel
                    {
                        PostEdits = postEdits
                    };
                    return PartialView(viewModel);
                }
            }

            return Content(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }
    }
}