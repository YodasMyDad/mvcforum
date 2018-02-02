namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using Application;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using ExtensionMethods;
    using Web.ViewModels;
    using Web.ViewModels.Admin;

    [Authorize(Roles = Constants.AdminRoleName)]
    public class AdminCategoryController : BaseAdminController
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoryController(ILoggingService loggingService,
            IMvcForumContext context,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            ICategoryService categoryService,
            ISettingsService settingsService)
            : base(loggingService, membershipService, localizationService, settingsService, context)
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
            var viewModel = new ListCategoriesViewModel
            {
                Categories = _categoryService.GetAll().OrderBy(x => x.SortOrder)
            };
            return PartialView(viewModel);
        }

        /// <summary>
        /// Removes the category image
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult RemoveCategoryImage(Guid id)
        {
            var category = _categoryService.Get(id);
            category.Image = string.Empty;
            Context.SaveChanges();
            return RedirectToAction("EditCategory", new { id });
        }

        public ActionResult CreateCategory()
        {
            var categoryViewModel = new CategoryEditViewModel
            {
                AllCategories = _categoryService.GetBaseSelectListCategories(_categoryService.GetAll())
            };
            return View(categoryViewModel);
        }        

        /// <summary>
        /// Create category logic
        /// </summary>
        /// <param name="categoryViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateCategory(CategoryEditViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {
                var category = categoryViewModel.ToCategory();

                var categoryResult = await _categoryService.Create(category, categoryViewModel.Files, categoryViewModel.ParentCategory);
                if (!categoryResult.Successful)
                {
                    ModelState.AddModelError("", categoryResult.ProcessLog.FirstOrDefault());
                }
                else
                {
                    // We use temp data because we are doing a redirect
                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Category Created",
                        MessageType =
                            GenericMessages.success
                    };
                    return RedirectToAction("Index");
                }
            }
            else
            {
                ModelState.AddModelError("", "There was an error creating the category");
            }

            categoryViewModel.AllCategories = _categoryService.GetBaseSelectListCategories(_categoryService.GetAll());

            return View(categoryViewModel);
        }

        public ActionResult EditCategory(Guid id)
        {
            var category = _categoryService.Get(id);
            var categoryViewModel = category.ToEditViewModel(_categoryService.GetBaseSelectListCategories(_categoryService.GetAll()
            .Where(x => x.Id != category.Id)
            .ToList()));

            return View(categoryViewModel);
        }

        [HttpPost]
        public async Task<ActionResult> EditCategory(CategoryEditViewModel categoryViewModel)
        {
            if (ModelState.IsValid)
            {
                // Reset the select list
                categoryViewModel.AllCategories = _categoryService.GetBaseSelectListCategories(_categoryService.GetAll()
                    .Where(x => x.Id != categoryViewModel.Id)
                    .ToList());

                var categoryToEdit = _categoryService.Get(categoryViewModel.Id);

                var category = categoryViewModel.ToCategory(categoryToEdit);

                var categoryResult = await _categoryService.Edit(category, categoryViewModel.Files, categoryViewModel.ParentCategory);
                if (!categoryResult.Successful)
                {
                    ModelState.AddModelError("", categoryResult.ProcessLog.FirstOrDefault());
                }
                else
                {
                    // We use temp data because we are doing a redirect
                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Category Edited",
                        MessageType = GenericMessages.success
                    };

                    // Set the view model
                    categoryViewModel = categoryResult.EntityToProcess.ToEditViewModel(
                        _categoryService.GetBaseSelectListCategories(_categoryService.GetAll()
                            .Where(x => x.Id != categoryViewModel.Id)
                            .ToList()));
                }
            }

            return View(categoryViewModel);
        }

        public ActionResult DeleteCategoryConfirmation(Guid id)
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

        public ActionResult DeleteCategory(Guid id)
        {
            try
            {
                var cat = _categoryService.Get(id);
                _categoryService.Delete(cat);
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Category Deleted",
                    MessageType = GenericMessages.success
                };
                Context.SaveChanges();
            }
            catch (Exception)
            {
                Context.RollBack();
            }

            return RedirectToAction("Index");
        }

        public ActionResult SyncCategoryPaths()
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
                            if (string.IsNullOrWhiteSpace(prevPath))
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
                    Context.SaveChanges();
                }


                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Category Paths Synced",
                    MessageType = GenericMessages.success
                };
                Context.SaveChanges();
            }
            catch (Exception)
            {
                Context.RollBack();
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Error syncing paths",
                    MessageType = GenericMessages.danger
                };
            }

            return RedirectToAction("Index");
        }

        private static List<Category> GetAllCategorySubCategories(Category parent, List<Category> allSubCategories,
            List<Category> subCats)
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