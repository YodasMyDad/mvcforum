namespace MvcForum.Web.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Core.Models.Entities;
    using ViewModels.Admin;

    public static class CategoryViewModelExtensions
    {
        /// <summary>
        /// Turns a Edit view model into a category
        /// </summary>
        /// <param name="categoryViewModel"></param>
        /// <returns></returns>
        public static Category ToCategory(this CategoryEditViewModel categoryViewModel)
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
                Colour = categoryViewModel.CategoryColour,
                DateCreated = DateTime.UtcNow
            };

            return category;
        }

        /// <summary>
        /// Maps changed data to a category
        /// </summary>
        /// <param name="categoryViewModel"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public static Category ToCategory(this CategoryEditViewModel categoryViewModel, Category category)
        {
            category.Description = categoryViewModel.Description;
            category.IsLocked = categoryViewModel.IsLocked;
            category.ModeratePosts = categoryViewModel.ModeratePosts;
            category.ModerateTopics = categoryViewModel.ModerateTopics;
            category.Name = categoryViewModel.Name;
            category.SortOrder = categoryViewModel.SortOrder;
            category.PageTitle = categoryViewModel.PageTitle;
            category.MetaDescription = categoryViewModel.MetaDesc;
            category.Colour = categoryViewModel.CategoryColour;

            return category;
        }

        /// <summary>
        /// Maps a Category to the edit view model
        /// </summary>
        /// <param name="category"></param>
        /// <param name="allCategorySelectListItems"></param>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static CategoryEditViewModel ToEditViewModel(this Category category, List<SelectListItem> allCategorySelectListItems, IEnumerable<SelectListItem> sections)
        {
            var categoryViewModel = new CategoryEditViewModel
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
                ParentCategory = category.ParentCategory?.Id ?? Guid.Empty,
                Section = category.Section?.Id ?? Guid.Empty,
                AllCategories = allCategorySelectListItems,
                AllSections = sections
            };
            return categoryViewModel;
        }

    }
}