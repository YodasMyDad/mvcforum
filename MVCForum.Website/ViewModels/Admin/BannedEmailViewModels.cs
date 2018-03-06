namespace MvcForum.Web.ViewModels.Admin
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Core.Models.Entities;
    using Core.Models.General;

    public class BannedEmailListViewModel
    {
        public PaginatedList<BannedEmail> Emails { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public string Search { get; set; }
    }

    public class AddBannedEmailViewModel
    {
        [Required]
        public string Email { get; set; }
    }

    public class AjaxEditEmailViewModel
    {
        [Required]
        public Guid EmailId { get; set; }

        [Required]
        public string NewName { get; set; }
    }
}