namespace MvcForum.Web.ViewModels.Moderate
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class ModerateViewModel
    {
        public IList<Topic> Topics { get; set; }
        public IList<Post> Posts { get; set; }
    }
}