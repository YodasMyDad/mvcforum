using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;

namespace Badge.VoteTheGrouch
{
    [Id("9ea3f651-ef37-4ad5-86a2-432012ad1e74")]
    [Name("TheGrouch")]
    [DisplayName("The Grouch")]
    [Description("This badge is awarded to users who have voted down other users posts 10 or more times.")]
    [Image("TheGrouch.png")]
    [AwardsPoints(0)]
    public class GrouchBadge : IVoteDownBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            // Get all down votes
            var downVotes = api.Vote.GetAllVotesGiven(user.Id).Where(x => x.Amount < 1).ToList();
            return downVotes.Count() >= 10;
        }
    }

}
