using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Mvc.Routing.Constraints;
using DataAnnotationsExtensions;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.Areas.Admin.ViewModels
{
    public class ListCategoriesViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
    }

    public class CreateCategoryViewModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [DisplayName("Category Name")]
        [Required]
        [StringLength(600)]
        public string Name { get; set; }

        [DisplayName("Category Description")]
        [DataType(DataType.MultilineText)]
        [UIHint(SiteConstants.EditorType), AllowHtml]
        public string Description { get; set; }

        [DisplayName("Lock The Category")]
        public bool IsLocked { get; set; }

        [DisplayName("Moderate all topics in this Category")]
        public bool ModerateTopics { get; set; }

        [DisplayName("Moderate all posts in this Category")]
        public bool ModeratePosts { get; set; }

        [DisplayName("Sort Order")]
        [Numeric]
        public int SortOrder { get; set; }

        [DisplayName("Parent Category")]
        public Guid? ParentCategory { get; set; }

        public List<Category> AllCategories { get; set; }

        [DisplayName("Page Title")]
        [MaxLength(80)]
        public string PageTitle { get; set; }

        [DisplayName("Meta Desc")]
        [MaxLength(200)]
        public string MetaDesc { get; set; }
    }

    public class EditCategoryViewModel
    {
        [DisplayName("Category Name")]
        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [DisplayName("Category Description")]
        [DataType(DataType.MultilineText)]
        [UIHint(SiteConstants.EditorType), AllowHtml]
        public string Description { get; set; }

        [DisplayName("Lock The Category")]
        public bool IsLocked { get; set; }

        [DisplayName("Moderate all topics in this Category")]
        public bool ModerateTopics { get; set; }

        [DisplayName("Moderate all posts in this Category")]
        public bool ModeratePosts { get; set; }

        [DisplayName("Sort Order")]
        [Numeric]
        public int SortOrder { get; set; }

        [HiddenInput]
        public Guid Id { get; set; }

        [DisplayName("Parent Category")]
        public Guid? ParentCategory { get; set; }

        public List<Category> AllCategories { get; set; }

        [DisplayName("Page Title")]
        [MaxLength(80)]
        public string PageTitle { get; set; }

        [DisplayName("Meta Desc")]
        [MaxLength(200)]
        public string MetaDesc { get; set; }
    }

    public class DeleteCategoryViewModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        public Category Category { get; set; }

        public List<Category> SubCategories { get; set; }
    }
}