using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;


namespace Badge.FavouriteFirstPost
{
    [Id("2D368A21-E62A-4158-885B-D08C002EC3BF")]
    [Name("FavouriteFirstPost")]
    [DisplayName("Badge.FavouriteFirstPost.Name")]
    [Description("Badge.FavouriteFirstPost.Desc")]
    [Image("you-favourited-your-first-post.png")]
    [AwardsPoints(1)]
    public class FavouriteFirstPostBadge : IFavouriteBadge
    {
        public bool Rule(MembershipUser user)
        {
            return user.Favourites.Count > 1;
        }
    }
}
