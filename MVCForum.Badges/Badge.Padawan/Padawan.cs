using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Services;

namespace Badge.Padawan
{
    [Id("A88C62B2-394F-4D89-B61E-04A7B546416B")]
    [Name("Padawan")]
    [DisplayName("Badge.Padawan.Name")]
    [Description("Badge.Padawan.Desc")]
    [Image("padawan.png")]
    [AwardsPoints(10)]
    public class PadawanBadge : IMarkAsSolutionBadge
    {
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;

        public PadawanBadge()
        {
            _postService = DependencyResolver.Current.GetService<IPostService>();
            _categoryService = DependencyResolver.Current.GetService<ICategoryService>();
        }


        public bool Rule(MembershipUser user)
        {
            //Post is marked as the answer to a topic - give the post author a badge

            // Get all categories as we want to check all the members solutions, even across
            // categories that he no longer is allowed to access
            var cats = _categoryService.GetAll();

            var usersSolutions = _postService.GetSolutionsByMember(user.Id, cats);

            return (usersSolutions.Count >= 10);
        }
    }
}
