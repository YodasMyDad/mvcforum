namespace MvcForum.Web.Areas.Admin.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Core.DomainModel.Entities;
    using Core.DomainModel.General;

    public class BannedWordListViewModel
    {
        public PagedList<BannedWord> Words { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public string Search { get; set; }
    }

    public class AddBannedWordViewModel
    {
        [Required]
        public string Word { get; set; }

        public bool IsStopWord { get; set; }
    }

    public class AjaxEditWordViewModel
    {
        [Required]
        public Guid WordId { get; set; }

        [Required]
        public string NewName { get; set; }
    }
}