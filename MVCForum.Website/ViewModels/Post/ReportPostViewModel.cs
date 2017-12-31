namespace MvcForum.Web.ViewModels.Post
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ReportPostViewModel
    {
        public Guid PostId { get; set; }
        public string PostCreatorUsername { get; set; }

        [Required]
        public string Reason { get; set; }
    }
}