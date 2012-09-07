using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    [Authorize]
    public class VoteController : BaseController
    {
        private readonly IPostService _postService;
        private readonly IVoteService _voteService;
        private readonly ITopicService _topicService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly IBadgeService _badgeService;

        public VoteController(ILoggingService loggingService, 
            IUnitOfWorkManager unitOfWorkManager, 
            IMembershipService membershipService, 
            ILocalizationService localizationService, 
            IRoleService roleService,
            IPostService postService, 
            IVoteService voteService, 
            ISettingsService settingsService, 
            ITopicService topicService, 
            IMembershipUserPointsService membershipUserPointsService,
            IBadgeService badgeService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _voteService = voteService;
            _topicService = topicService;
            _membershipUserPointsService = membershipUserPointsService;
            _badgeService = badgeService;
        }

        [HttpPost]
        public void VoteUpPost(VoteUpViewModel voteUpViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                // Quick check to see if user is locked out, when logged in
                if (LoggedOnUser.IsLockedOut | !LoggedOnUser.IsApproved)
                {
                    FormsAuthentication.SignOut();
                    throw new Exception(LocalizationService.GetResourceString("Errors.NoAccess"));
                }
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        // Firstly get the post
                        var post = _postService.Get(voteUpViewModel.Post);

                        // Now get the current user
                        var voter = LoggedOnUser;

                        // Also get the user that wrote the post
                        var postWriter = MembershipService.GetUser(post.User.Id);

                        // Check this user is not the post owner
                        if (voter.Id != postWriter.Id)
                        {
                            // Not the same person, now check they haven't voted on this post before
                            if (!post.Votes.Select(x => x.User.Id == LoggedOnUser.Id).Any())
                            {
                                // Good to go, no tinkering
                                //new post point count
                                var newPointTotal = (post.VoteCount + 1);

                                // Update the users points who wrote the post
                                _membershipUserPointsService.Add(new MembershipUserPoints
                                                                     {
                                                                         Points =
                                                                             SettingsService.GetSettings().
                                                                             PointsAddedPostiveVote,
                                                                         User = postWriter
                                                                     });

                                // Update the post with the new vote of the voter
                                var vote = new Vote { Post = post, User = voter, Amount = 1 };
                                _voteService.Add(vote);

                                // Update the post with the new points amount
                                post.VoteCount = newPointTotal;
                            }
                        }

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
        }

        [HttpPost]
        public void VoteDownPost(VoteDownViewModel voteDownViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                // Quick check to see if user is locked out, when logged in
                if (LoggedOnUser.IsLockedOut | !LoggedOnUser.IsApproved)
                {
                    FormsAuthentication.SignOut();
                    throw new Exception(LocalizationService.GetResourceString("Errors.NoAccess"));
                }

                // Firstly get the post
                var post = _postService.Get(voteDownViewModel.Post);

                // Now get the current user
                var voter = LoggedOnUser;

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {

                    // Also get the user that wrote the post
                    var postWriter = MembershipService.GetUser(post.User.Id);

                    // Check this user is not the post owner
                    if (voter.Id != postWriter.Id)
                    {
                        // Not the same person, now check they haven't voted on this post before
                        if (!post.Votes.Select(x => x.User.Id == LoggedOnUser.Id).Any())
                        {
                            // Good to go, no tinkering
                            //new post point count
                            var newPointTotal = (post.VoteCount - 1);

                            // Update the users points who wrote the post
                            _membershipUserPointsService.Add(new MembershipUserPoints { Points = SettingsService.GetSettings().PointsDeductedNagativeVote, User = postWriter });

                            // Update the post with the new vote of the voter
                            var vote = new Vote { Post = post, User = voter, Amount = -1 };
                            _voteService.Add(vote);

                            // Update the post with the new points amount
                            post.VoteCount = newPointTotal;

                        }
                    }

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
        }

        [HttpPost]
        public void MarkAsSolution(MarkAsSolutionViewModel markAsSolutionViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                // Quick check to see if user is locked out, when logged in
                if (LoggedOnUser.IsLockedOut | !LoggedOnUser.IsApproved)
                {
                    FormsAuthentication.SignOut();
                    throw new Exception(LocalizationService.GetResourceString("Errors.NoAccess"));
                }

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    // Firstly get the post
                    var post = _postService.Get(markAsSolutionViewModel.Post);

                    // Person who created the solution post
                    var solutionWriter = post.User;

                    // Get the post topic
                    var topic = post.Topic;

                    // Now get the current user
                    var marker = LoggedOnUser;
                    try
                    {
                        var solved = _topicService.SolveTopic(topic, post, marker, solutionWriter);
                   
                        if (solved)
                        {
                            unitOfWork.Commit();
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
        }
    }
}
