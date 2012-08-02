using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.Areas.Admin.ViewModels
{
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