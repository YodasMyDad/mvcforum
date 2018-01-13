namespace MvcForum.Plugins.Badges
{
    using Core.Interfaces.Badges;
    using Core.Interfaces.Services;
    using Core.Models.Attributes;
    using Core.Models.Entities;

    [Id("4c54474b-51c2-4a52-bad2-96af5dea14d1")]
    [Name("JediMaster")]
    [DisplayName("Badge.JediMaster.Name")]
    [Description("Badge.JediMaster.Desc")]
    [Image("jedi.png")]
    [AwardsPoints(50)]
    public class JediMasterBadge : IMarkAsSolutionBadge
    {
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;

        public JediMasterBadge(ICategoryService categoryService, IPostService postService)
        {
            _categoryService = categoryService;
            _postService = postService;
        }

        public bool Rule(MembershipUser user)
        {
            //Post is marked as the answer to a topic - give the post author a badge

            // Get all categories as we want to check all the members solutions, even across
            // categories that he no longer is allowed to access
            var cats = _categoryService.GetAll();
            var usersSolutions = _postService.GetSolutionsByMember(user.Id, cats);

            return usersSolutions.Count >= 50;
        }
    }
}