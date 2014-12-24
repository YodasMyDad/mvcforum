using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;

namespace MVCForum.Website.Badges
{
    [Id("8250f9f0-84d2-4dff-b651-c3df9e12bf2a")]
    [Name("PosterMarkAsSolution")]
    [DisplayName("Badge.PosterMarkAsSolutionBadge.Name")]
    [Description("Badge.PosterMarkAsSolutionBadge.Desc")]
    [Image("PosterMarkAsSolutionBadge.png")]
    [AwardsPoints(2)]
    public class PosterMarkAsSolutionBadge : IMarkAsSolutionBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var usersSolutions = api.Post.GetSolutionsWrittenByMember(user.Id);

            return (usersSolutions.Count >= 1);
        }
    }
}
