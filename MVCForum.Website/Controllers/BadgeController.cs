using System;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public partial class BadgeController : BaseController
    {
        private readonly IBadgeService _badgeService;
        private readonly IPostService _postService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="badgeService"> </param>
        /// <param name="loggingService"> </param>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="postService"> </param>
        /// <param name="membershipService"> </param>
        /// <param name="localizationService"></param>
        /// <param name="roleService"> </param>
        /// <param name="settingsService"> </param>
        public BadgeController(ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IBadgeService badgeService,
            IPostService postService,
            IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService,
            ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _badgeService = badgeService;
            _postService = postService;
        }


        [HttpPost]
        [Authorize]
        public void VoteUpPost(VoteBadgeViewModel voteUpBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var databaseUpdateNeededOne = _badgeService.ProcessBadge(BadgeType.VoteUp, LoggedOnUser);
                    if (databaseUpdateNeededOne)
                    {
                        unitOfwork.SaveChanges();
                    }

                    var post = _postService.Get(voteUpBadgeViewModel.PostId);
                    var databaseUpdateNeededTwo = _badgeService.ProcessBadge(BadgeType.VoteUp, post.User);
                    if (databaseUpdateNeededTwo)
                    {
                        unitOfwork.SaveChanges();
                    }

                    if (databaseUpdateNeededOne || databaseUpdateNeededTwo)
                    {
                        unitOfwork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    unitOfwork.Rollback();
                    LoggingService.Error(ex);
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void VoteDownPost(VoteBadgeViewModel voteUpBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var databaseUpdateNeededOne = _badgeService.ProcessBadge(BadgeType.VoteDown, LoggedOnUser);
                    if (databaseUpdateNeededOne)
                    {
                        unitOfwork.SaveChanges();
                    }

                    var post = _postService.Get(voteUpBadgeViewModel.PostId);
                    var databaseUpdateNeededTwo = _badgeService.ProcessBadge(BadgeType.VoteDown, post.User);

                    if (databaseUpdateNeededTwo)
                    {
                        unitOfwork.SaveChanges();
                    }

                    if (databaseUpdateNeededOne || databaseUpdateNeededTwo)
                    {
                        unitOfwork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    unitOfwork.Rollback();
                    LoggingService.Error(ex);
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void Post()
        {
            if (Request.IsAjaxRequest())
            {
                using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.Post, LoggedOnUser);

                        if (databaseUpdateNeeded)
                        {
                            unitOfwork.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        unitOfwork.Rollback();
                        LoggingService.Error(ex);
                    }
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void MarkAsSolution(MarkAsSolutionBadgeViewModel markAsSolutionBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var post = _postService.Get(markAsSolutionBadgeViewModel.PostId);
                    var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.MarkAsSolution, post.User) | _badgeService.ProcessBadge(BadgeType.MarkAsSolution, post.Topic.User);

                    if (databaseUpdateNeeded)
                    {
                        unitOfwork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    unitOfwork.Rollback();
                    LoggingService.Error(ex);
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void ProfileBadgeCheck()
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    if (LoggedOnUser != null)
                    {
                        var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.Profile, LoggedOnUser);

                        if (databaseUpdateNeeded)
                        {
                            unitOfwork.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    unitOfwork.Rollback();
                    LoggingService.Error(ex);
                }
            }
        }

        [HttpPost]
        public void Time(TimeBadgeViewModel timeBadgeViewModel)
        {
            using (var unitOfwork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var user = MembershipService.GetUser(timeBadgeViewModel.Id);
                    var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.Time, user);

                    if (databaseUpdateNeeded)
                    {
                        unitOfwork.Commit();
                    }

                }
                catch (Exception ex)
                {
                    unitOfwork.Rollback();
                    LoggingService.Error(ex);
                }
            }
        }

        public ActionResult AllBadges()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var allBadges = _badgeService.GetallBadges();

                var badgesListModel = new AllBadgesViewModel
                {
                    AllBadges = allBadges
                };

                return View(badgesListModel);
            }
        }

    }
}
