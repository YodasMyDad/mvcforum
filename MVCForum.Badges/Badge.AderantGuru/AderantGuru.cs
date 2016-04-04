using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Services;

namespace Badge.AderantGuru
{
    [Id("034870a9-1d0f-4577-9e4d-e0b0a663d9fb")]
    [Name("AderantGuru")]
    [DisplayName("Badge.AderantGuru.Name")]
    [Description("Badge.AderantGuru.Desc")]
    [Image("AderantGuru.png")]
    [AwardsPoints(50)]
    public class AderantGuruBadge : IMarkAsSolutionBadge
    {

        public bool Rule(MembershipUser user)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var postService = DependencyResolver.Current.GetService<IPostService>();
            var cats = DependencyResolver.Current.GetService<ICategoryService>().GetAll();
            var usersSolutions = postService.GetSolutionsByMember(user.Id, cats);

            return (usersSolutions.Count >= 50);
        }
    }
}
