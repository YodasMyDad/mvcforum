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
    public class BannedWordController : BaseAdminController
    {
        private readonly IBannedWordService _bannedWordService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="settingsService"> </param>
        /// <param name="bannedWordService"></param>
        /// <param name="context"></param>
        public BannedWordController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService,
            IBannedWordService bannedWordService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
            _bannedWordService = bannedWordService;
        }


        public async Task<ActionResult> Index(int? p, string search)
        {
            var pageIndex = p ?? 1;
            var allEmails = string.IsNullOrWhiteSpace(search)
                ? await _bannedWordService.GetAllPaged(pageIndex, ForumConfiguration.Instance.AdminListPageSize)
                : await _bannedWordService.GetAllPaged(search, pageIndex, ForumConfiguration.Instance.AdminListPageSize);

            var viewModel = new BannedWordListViewModel
            {
                Words = allEmails,
                PageIndex = pageIndex,
                TotalCount = allEmails.TotalCount,
                Search = search
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Add(AddBannedWordViewModel addBannedEmailViewModel)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(addBannedEmailViewModel.Word))
                {
                    var bannedWord = new BannedWord
                    {
                        Word = addBannedEmailViewModel.Word,
                        DateAdded = DateTime.UtcNow,
                        IsStopWord = addBannedEmailViewModel.IsStopWord
                    };

                    _bannedWordService.Add(bannedWord);

                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Word added",
                        MessageType = GenericMessages.success
                    };

                    Context.SaveChanges();
                }
                else
                {
                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Please add a word",
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
                var word = _bannedWordService.Get(id);
                if (word == null)
                {
                    throw new ApplicationException("Cannot delete word - Word does not exist");
                }

                _bannedWordService.Delete(word);

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Word delete successfully",
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
        public void AjaxUpdateWord(AjaxEditWordViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    var wordToUpdate = _bannedWordService.Get(viewModel.WordId);
                    wordToUpdate.Word = viewModel.NewName;
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