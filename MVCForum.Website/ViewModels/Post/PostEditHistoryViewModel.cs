namespace MvcForum.Web.ViewModels.Post
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class PostEditHistoryViewModel
    {
        public IList<PostEdit> PostEdits { get; set; }
    }
}