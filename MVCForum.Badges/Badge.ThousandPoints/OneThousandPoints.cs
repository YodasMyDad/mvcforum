namespace Badge.ThousandPoints
{
    using System.Linq;
    using MvcForum.Core.Interfaces.Badges;
    using MvcForum.Core.Models.Attributes;
    using MvcForum.Core.Models.Entities;

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