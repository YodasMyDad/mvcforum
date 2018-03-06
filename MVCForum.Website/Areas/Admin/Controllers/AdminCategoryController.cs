namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using ExtensionMethods;
    using ViewModels;
    using ViewModels.Admin;

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
        ///     Removes the category image
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult RemoveCategoryImage(Guid id)
        {
            var category = _categoryService.Get(id);
            category.Image = string.Empty;
            Context.SaveChanges();
            return RedirectToAction("EditCategory", new {id});
        }

        public ActionResult CreateCategory()
        {
            var categoryViewModel = new CategoryEditViewModel
            {
                AllCategories = _categoryService.GetBaseSelectListCategories(_categoryService.GetAll()),
                AllSections = _categoryService.GetAllSections().ToSelectList()
            };
            return View(categoryViewModel);
        }

        /// <summary>
        ///     Create category logic
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

                var categoryResult = await _categoryService.Create(category, categoryViewModel.Files,
                    categoryViewModel.ParentCategory, categoryViewModel.Section);
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
            categoryViewModel.AllSections = _categoryService.GetAllSections().ToSelectList();

            return View(categoryViewModel);
        }

        public ActionResult EditCategory(Guid id)
        {
            var category = _categoryService.Get(id);
            var categoryViewModel = category.ToEditViewModel(_categoryService.GetBaseSelectListCategories(_categoryService.GetAll().Where(x => x.Id != category.Id).ToList()), _categoryService.GetAllSections().ToSelectList());

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

                var categoryResult = await _categoryService.Edit(category, categoryViewModel.Files,
                    categoryViewModel.ParentCategory, categoryViewModel.Section);
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
                            .ToList()), _categoryService.GetAllSections().ToSelectList());
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

        public async Task<ActionResult> DeleteCategory(Guid id)
        {
            var cat = _categoryService.Get(id);
            var categoryResult = await _categoryService.Delete(cat);

            if (!categoryResult.Successful)
            {
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = categoryResult.ProcessLog.FirstOrDefault(),
                    MessageType = GenericMessages.danger
                };
            }
            else
            {
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Category Deleted",
                    MessageType = GenericMessages.success
                };
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

        #region Sections

        /// <summary>
        /// Sections page
        /// </summary>
        /// <returns></returns>
        public ActionResult Sections()
        {
            return View();
        }

        /// <summary>
        /// List sections
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public PartialViewResult GetSections()
        {
            var viewModel = new SectionListViewModel
            {
                Sections = _categoryService.GetAllSections()
            };
            return PartialView(viewModel);
        }

        /// <summary>
        /// Create edit section view
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult AddEditSection(Guid? id)
        {
            var categoryViewModel = new SectionAddEditViewModel();

            if (id != null)
            {
                var section = _categoryService.GetSection(id.Value);

                categoryViewModel.IsEdit = true;
                categoryViewModel.Id = section.Id;
                categoryViewModel.Name = section.Name;
                categoryViewModel.Description = section.Description;
                categoryViewModel.SortOrder = section.SortOrder;
            }

            return View(categoryViewModel);
        }

        /// <summary>
        ///     Create / Edit a section logic
        /// </summary>
        /// <param name="sectionAddEditViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddEditSection(SectionAddEditViewModel sectionAddEditViewModel)
        {
            if (ModelState.IsValid)
            {
                var section = sectionAddEditViewModel.IsEdit ? _categoryService.GetSection(sectionAddEditViewModel.Id) 
                                                                : new Section{DateCreated = DateTime.UtcNow};

                section.Name = sectionAddEditViewModel.Name;
                section.Description = sectionAddEditViewModel.Description;
                section.SortOrder = sectionAddEditViewModel.SortOrder;

                // TODO - This should all be in the service!!!
                if (!sectionAddEditViewModel.IsEdit)
                {
                    Context.Section.Add(section);
                }

                Context.SaveChanges();


                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Successful",
                    MessageType = GenericMessages.success
                };

                return RedirectToAction("Sections");
            }

            return View(sectionAddEditViewModel);
        }

        public ActionResult DeleteSection(Guid id)
        {
            _categoryService.DeleteSection(id);

            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Successful",
                MessageType = GenericMessages.success
            };

            return RedirectToAction("Sections");
        }

        #endregion

    }
}