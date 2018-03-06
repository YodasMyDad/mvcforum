namespace MvcForum.Web.ViewModels.Admin
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Core.Models.Entities;
    using Core.Models.General;

    public class BannedWordListViewModel
    {
        public PaginatedList<BannedWord> Words { get; set; }

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