using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels.Mapping;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class BannedEmailController : BaseAdminController
    {

        public IActivityService _activityService { get; set; }
        private readonly IRoleService _roleService;
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly IPollService _pollService;
        private readonly IPollVoteService _pollVoteService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly IBannedEmailService _bannedEmailService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="roleService"> </param>
        /// <param name="settingsService"> </param>
        /// <param name="loggingService"> </param>
        /// <param name="postService"> </param>
        /// <param name="topicService"> </param>
        /// <param name="membershipUserPointsService"> </param>
        /// <param name="activityService"> </param>
        /// <param name="pollService"> </param>
        /// <param name="pollVoteService"> </param>
        /// <param name="pollAnswerService"> </param>
        /// <param name="bannedEmailService"></param>
        public BannedEmailController(ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ISettingsService settingsService, IPostService postService, ITopicService topicService, IMembershipUserPointsService membershipUserPointsService, 
            IActivityService activityService, IPollService pollService, IPollVoteService pollVoteService, IPollAnswerService pollAnswerService, IBannedEmailService bannedEmailService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _activityService = activityService;
            _roleService = roleService;
            _postService = postService;
            _topicService = topicService;
            _membershipUserPointsService = membershipUserPointsService;
            _pollService = pollService;
            _pollVoteService = pollVoteService;
            _pollAnswerService = pollAnswerService;
            _bannedEmailService = bannedEmailService;
        }


        public ActionResult Index(int? p, string search)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var pageIndex = p ?? 1;
                var allEmails = string.IsNullOrEmpty(search) ? _bannedEmailService.GetAllPaged(pageIndex, AppConstants.AdminListPageSize) :
                                    _bannedEmailService.GetAllPaged(search, pageIndex, AppConstants.AdminListPageSize);

                var vieWModel = new BannedEmailListViewModel
                    {
                        Emails = allEmails,
                        PageIndex = pageIndex,
                        TotalCount = allEmails.TotalCount,
                        Search = search
                    };
                
                return View(vieWModel);
            }
            
        }

        [HttpPost]
        public ActionResult Add(AddBannedEmailViewModel addBannedEmailViewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    if (!string.IsNullOrEmpty(addBannedEmailViewModel.Email))
                    {
                        var bannedEmail = new BannedEmail
                        {
                            Email = addBannedEmailViewModel.Email,
                            DateAdded = DateTime.Now
                        };

                        _bannedEmailService.Add(bannedEmail);

                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "Email added",
                            MessageType = GenericMessages.success
                        };

                        unitOfWork.Commit();
                    }
                    else
                    {
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "Please add an email address",
                            MessageType = GenericMessages.error
                        };
                    }

                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
                        MessageType = GenericMessages.error
                    }; 
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(Guid id, int? p, string search)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var email = _bannedEmailService.Get(id);
                    if (email == null)
                    {
                        throw new ApplicationException("Cannot delete email - email does not exist");
                    }

                    _bannedEmailService.Delete(email);

                    ViewBag.Message = new GenericMessageViewModel
                    {
                        Message = "Email delete successfully",
                        MessageType = GenericMessages.success
                    };
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ViewBag.Message = new GenericMessageViewModel
                    {
                        Message = string.Format("Delete failed: {0}", ex.Message),
                        MessageType = GenericMessages.error
                    };
                }

                return RedirectToAction("Index", new { p, search });
            }
        }

        /// <summary>
        /// Edit a resource key
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public void AjaxUpdateEmail(AjaxEditEmailViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var emailToUpdate = _bannedEmailService.Get(viewModel.EmailId);
                        emailToUpdate.Email = viewModel.NewName;
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                    }
                }
            }
        }
    }
}
