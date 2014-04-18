using System;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.Areas.Admin.ViewModels
{
    public class AdminModerateViewModel
    {
        public PagedList<Topic> Topics { get; set; }
        public PagedList<Post> Posts { get; set; }
    }

    public class ModerateActionViewModel
    {
        public bool IsApproved { get; set; }
        public Guid TopicId { get; set; }
        public Guid PostId { get; set; }
    }
}