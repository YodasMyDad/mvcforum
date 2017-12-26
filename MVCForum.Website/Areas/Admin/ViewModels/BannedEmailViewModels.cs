namespace MvcForum.Web.Areas.Admin.ViewModels
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Core.DomainModel.Entities;
    using Core.DomainModel.General;

    public class BannedEmailListViewModel
    {
        public PagedList<BannedEmail> Emails { get; set; }

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