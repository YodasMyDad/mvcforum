using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public partial class AdminCategoryController : BaseAdminController
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
                var categoryViewModel = new CreateCategoryViewModel { AllCategories = _categoryService.GetAll().ToList() };
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
                                               ModeratePosts = categoryViewModel.ModeratePosts,
                                               ModerateTopics = categoryViewModel.ModerateTopics,
                                               SortOrder = categoryViewModel.SortOrder,
                                               PageTitle = categoryViewModel.PageTitle,
                                               MetaDescription = categoryViewModel.MetaDesc
                                           };

                        

                        if (categoryViewModel.ParentCategory != null)
                        {
                            var parentCategory = _categoryService.Get(categoryViewModel.ParentCategory.Value);
                            category.ParentCategory = parentCategory;
                            SortPath(category, parentCategory);
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
            else
            {
                ModelState.AddModelError("", "There was an error creating the category");
            }

            return RedirectToAction("Index");
        }


        private EditCategoryViewModel CreateEditCategoryViewModel(Category category)
        {
            var categoryViewModel = new EditCategoryViewModel
            {
                Name = category.Name,
                Description = category.Description,
                IsLocked = category.IsLocked,
                ModeratePosts = category.ModeratePosts == true,
                ModerateTopics = category.ModerateTopics == true,
                SortOrder = category.SortOrder,
                Id = category.Id,
                PageTitle = category.PageTitle,
                MetaDesc = category.MetaDescription,
                ParentCategory = category.ParentCategory == null ? Guid.Empty : category.ParentCategory.Id,
                AllCategories = _categoryService.GetAll()
                    .Where(x => x.Id != category.Id)
                    .ToList()
            };
            return categoryViewModel;
        }

        public ActionResult EditCategory(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var category = _categoryService.Get(id);
                var categoryViewModel = CreateEditCategoryViewModel(category);

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
                        category.ModeratePosts = categoryViewModel.ModeratePosts;
                        category.ModerateTopics = categoryViewModel.ModerateTopics;
                        category.Name = categoryViewModel.Name;
                        category.SortOrder = categoryViewModel.SortOrder;
                        category.PageTitle = categoryViewModel.PageTitle;
                        category.MetaDescription = categoryViewModel.MetaDesc;

                        if (categoryViewModel.ParentCategory != null)
                        {
                            // Set the parent category
                            var parentCategory = _categoryService.Get(categoryViewModel.ParentCategory.Value);
                            category.ParentCategory = parentCategory;

                            // Append the path from the parent category
                            SortPath(category, parentCategory);
                        }
                        else
                        {
                            // Must access property (trigger lazy-loading) before we can set it to null (Entity Framework bug!!!)
                            var triggerEfLoad = category.ParentCategory;
                            category.ParentCategory = null;

                            // Also clear the path
                            category.Path = null;
                        }

                        _categoryService.UpdateSlugFromName(category);

                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                                                        {
                                                                            Message = "Category Updated",
                                                                            MessageType = GenericMessages.success
                                                                        };

                        categoryViewModel = CreateEditCategoryViewModel(category);

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

            return View(categoryViewModel);
        }

        private void SortPath(Category category, Category parentCategory)
        {
            // Append the path from the parent category
            var path = string.Empty;
            if (!string.IsNullOrEmpty(parentCategory.Path))
            {
                path = string.Concat(parentCategory.Path, ",", parentCategory.Id.ToString());
            }
            else
            {
                path = parentCategory.Id.ToString();
            }

            category.Path = path;
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

        public ActionResult SyncCategoryPaths()
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    // var all categories
                    var all = _categoryService.GetAll().ToList();

                    // Get all the categories
                    var maincategories = all.Where(x => x.ParentCategory == null).ToList();

                    // Get the sub categories
                    var subcategories = all.Where(x => x.ParentCategory != null).ToList();

                    // loop through the main categories and get all it's sub categories
                    foreach (var maincategory in maincategories)
                    {
                        // get a list of sub categories, from this category
                        var subCats = new List<Category>();
                        subCats = GetAllCategorySubCategories(maincategory, subcategories, subCats);

                        // Now loop through these subcategories and set the paths
                        var count = 1;
                        var prevCatId = string.Empty;
                        var prevPath = string.Empty;
                        foreach (var cat in subCats)
                        {
                            if (count == 1)
                            {
                                // If first count just set the parent category Id
                                cat.Path = maincategory.Id.ToString();
                            }
                            else
                            {
                                // If past one, then we use the previous category
                                if (string.IsNullOrEmpty(prevPath))
                                {
                                    cat.Path = prevCatId;
                                }
                                else
                                {
                                    cat.Path = string.Concat(prevPath, ",", prevCatId);
                                }

                            }
                            prevCatId = cat.Id.ToString();
                            prevPath = cat.Path;
                            count++;
                        }

                        // Save changes on each category
                        unitOfWork.SaveChanges();
                    }


                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Category Paths Synced",
                        MessageType = GenericMessages.success
                    };
                    unitOfWork.Commit();
                }
                catch (Exception)
                {
                    unitOfWork.Rollback();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Error syncing paths",
                        MessageType = GenericMessages.error
                    };                    
                }

                return RedirectToAction("Index");
            }
        }

        private List<Category> GetAllCategorySubCategories(Category parent, List<Category> allSubCategories, List<Category> subCats)
        {
            foreach (var cat in allSubCategories)
            {
                if (cat.ParentCategory.Id == parent.Id)
                {
                    subCats.Add(cat);
                    GetAllCategorySubCategories(cat, allSubCategories, subCats);
                }
            }
            return subCats;
        }
    }
}
