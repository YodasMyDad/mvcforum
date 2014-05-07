using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;

namespace Badge.Padawan
{
    [Id("A88C62B2-394F-4D89-B61E-04A7B546416B")]
    [Name("PadawanBadge")]
    [DisplayName("Padawan")]
    [Description("Had 10 or more posts successfully marked as an answer.")]
    [Image("padawan.png")]
    [AwardsPoints(10)]
    public class PosterMarkAsSolutionBadge : IMarkAsSolutionBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var usersSolutions = api.Post.GetSolutionsWrittenByMember(user.Id);

            return (usersSolutions.Count >= 10);
        }
    }
}
