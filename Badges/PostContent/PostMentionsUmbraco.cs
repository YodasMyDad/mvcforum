using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;

namespace Badges.PostContent
{
    [Id("")]
    [Name("PostContainsUmbraco")]
    [DisplayName("Mentioned Umbraco")]
    [Description("This badge is awarded to topic authors the first time they have a post marked as the answer.")]
    [Image("UserMarkAsSolutionBadge.png")]
    public class PostMentionsUmbraco : IPostBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            throw new System.NotImplementedException();
        }
    }
}
