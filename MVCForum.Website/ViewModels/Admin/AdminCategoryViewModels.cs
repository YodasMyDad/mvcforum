namespace MvcForum.Web.ViewModels.Admin
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.Models.Entities;

    public class ListCategoriesViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
    }

    public class CategoryEditViewModel
    {
        public CategoryEditViewModel()
        {
            AllCategories = new List<SelectListItem>();
            AllSections = new List<SelectListItem>();
        }

        [HiddenInput]
        public Guid Id { get; set; }

        [DisplayName("Category Name")]
        [Required]
        [StringLength(600)]
        public string Name { get; set; }

        [DisplayName("Category Description")]
        [DataType(DataType.MultilineText)]
        [UIHint(Constants.EditorType)]
        [AllowHtml]
        public string Description { get; set; }

        [DisplayName("Category Colour")]
        [UIHint(Constants.EditorTemplateColourPicker)]
        [AllowHtml]
        public string CategoryColour { get; set; }

        [DisplayName("Lock The Category")]
        public bool IsLocked { get; set; }

        [DisplayName("Moderate all topics in this Category")]
        public bool ModerateTopics { get; set; }

        [DisplayName("Moderate all posts in this Category")]
        public bool ModeratePosts { get; set; }

        [DisplayName("Sort Order")]
        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }

        [DisplayName("Parent Category")]
        public Guid? ParentCategory { get; set; }

        [DisplayName("Section")]
        public Guid? Section { get; set; }

        public List<SelectListItem> AllCategories { get; set; }

        public IEnumerable<SelectListItem> AllSections { get; set; }

        [DisplayName("Page Title")]
        [MaxLength(80)]
        public string PageTitle { get; set; }

        [DisplayName("Meta Desc")]
        [MaxLength(200)]
        public string MetaDesc { get; set; }

        [DisplayName("Category Image")]
        public HttpPostedFileBase[] Files { get; set; }

        public string Image { get; set; }
    }

    public class DeleteCategoryViewModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        public Category Category { get; set; }
        public List<Category> SubCategories { get; set; }
    }
}