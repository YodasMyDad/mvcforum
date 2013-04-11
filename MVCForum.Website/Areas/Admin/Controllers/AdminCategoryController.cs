using System;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class AdminCategoryController : BaseAdminController
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoryController(ILoggingService loggingService,
                                        IUnitOfWorkManager unitOfWorkManager,
                                       IMembershipService membershipService,
                                       ILocalizationService localizationService,
                                       ICategoryService categoryService,
                                       ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _categoryService = categoryService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public PartialViewResult ListAllCategories(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new ListCategoriesViewModel
                                    {
                                        Categories = _categoryService.GetAllSubCategories(id)
                                    };
                return PartialView(viewModel);
            }
        }

        [ChildActionOnly]
        public PartialViewResult GetMainCategories()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new ListCategoriesViewModel
                                    {
                                        Categories = _categoryService.GetAll().OrderBy(x => x.SortOrder)
                                    };
                return PartialView(viewModel);
            }
        }

        [ChildActionOnly]
        public PartialViewResult CreateCategory()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var categoryViewModel = new CreateCategoryViewModel {AllCategories = _categoryService.GetAll().ToList()};
                return PartialView(categoryViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(CreateCategoryViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var category = new Category
                                           {
                                               Name = categoryViewModel.Name,
                                               Description = categoryViewModel.Description,
                                               IsLocked = categoryViewModel.IsLocked,
                                               SortOrder = categoryViewModel.SortOrder,
                                           };
                       
                        if (categoryViewModel.ParentCategory != null)
                        {
                            category.ParentCategory =
                                _categoryService.Get(categoryViewModel.ParentCategory.Value);
                        }

                        _categoryService.Add(category);

                        // We use temp data because we are doing a redirect
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                                                        {
                                                                            Message = "Category Created",
                                                                            MessageType =
                                                                                GenericMessages.success
                                                                        };
                        unitOfWork.Commit();
                    }
                    catch (Exception)
                    {
                        unitOfWork.Rollback();
                    }
                }
            }
            
            return RedirectToAction("Index");
        }
    

    public ActionResult EditCategory(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var category = _categoryService.Get(id);
                var categoryViewModel = new EditCategoryViewModel
                                            {
                                                Name = category.Name,
                                                Description = category.Description,
                                                IsLocked = category.IsLocked,
                                                SortOrder = category.SortOrder,
                                                Id = category.Id,
                                                DateCreated = category.DateCreated,
                                                NiceUrl = category.NiceUrl,
                                                ParentCategory = category.ParentCategory == null ? Guid.Empty : category.ParentCategory.Id,
                                                AllCategories = _categoryService.GetAll()
                                                    .Where(x => x.Id != category.Id)
                                                    .ToList(),
                                            };

                return View(categoryViewModel);
            }
        }

        [HttpPost]
        public ActionResult EditCategory(EditCategoryViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        var category = _categoryService.Get(categoryViewModel.Id);

                        category.Description = categoryViewModel.Description;
                        category.IsLocked = categoryViewModel.IsLocked;
                        category.Name = categoryViewModel.Name;
                        category.SortOrder = categoryViewModel.SortOrder;
                        
                        if (categoryViewModel.ParentCategory != null)
                        {
                            category.ParentCategory = _categoryService.Get(categoryViewModel.ParentCategory.Value);
                        }
                        _categoryService.UpdateSlugFromName(category);

                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                                                        {
                                                                            Message = "Category Updated",
                                                                            MessageType = GenericMessages.success
                                                                        };

                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Error(ex);
                        unitOfWork.Rollback();

                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "Category Update Failed",
                            MessageType = GenericMessages.error
                        };
                    }
                }
            }

            return View("PopupConfirm");
        }

        public ActionResult DeleteCategoryConfirmation(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var cat = _categoryService.Get(id);
                var subCats = _categoryService.GetAllSubCategories(id).ToList();
                var viewModel = new DeleteCategoryViewModel
                                    {
                                        Id = cat.Id,
                                        Category = cat,
                                        SubCategories = subCats
                                    };

            return View(viewModel);
            }
        }

        public ActionResult DeleteCategory(Guid id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var cat = _categoryService.Get(id);
                    _categoryService.Delete(cat);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                                                    {
                                                                        Message = "Category Deleted",
                                                                        MessageType = GenericMessages.success
                                                                    };
                    unitOfWork.Commit();
                }
                catch (Exception)
                {
                    unitOfWork.Rollback();
                }

                return RedirectToAction("Index");
            }
        }
    }
}
