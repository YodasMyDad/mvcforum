namespace Badge.PosterVoteUp
{
    using System.Linq;
    using MvcForum.Core.DomainModel.Attributes;
    using MvcForum.Core.DomainModel.Entities;
    using MvcForum.Core.Interfaces.Badges;

    [Id("2ac1fc11-2f9e-4d5a-9df4-29715f10b6d1")]
    [Name("PosterVoteUp")]
    [DisplayName("Badge.PosterVoteUp.Name")]
    [Description("Badge.PosterVoteUp.Desc")]
    [Image("PosterVoteUpBadge.png")]
    [AwardsPoints(2)]
    public class PosterVoteUpBadge : IVoteUpBadge
    {
        public bool Rule(MembershipUser user)
        {
            return user.Posts != null && user.Posts.Any(post => post.Votes.Count > 0);
        }
    }
}