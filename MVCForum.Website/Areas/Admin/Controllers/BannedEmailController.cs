namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Web.ViewModels;
    using Web.ViewModels.Admin;

    [Authorize(Roles = Constants.AdminRoleName)]
    public class BannedEmailController : BaseAdminController
    {
        private readonly IBannedEmailService _bannedEmailService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="settingsService"> </param>
        /// <param name="bannedEmailService"></param>
        /// <param name="context"></param>
        public BannedEmailController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService,
            IBannedEmailService bannedEmailService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
            _bannedEmailService = bannedEmailService;
        }


        public async Task<ActionResult> Index(int? p, string search)
        {
            var pageIndex = p ?? 1;
            var allEmails = string.IsNullOrWhiteSpace(search)
                ? await _bannedEmailService.GetAllPaged(pageIndex, ForumConfiguration.Instance.AdminListPageSize)
                : await _bannedEmailService.GetAllPaged(search, pageIndex, ForumConfiguration.Instance.AdminListPageSize);

            var vieWModel = new BannedEmailListViewModel
            {
                Emails = allEmails,
                PageIndex = pageIndex,
                TotalCount = allEmails.TotalCount,
                Search = search
            };

            return View(vieWModel);
        }

        [HttpPost]
        public ActionResult Add(AddBannedEmailViewModel addBannedEmailViewModel)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(addBannedEmailViewModel.Email))
                {
                    var bannedEmail = new BannedEmail
                    {
                        Email = addBannedEmailViewModel.Email,
                        DateAdded = DateTime.UtcNow
                    };

                    _bannedEmailService.Add(bannedEmail);

                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Email added",
                        MessageType = GenericMessages.success
                    };

                    Context.SaveChanges();
                }
                else
                {
                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Please add an email address",
                        MessageType = GenericMessages.danger
                    };
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
                    MessageType = GenericMessages.danger
                };
            }

            return RedirectToAction("Index");
        }

        public ActionResult Delete(Guid id, int? p, string search)
        {
            try
            {
                var email = _bannedEmailService.Get(id);
                if (email == null)
                {
                    throw new ApplicationException("Cannot delete email - email does not exist");
                }

                _bannedEmailService.Delete(email);

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Email delete successfully",
                    MessageType = GenericMessages.success
                };
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = $"Delete failed: {ex.Message}",
                    MessageType = GenericMessages.danger
                };
            }

            return RedirectToAction("Index", new {p, search});
        }

        /// <summary>
        ///     Edit a resource key
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public void AjaxUpdateEmail(AjaxEditEmailViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    var emailToUpdate = _bannedEmailService.Get(viewModel.EmailId);
                    emailToUpdate.Email = viewModel.NewName;
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                }
            }
        }
    }
}