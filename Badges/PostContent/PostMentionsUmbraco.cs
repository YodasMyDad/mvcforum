using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;

namespace Badges.PostContent
{
    [Id("9a247d50-35b5-4cd2-adaa-a0cf013325ac")]
    [Name("PostContainsUmbraco")]
    [DisplayName("Mentioned Umbraco In A Post Or Topic")]
    [Description("This badge is awarded to a user who mentions Umbraco in their latest post.")]
    [Image("MentionsUmbracoBadge.png")]
    [AwardsPoints(1)]
    public class PostMentionsUmbraco : IPostBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            var lastPost = user.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();
            if(lastPost != null && lastPost.PostContent.ToLower().Contains("umbraco"))
            {
                return true;
            }
            return false;
        }
    }
}
