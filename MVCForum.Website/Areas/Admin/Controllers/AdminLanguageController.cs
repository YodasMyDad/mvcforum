namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Utilities;
    using Web.ViewModels;
    using Web.ViewModels.Admin;

    public class AdminLanguageController : BaseAdminController
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="membershipService"> </param>
        /// <param name="localizationService"></param>
        /// <param name="settingsService"> </param>
        /// <param name="context"></param>
        public AdminLanguageController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
        }

        //
        // GET: /Admin/AdminLanguage/

        /// <summary>
        ///     We get here via the admin default layout (_AdminLayout). The returned view is displayed by
        ///     the @RenderBody in that layout
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        ///     Returns a partial view listing all languages
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public PartialViewResult GetLanguages()
        {
            var viewModel = new ListLanguagesViewModel {Languages = new List<LanguageDisplayViewModel>()};

            try
            {
                foreach (var language in LocalizationService.AllLanguages)
                {
                    var languageViewModel = new LanguageDisplayViewModel
                    {
                        Id = language.Id,
                        IsDefault =
                            language.Id == LocalizationService.CurrentLanguage.Id,
                        Name = language.Name,
                        LanguageCulture = language.LanguageCulture
                    };

                    viewModel.Languages.Add(languageViewModel);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                LoggingService.Error(ex);
            }

            return PartialView(viewModel);
        }

        /// <summary>
        ///     Confirmation prompt when a user requests to delete a message
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult DeleteLanguageConfirmation(Guid id)
        {
            try
            {
                var language = LocalizationService.Get(id);
                var languageViewModel = new LanguageDisplayViewModel
                {
                    Id = language.Id,
                    IsDefault =
                        language.Id == LocalizationService.CurrentLanguage.Id,
                    Name = language.Name,
                    LanguageCulture = language.LanguageCulture
                };

                return View(languageViewModel);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                LoggingService.Error("Delete confirmation not working");
                return View("Index");
            }
        }

        /// <summary>
        ///     Request to delete a language (e.g. confirming on prompt)
        /// </summary>
        /// <param name="buttonYes"></param>
        /// <param name="buttonNo"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult DeleteLanguage(string buttonYes, string buttonNo, Guid id)
        {
            if (buttonYes != null)
            {
                try
                {
                    LocalizationService.Delete(LocalizationService.Get(id));
                    Context.SaveChanges();
                    ShowSuccess("Language Deleted");
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    ShowError(ex.Message);
                    LoggingService.Error(ex);
                }
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        ///     Request to delete a locale resource
        /// </summary>
        /// <param name="resourceKeyId"></param>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult DeleteResourceConfirmation(Guid resourceKeyId)
        {
            try
            {
                var resourceKey = LocalizationService.GetResourceKey(resourceKeyId);
                var viewModel = new LocaleResourceKeyViewModel
                {
                    Id = resourceKey.Id,
                    Name = resourceKey.Name,
                    Notes = resourceKey.Notes,
                    DateAdded = resourceKey.DateAdded
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                LoggingService.Error(ex);
                return View("Index");
            }
        }

        /// <summary>
        ///     Delete resource confirmation result
        /// </summary>
        /// <param name="buttonYes"></param>
        /// <param name="buttonNo"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult DeleteResource(string buttonYes, string buttonNo, Guid id)
        {
            if (buttonYes != null)
            {
                try
                {
                    var resourceKey = LocalizationService.GetResourceKey(id);
                    LocalizationService.DeleteLocaleResourceKey(resourceKey);
                    Context.SaveChanges();
                    ShowSuccess("Resource Deleted");
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    ShowError(ex.Message);
                }
            }

            return RedirectToAction("ManageResourceKeys");
        }

        /// <summary>
        ///     Get - create a new language
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        [Authorize(Roles = Constants.AdminRoleName)]
        public PartialViewResult CreateLanguage()
        {
            return PartialView();
        }

        /// <summary>
        ///     Post - create a new language
        /// </summary>
        /// <param name="languageViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult CreateLanguage(CreateLanguageViewModel languageViewModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get the culture info
                    var cultureInfo = LanguageUtils.GetCulture(languageViewModel.Name);


                    try
                    {
                        LocalizationService.Add(cultureInfo);
                        Context.SaveChanges();
                        ShowSuccess("Language Created");
                    }
                    catch (Exception ex)
                    {
                        Context.RollBack();
                        LoggingService.Error(ex);
                        throw;
                    }
                }
                else
                {
                    var errors = (from key in ModelState.Keys
                        select ModelState[key]
                        into state
                        where state.Errors.Any()
                        select state.Errors.First().ErrorMessage).ToList();
                    ShowErrors(errors);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            // Default ie error
            return RedirectToAction("Index");
        }

        /// <summary>
        ///     Manage resource values for a language
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        public async Task<ActionResult> ManageLanguageResourceValues(Guid languageId, int? p, string search)
        {
            return await GetLanguageResources(false, languageId, p, search);
        }

        /// <summary>
        ///     Manage resource values for a language
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        public async Task<ActionResult> ManageLanguageResourceKeys(Guid languageId, int? p, string search)
        {
            return await GetLanguageResources(true, languageId, p, search);
        }

        /// <summary>
        ///     Manage resource keys (for all languages)
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        public async Task<ActionResult> ManageResourceKeys(int? p, string search)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = (from key in ModelState.Keys
                        select ModelState[key]
                        into state
                        where state.Errors.Any()
                        select state.Errors.First().ErrorMessage).ToList();
                    ShowErrors(errors);
                }

                return await ListResourceKeys(p, search);
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        ///     Edit a resource
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public void UpdateResourceValue(AjaxEditLanguageValueViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    LocalizationService.UpdateResourceString(viewModel.LanguageId, viewModel.ResourceKey,
                        viewModel.NewValue);
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                }
            }
        }

        /// <summary>
        ///     Edit a resource key
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public void UpdateResourceKey(AjaxEditLanguageKeyViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    LocalizationService.UpdateResourceKey(viewModel.ResourceKeyId, viewModel.NewName);
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                }
            }
        }

        /// <summary>
        ///     Edit a resource in all languages
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult EditAll(Guid resourceKeyId)
        {
            try
            {
                var resourceKey = LocalizationService.GetResourceKey(resourceKeyId);
                var localeResourceKeyViewModel = new LocaleResourceKeyViewModel
                {
                    Id = resourceKey.Id,
                    Name = resourceKey.Name,
                    Notes = resourceKey.Notes,
                    DateAdded = resourceKey.DateAdded
                };

                var viewModel = new AllResourceValuesViewModel
                {
                    ResourceKey = localeResourceKeyViewModel,
                    ResourceValues =
                        new Dictionary<LanguageDisplayViewModel, LocaleResourceViewModel>()
                };

                foreach (var localeStringResource in LocalizationService.GetAllValuesForKey(resourceKeyId))
                {
                    var stringResourceViewModel = new LocaleResourceViewModel
                    {
                        Id = localeStringResource.Id,
                        ResourceKeyId = localeStringResource.LocaleResourceKey.Id,
                        LocaleResourceKey = localeStringResource.LocaleResourceKey.Name,
                        ResourceValue = localeStringResource.ResourceValue
                    };

                    var languageViewModel = new LanguageDisplayViewModel
                    {
                        Id = localeStringResource.Language.Id,
                        IsDefault = localeStringResource.Language.Id == LocalizationService.CurrentLanguage.Id,
                        Name = localeStringResource.Language.Name,
                        LanguageCulture = localeStringResource.Language.LanguageCulture
                    };

                    if (!viewModel.ResourceValues.ContainsKey(languageViewModel))
                    {
                        viewModel.ResourceValues.Add(languageViewModel, stringResourceViewModel);
                    }
                    else
                    {
                        viewModel.ResourceValues[languageViewModel] = stringResourceViewModel;
                    }
                }

                //Context.SaveChanges();
                return View("ListAllValues", viewModel);
            }
            catch (Exception ex)
            {
                //Context.RollBack();
                ShowError(ex.Message);
                LoggingService.Error(ex);
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        ///     Add a new resource
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult AddResourceKey()
        {
            var resourceKey = LocalizationService.CreateEmptyLocaleResourceKey();
            var viewModel = new LocaleResourceKeyViewModel
            {
                Id = resourceKey.Id,
                Name = resourceKey.Name,
                Notes = resourceKey.Notes,
                DateAdded = resourceKey.DateAdded
            };

            return View(viewModel);
        }

        //
        // POST /Account/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult AddResourceKey(LocaleResourceKeyViewModel newResourceKeyViewModel)
        {
            try
            {
                var resourceKeyToSave = new LocaleResourceKey
                {
                    Name = newResourceKeyViewModel.Name,
                    Notes = newResourceKeyViewModel.Notes,
                    DateAdded = newResourceKeyViewModel.DateAdded
                };

                LocalizationService.Add(resourceKeyToSave);
                Context.SaveChanges();
                ShowSuccess("Resource key created successfully");
                var currentLanguage = SettingsService.GetSettings().DefaultLanguage.Id;
                return RedirectToAction("ManageLanguageResourceValues", new {languageId = currentLanguage});
            }
            catch (Exception ex)
            {
                Context.RollBack();
                ShowError(ex.Message);
                LoggingService.Error(ex);
                return RedirectToAction("AddResourceKey");
            }
        }

        #region Private methods

        /// <summary>
        ///     Create a message to be displayed when some action is successful
        /// </summary>
        /// <param name="message"></param>
        private void ShowSuccess(string message)
        {
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = message,
                MessageType = GenericMessages.success
            };
        }

        /// <summary>
        ///     Create a message to be displayed as an error
        /// </summary>
        /// <param name="message"></param>
        private void ShowError(string message)
        {
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = message,
                MessageType = GenericMessages.danger
            };
        }

        /// <summary>
        ///     Create a message to be displayed as an error from
        ///     a set of messages
        /// </summary>
        /// <param name="messages"></param>
        private void ShowErrors(IEnumerable<string> messages)
        {
            var errors = new StringBuilder();

            foreach (var message in messages)
            {
                errors.AppendLine(message);
            }

            ShowError(errors.ToString());
        }

        /// <summary>
        ///     List out resource keys and allow editing
        /// </summary>
        /// <returns></returns>
        private async Task<ActionResult> ListResourceKeys(int? page, string search)
        {
            var pageIndex = page ?? 1;
            var allResources = string.IsNullOrWhiteSpace(search)
                ? await LocalizationService.GetAllResourceKeys(pageIndex, ForumConfiguration.Instance.AdminListPageSize)
                : await LocalizationService.SearchResourceKeys(search, pageIndex,
                    ForumConfiguration.Instance.AdminListPageSize);

            // Redisplay list of resources
            var allViewModelResourceKeys = allResources.Select(resource => new LocaleResourceKeyViewModel
            {
                Id = resource.Id,
                Name = resource.Name,
                Notes = resource.Notes,
                DateAdded = resource.DateAdded
            }).ToList();

            var resourceListModel = new ResourceKeyListViewModel
            {
                ResourceKeys = allViewModelResourceKeys,
                PageIndex = pageIndex,
                TotalCount = allResources.TotalCount,
                Search = search,
                TotalPages = allResources.TotalPages
            };

            return View("ListKeys", resourceListModel);
        }

        /// <summary>
        ///     Search through all resources for a language by page and search terms.
        ///     Search either by key or value.
        /// </summary>
        /// <param name="searchByKey">True means serach the keys else search the values</param>
        /// <param name="languageId"></param>
        /// <param name="p"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        private async Task<ActionResult> GetLanguageResources(bool searchByKey, Guid languageId, int? p, string search)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = (from key in ModelState.Keys
                        select ModelState[key]
                        into state
                        where state.Errors.Any()
                        select state.Errors.First().ErrorMessage).ToList();
                    ShowErrors(errors);
                }
                else
                {
                    var language = LocalizationService.Get(languageId);
                    var pageIndex = p ?? 1;

                    // Get all the resources or just the ones that match the search
                    var allResources = string.IsNullOrWhiteSpace(search)
                        ? await LocalizationService.GetAllValues(language.Id, pageIndex,
                            ForumConfiguration.Instance.AdminListPageSize)
                        : searchByKey
                            ? await LocalizationService.SearchResourceKeys(language.Id, search,
                                pageIndex,
                                ForumConfiguration.Instance.AdminListPageSize)
                            : await LocalizationService.SearchResourceValues(language.Id, search,
                                pageIndex,
                                ForumConfiguration.Instance.AdminListPageSize);

                    var models = allResources.Select(resource => new LocaleResourceViewModel
                    {
                        Id = resource.Id,
                        ResourceKeyId = resource.LocaleResourceKey.Id,
                        LocaleResourceKey = resource.LocaleResourceKey.Name,
                        ResourceValue = resource.ResourceValue
                    }).ToList();

                    var resourceListModel = new LanguageListResourcesViewModel
                    {
                        LanguageId = language.Id,
                        LanguageName = language.Name,
                        LocaleResources = models,
                        PageIndex = pageIndex,
                        TotalCount = allResources.TotalCount,
                        Search = search,
                        TotalPages = allResources.TotalPages
                    };

                    return View("ListValues", resourceListModel);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }

            // Default ie error
            return RedirectToAction("Index");
        }

        #endregion
    }
}