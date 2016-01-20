using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;

namespace Badge.UserVoteUp
{
    [Id("c9913ee2-b8e0-4543-8930-c723497ee65c")]
    [Name("UserVoteUp")]
    [DisplayName("Badge.UserVoteUp.Name")]
    [Description("Badge.UserVoteUp.Desc")]
    [Image("UserVoteUpBadge.png")]
    [AwardsPoints(2)]
    public class UserVoteUpBadge : IVoteUpBadge
    {
        public bool Rule(MembershipUser user)
        {
            return user.Votes != null && user.Votes.Count >= 1;
        }
    }
}
