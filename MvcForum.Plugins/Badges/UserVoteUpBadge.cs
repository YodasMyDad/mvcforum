namespace MvcForum.Plugins.Badges
{
    using Core.Interfaces.Badges;
    using Core.Models.Attributes;
    using Core.Models.Entities;

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
            return user.VotesGiven != null && user.VotesGiven.Count >= 1;
        }
    }
}