namespace MvcForum.Web.ViewModels.Admin
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Core.Constants;

    public class SectionAddEditViewModel
    {
        [HiddenInput]
        public Guid Id { get; set; }

        [DisplayName("Section Name")]
        [Required]
        [StringLength(600)]
        public string Name { get; set; }

        [DisplayName("Section Description")]
        [DataType(DataType.MultilineText)]
        [UIHint(Constants.EditorType)]
        [AllowHtml]
        public string Description { get; set; }

        [DisplayName("Sort Order")]
        [Range(0, int.MaxValue)]
        public int SortOrder { get; set; }

        public bool IsEdit { get; set; }
    }
}