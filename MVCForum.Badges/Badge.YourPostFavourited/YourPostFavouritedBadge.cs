namespace Badge.YourPostFavourited
{
    using System.Linq;
    using System.Web.Mvc;
    using MvcForum.Core.Interfaces.Badges;
    using MvcForum.Core.Interfaces.Services;
    using MvcForum.Core.Models.Attributes;
    using MvcForum.Core.Models.Entities;

    [Id("6EF03C66-9094-40B6-9F60-10065BF89104")]
    [Name("YourPostFavourited")]
    [DisplayName("Badge.YourPostFavourited.Name")]
    [Description("Badge.YourPostFavourited.Desc")]
    [Image("your-post-got-favourited-by-another-member.png")]
    [AwardsPoints(2)]
    public class YourPostFavouritedBadge : IFavouriteBadge
    {
        private readonly IPostService _postService;

        public YourPostFavouritedBadge()
        {
            _postService = DependencyResolver.Current.GetService<IPostService>();
        }

        public bool Rule(MembershipUser user)
        {
            return _postService.GetPostsFavouritedByOtherMembers(user.Id).Any();
        }
    }
}