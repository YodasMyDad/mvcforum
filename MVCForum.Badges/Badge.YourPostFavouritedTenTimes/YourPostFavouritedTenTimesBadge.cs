namespace Badge.YourPostFavouritedTenTimes
{
    using System.Linq;
    using System.Web.Mvc;
    using MvcForum.Core.Interfaces.Badges;
    using MvcForum.Core.Interfaces.Services;
    using MvcForum.Core.Models.Attributes;
    using MvcForum.Core.Models.Entities;

    [Id("C34784C1-FA77-4A0A-8141-9762A4069961")]
    [Name("YourPostFavouritedTenTimes")]
    [DisplayName("Badge.YourPostFavouritedTenTimes.Name")]
    [Description("Badge.YourPostFavouritedTenTimes.Desc")]
    [Image("recognised-post.png")]
    [AwardsPoints(10)]
    public class YourPostFavouritedTenTimesBadge : IFavouriteBadge
    {
        private readonly IPostService _postService;

        public YourPostFavouritedTenTimesBadge()
        {
            _postService = DependencyResolver.Current.GetService<IPostService>();
        }

        public bool Rule(MembershipUser user)
        {
            return _postService.GetPostsByFavouriteCount(user.Id, 10).Any();
        }
    }
}