
namespace Badge.AuthorMarkAsSolution
{
    using MvcForum.Core.Interfaces.Badges;
    using MvcForum.Core.Interfaces.Services;
    using System.Web.Mvc;
    using MvcForum.Core.Models.Attributes;
    using MvcForum.Core.Models.Entities;

    [Id("d68c289a-e3f7-4f55-ae4f-fc7ac2147781")]
    [Name("AuthorMarkAsSolution")]
    [DisplayName("Badge.AuthorMarkAsSolution.Name")]
    [Description("Badge.AuthorMarkAsSolution.Desc")]
    [Image("UserMarkAsSolutionBadge.png")]
    [AwardsPoints(2)]
    public class AuthorMarkAsSolutionBadge : IMarkAsSolutionBadge
    {
        private readonly ITopicService _topicService;
        private readonly ICategoryService _categoryService;

        public AuthorMarkAsSolutionBadge()
        {
            _topicService = DependencyResolver.Current.GetService<ITopicService>();
            _categoryService = DependencyResolver.Current.GetService<ICategoryService>();
        }

        /// <summary>
        /// Post is marked as the answer to a topic - give the topic author a badge
        /// </summary>
        /// <returns></returns>
        public bool Rule(MembershipUser user)
        {
            var allCats = _categoryService.GetAll();
            return _topicService.GetSolvedTopicsByMember(user.Id, allCats).Count >= 1;

        }
    }
}
