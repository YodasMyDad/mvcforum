using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.Areas.Admin.ViewModels
{
    public class MoveTagsViewModel
    {
        [Required]
        [DisplayName("Current Tag")]
        public Guid CurrentTagId { get; set; }
        [Required]
        [DisplayName("Tag where everything is going")]
        public Guid NewTagId { get; set; }
        public List<SelectListItem> Tags { get; set; }
    }

    public class ListTagsViewModel
    {
        public PagedList<TopicTag> Tags { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public string Search { get; set; }
    }

    public class AjaxEditTagViewModel
    {
        [Required]
        public string OldName { get; set; }

        [Required]
        public string NewName { get; set; }
    }
}