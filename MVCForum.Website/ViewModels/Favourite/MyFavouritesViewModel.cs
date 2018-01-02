namespace MvcForum.Web.ViewModels.Favourite
{
    using System.Collections.Generic;
    using Post;

    public class MyFavouritesViewModel
    {
        public MyFavouritesViewModel()
        {
            Posts = new List<PostViewModel>();
        }

        public List<PostViewModel> Posts { get; set; }
    }
}