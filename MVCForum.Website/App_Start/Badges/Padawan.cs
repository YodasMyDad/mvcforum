using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;

namespace MVCForum.Website.Badges
{
    [Id("A88C62B2-394F-4D89-B61E-04A7B546416B")]
    [Name("PadawanBadge")]
    [DisplayName("Badge.PadawanBadge.Name")]
    [Description("Badge.PadawanBadge.Desc")]
    [Image("padawan.png")]
    [AwardsPoints(10)]
    public class PadawanBadge : IMarkAsSolutionBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var usersSolutions = api.Post.GetSolutionsWrittenByMember(user.Id);

            return (usersSolutions.Count >= 10);
        }
    }
}
