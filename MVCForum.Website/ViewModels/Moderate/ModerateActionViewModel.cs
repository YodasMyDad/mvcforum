namespace MvcForum.Web.ViewModels
{
    using System;

    public class ModerateActionViewModel
    {
        public bool IsApproved { get; set; }
        public Guid TopicId { get; set; }
        public Guid PostId { get; set; }
    }
}