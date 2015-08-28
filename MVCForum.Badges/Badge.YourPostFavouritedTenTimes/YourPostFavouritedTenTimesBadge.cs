using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Services;

namespace Badge.YourPostFavouritedTenTimes
{
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


