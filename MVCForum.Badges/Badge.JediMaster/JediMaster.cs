using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Services;

namespace Badge.JediMaster
{
    [Id("4c54474b-51c2-4a52-bad2-96af5dea14d1")]
    [Name("JediMaster")]
    [DisplayName("Badge.JediMaster.Name")]
    [Description("Badge.JediMaster.Desc")]
    [Image("jedi.png")]
    [AwardsPoints(50)]
    public class JediMasterBadge : IMarkAsSolutionBadge
    {
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;

        public JediMasterBadge()
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

            return (usersSolutions.Count >= 50);
        }
    }
}
