using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class BatchController : BaseAdminController
    {
        public BatchController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, ISettingsService settingsService) 
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult BatchDeleteMembers()
        {
            return View(new BatchDeleteMembersViewModel{AmoutOfDaysSinceRegistered = 0, AmoutOfPosts = 0});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BatchDeleteMembers(BatchDeleteMembersViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var membersToDelete = MembershipService.GetUsersByDaysPostsPoints(viewModel.AmoutOfDaysSinceRegistered,
                                                                                        viewModel.AmoutOfPosts);
                    var count = membersToDelete.Count;
                    foreach (var membershipUser in membersToDelete)
                    {
                        MembershipService.Delete(membershipUser);
                    }
                    unitOfWork.Commit();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = string.Format("{0} members deleted", count),
                        MessageType = GenericMessages.success
                    };
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = ex.Message,
                        MessageType = GenericMessages.error
                    };
                }
            }

            return View();
        }

        public ActionResult BatchMoveTopics()
        {
            return View();
        }

    }
}
