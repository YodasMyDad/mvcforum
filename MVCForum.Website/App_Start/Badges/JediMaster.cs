using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;

namespace MVCForum.Website.Badges
{
    [Id("4c54474b-51c2-4a52-bad2-96af5dea14d1")]
    [Name("JediMasterBadge")]
    [DisplayName("Badge.JediMasterBadge.Name")]
    [Description("Badge.JediMasterBadge.Desc")]
    [Image("jedi.png")]
    [AwardsPoints(50)]
    public class JediMasterBadge : IMarkAsSolutionBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var usersSolutions = api.Post.GetSolutionsWrittenByMember(user.Id);

            return (usersSolutions.Count >= 50);
        }
    }
}
