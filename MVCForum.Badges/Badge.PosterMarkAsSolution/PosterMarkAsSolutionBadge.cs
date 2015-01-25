using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Services;

namespace Badge.PosterMarkAsSolution
{
    [Id("8250f9f0-84d2-4dff-b651-c3df9e12bf2a")]
    [Name("PosterMarkAsSolution")]
    [DisplayName("Badge.PosterMarkAsSolutionBadge.Name")]
    [Description("Badge.PosterMarkAsSolutionBadge.Desc")]
    [Image("PosterMarkAsSolutionBadge.png")]
    [AwardsPoints(2)]
    public class PosterMarkAsSolutionBadge : IMarkAsSolutionBadge
    {
        public bool Rule(MembershipUser user)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var postService = DependencyResolver.Current.GetService<IPostService>();
            var usersSolutions = postService.GetSolutionsByMember(user.Id);

            return (usersSolutions.Count >= 1);
        }
    }
}
