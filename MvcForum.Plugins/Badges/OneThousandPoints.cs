namespace MvcForum.Plugins.Badges
{
    using System.Linq;
    using Core.Interfaces.Badges;
    using Core.Models.Attributes;
    using Core.Models.Entities;

    [Id("a54ec5d1-111d-4698-b2d0-78fbdaa52d1b")]
    [Name("OneThousandPoints")]
    [DisplayName("Badge.OneThousandPoints.Name")]
    [Description("Badge.OneThousandPoints.Desc")]
    [Image("OneThousandPoints.png")]
    [AwardsPoints(10)]
    public class OneThousandPoints : IPostBadge
    {
        public bool Rule(MembershipUser user)
        {
            var points = user.Points.Sum(x => x.Points);
            return points >= 1000;
        }
    }
}