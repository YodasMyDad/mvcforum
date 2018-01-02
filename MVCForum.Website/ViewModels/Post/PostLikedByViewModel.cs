namespace MvcForum.Web.ViewModels.Post
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class PostLikedByViewModel
    {
        public List<Vote> Votes { get; set; }
        public Post Post { get; set; }
    }
}