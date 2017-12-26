namespace Badge.Grouch
{
    using System.Linq;
    using System.Web.Mvc;
    using MvcForum.Core.DomainModel.Attributes;
    using MvcForum.Core.DomainModel.Entities;
    using MvcForum.Core.Interfaces.Badges;
    using MvcForum.Core.Interfaces.Services;

    [Id("9ea3f651-ef37-4ad5-86a2-432012ad1e74")]
    [Name("TheGrouch")]
    [DisplayName("Badge.TheGrouch.Name")]
    [Description("Badge.TheGrouch.Desc")]
    [Image("TheGrouch.png")]
    [AwardsPoints(0)]
    public class GrouchBadge : IVoteDownBadge
    {
        private readonly IVoteService _voteService;

        public GrouchBadge()
        {
            _voteService = DependencyResolver.Current.GetService<IVoteService>();
        }

        public bool Rule(MembershipUser user)
        {
            // Get all down votes
            var downVotes = _voteService.GetAllVotesByUser(user.Id).Where(x => x.Amount < 1).ToList();
            return downVotes.Count >= 10;
        }
    }
}