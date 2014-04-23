using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;

namespace UserVoteUpBadge
{
    [Id("c9913ee2-b8e0-4543-8930-c723497ee65c")]
    [Name("UserVoteUp")]
    [DisplayName("You've Given Your First Vote Up")]
    [Description("This badge is awarded to users after they make their first vote up.")]
    [Image("UserVoteUpBadge.png")]
    [AwardsPoints(2)]
    public class UserVoteUpBadge : IVoteUpBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            return user.Votes != null && user.Votes.Count >= 1;
        }
    }
}
