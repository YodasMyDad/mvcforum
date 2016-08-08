using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;
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

        public ActionResult CreateCategory()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var categoryViewModel = new CategoryViewModel { AllCategories = _categoryService.GetBaseSelectListCategories(_categoryService.GetAll()) };
                return View(categoryViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(CategoryViewModel categoryViewModel)
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
                                               MetaDescription = categoryViewModel.MetaDesc,
                                               Colour = categoryViewModel.CategoryColour
                                           };

                        // Sort image out first
                        if (categoryViewModel.Files != null)
                        {
                            // Before we save anything, check the user already has an upload folder and if not create one
                            var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, category.Id));
                            if (!Directory.Exists(uploadFolderPath))
                            {
                                Directory.CreateDirectory(uploadFolderPath);
                            }

                            // Loop through each file and get the file info and save to the users folder and Db
                            var file = categoryViewModel.Files[0];
                            if (file != null)
                            {
                                // If successful then upload the file
                                var uploadResult = AppHelpers.UploadFile(file, uploadFolderPath, LocalizationService, true);

                                if (!uploadResult.UploadSuccessful)
                                {
                                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                    {
                                        Message = uploadResult.ErrorMessage,
                                        MessageType = GenericMessages.danger
                                    };
                                    return View(categoryViewModel);
                                }

                                // Save avatar to user
                                category.Image = uploadResult.UploadedFileName;
                            }

                        }

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


        private CategoryViewModel CreateEditCategoryViewModel(Category category)
        {
            var categoryViewModel = new CategoryViewModel
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
                Image = category.Image,
                CategoryColour = category.Colour,
                ParentCategory = category.ParentCategory == null ? Guid.Empty : category.ParentCategory.Id,
                AllCategories = _categoryService.GetBaseSelectListCategories(_categoryService.GetAll()
                    .Where(x => x.Id != category.Id)
                    .ToList())
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
        public ActionResult EditCategory(CategoryViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {

                        var category = _categoryService.Get(categoryViewModel.Id);
                        var parentCat = categoryViewModel.ParentCategory != null
                                            ? _categoryService.Get((Guid)categoryViewModel.ParentCategory.Value)
                                            : null;

                        // Check they are not trying to add a subcategory of this category as the parent or it will break
                        if (parentCat?.Path != null && categoryViewModel.ParentCategory != null)
                        {
                            var parentCats = parentCat.Path.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(x => new Guid(x)).ToList();
                            if (parentCats.Contains(categoryViewModel.Id))
                            {
                                // Remove the parent category, but still let them create the catgory
                                categoryViewModel.ParentCategory = null;
                            }
                        }

                        // Sort image out first
                        if (categoryViewModel.Files != null)
                        {
                            // Before we save anything, check the user already has an upload folder and if not create one
                            var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath, categoryViewModel.Id));
                            if (!Directory.Exists(uploadFolderPath))
                            {
                                Directory.CreateDirectory(uploadFolderPath);
                            }

                            // Loop through each file and get the file info and save to the users folder and Db
                            var file = categoryViewModel.Files[0];
                            if (file != null)
                            {
                                // If successful then upload the file
                                var uploadResult = AppHelpers.UploadFile(file, uploadFolderPath, LocalizationService, true);

                                if (!uploadResult.UploadSuccessful)
                                {
                                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                    {
                                        Message = uploadResult.ErrorMessage,
                                        MessageType = GenericMessages.danger
                                    };
                                    return View(categoryViewModel);
                                }

                                // Save avatar to user
                                category.Image = uploadResult.UploadedFileName;
                            }

                        }


                        category.Description = categoryViewModel.Description;
                        category.IsLocked = categoryViewModel.IsLocked;
                        category.ModeratePosts = categoryViewModel.ModeratePosts;
                        category.ModerateTopics = categoryViewModel.ModerateTopics;
                        category.Name = categoryViewModel.Name;
                        category.SortOrder = categoryViewModel.SortOrder;
                        category.PageTitle = categoryViewModel.PageTitle;
                        category.MetaDescription = categoryViewModel.MetaDesc;
                        category.Colour = categoryViewModel.CategoryColour;

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
                            MessageType = GenericMessages.danger
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
                    var all = _categoryService.GetAll();

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
                        MessageType = GenericMessages.danger
                    };                    
                }

                return RedirectToAction("Index");
            }
        }

        private static List<Category> GetAllCategorySubCategories(Category parent, List<Category> allSubCategories, List<Category> subCats)
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
