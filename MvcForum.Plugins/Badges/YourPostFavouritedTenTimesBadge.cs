namespace MvcForum.Plugins.Badges
{
    using System.Linq;
    using Core.Interfaces.Badges;
    using Core.Interfaces.Services;
    using Core.Models.Attributes;
    using Core.Models.Entities;

    [Id("C34784C1-FA77-4A0A-8141-9762A4069961")]
    [Name("YourPostFavouritedTenTimes")]
    [DisplayName("Badge.YourPostFavouritedTenTimes.Name")]
    [Description("Badge.YourPostFavouritedTenTimes.Desc")]
    [Image("recognised-post.png")]
    [AwardsPoints(10)]
    public class YourPostFavouritedTenTimesBadge : IFavouriteBadge
    {
        private readonly IPostService _postService;

        public YourPostFavouritedTenTimesBadge(IPostService postService)
        {
            _postService = postService;
        }

        public bool Rule(MembershipUser user)
        {
            return _postService.GetPostsByFavouriteCount(user.Id, 10).Any();
        }
    }
}