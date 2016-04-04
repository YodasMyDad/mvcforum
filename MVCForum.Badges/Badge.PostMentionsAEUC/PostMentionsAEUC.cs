using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;

namespace Badge.PostMentionsUmbraco
{
    [Id("22268dda-da58-4cc2-aff2-a9c1d4fa1b12")]
    [Name("PostMentionsAEUC")]
    [DisplayName("Badge.PostMentionsAEUC.Name")]
    [Description("Badge.PostMentionsAEUC.Desc")]
    [Image("MentionsAEUCBadge.png")]
    [AwardsPoints(10)]
    public class PostMentionsAEUC : IPostBadge
    {
        public bool Rule(MembershipUser user)
        {
            var lastPost = user.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();
            if (lastPost != null && lastPost.PostContent.ToLower().Contains("aeuc"))
            {
                return true;
            }
            return false;
        }
    }
}
