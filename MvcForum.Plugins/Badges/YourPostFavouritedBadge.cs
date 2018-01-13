namespace MvcForum.Plugins.Badges
{
    using System.Linq;
    using Core.Interfaces.Badges;
    using Core.Interfaces.Services;
    using Core.Models.Attributes;
    using Core.Models.Entities;

    [Id("6EF03C66-9094-40B6-9F60-10065BF89104")]
    [Name("YourPostFavourited")]
    [DisplayName("Badge.YourPostFavourited.Name")]
    [Description("Badge.YourPostFavourited.Desc")]
    [Image("your-post-got-favourited-by-another-member.png")]
    [AwardsPoints(2)]
    public class YourPostFavouritedBadge : IFavouriteBadge
    {
        private readonly IPostService _postService;

        public YourPostFavouritedBadge(IPostService postService)
        {
            _postService = postService;
        }

        public bool Rule(MembershipUser user)
        {
            return _postService.GetPostsFavouritedByOtherMembers(user.Id).Any();
        }
    }
}