namespace MvcForum.Web.ViewModels.Post
{
    using System.Collections.Generic;
    using Core.DomainModel.Entities;

    public class PostLikedByViewModel
    {
        public List<Vote> Votes { get; set; }
        public Post Post { get; set; }
    }
}