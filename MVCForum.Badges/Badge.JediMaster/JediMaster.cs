using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Services;

namespace Badge.JediMaster
{
    [Id("4c54474b-51c2-4a52-bad2-96af5dea14d1")]
    [Name("JediMasterBadge")]
    [DisplayName("Badge.JediMasterBadge.Name")]
    [Description("Badge.JediMasterBadge.Desc")]
    [Image("jedi.png")]
    [AwardsPoints(50)]
    public class JediMasterBadge : IMarkAsSolutionBadge
    {
        public bool Rule(MembershipUser user)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var postService = DependencyResolver.Current.GetService<IPostService>();
            var categoryService = DependencyResolver.Current.GetService<ICategoryService>();

            // Get all categories as we want to check all the members solutions, even across
            // categories that he no longer is allowed to access
            var cats = categoryService.GetAll();
            var usersSolutions = postService.GetSolutionsByMember(user.Id, cats.ToList());

            return (usersSolutions.Count >= 50);
        }
    }
}
