using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Services;

namespace Badge.YourPostFavourited
{
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