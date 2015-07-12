using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Services;

namespace Badge.Padawan
{
    [Id("A88C62B2-394F-4D89-B61E-04A7B546416B")]
    [Name("PadawanBadge")]
    [DisplayName("Badge.PadawanBadge.Name")]
    [Description("Badge.PadawanBadge.Desc")]
    [Image("padawan.png")]
    [AwardsPoints(10)]
    public class PadawanBadge : IMarkAsSolutionBadge
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

            return (usersSolutions.Count >= 10);
        }
    }
}
