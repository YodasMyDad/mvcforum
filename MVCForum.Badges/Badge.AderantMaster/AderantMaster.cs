using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Services;

namespace Badge.AderantMaster
{
    [Id("3bf5086c-0188-496c-9842-93aba1025b55")]
    [Name("AderantMaster")]
    [DisplayName("Badge.AderantMaster.Name")]
    [Description("Badge.AderantMaster.Desc")]
    [Image("AderantMaster.png")]
    [AwardsPoints(10)]
    public class AderantMasterBadge : IMarkAsSolutionBadge
    {
        public bool Rule(MembershipUser user)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var postService = DependencyResolver.Current.GetService<IPostService>();
            var cats = DependencyResolver.Current.GetService<ICategoryService>().GetAll();
            var usersSolutions = postService.GetSolutionsByMember(user.Id, cats);

            return (usersSolutions.Count >= 10);
        }
    }
}
