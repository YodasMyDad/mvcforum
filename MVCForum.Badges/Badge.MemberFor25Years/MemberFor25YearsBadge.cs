using System;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;

namespace Badge.MemberFor5Years
{
    [Id("c5bba23b-f3fc-4139-8f3e-765fed2ca4ec")]
    [Name("MemberFor25Years")]
    [DisplayName("Badge.MemberFor25Years.Name")]
    [Description("Badge.MemberFor25Years.Desc")]
    [Image("MemberFor25YearsBadge.png")]
    [AwardsPoints(25)]
    public class MemberFor25YearsBadge : ITimeBadge
    {
        public bool Rule(MembershipUser user)
        {
            var anniversary = new DateTime(user.CreateDate.Year + 25, user.CreateDate.Month, user.CreateDate.Day);
            return DateTime.UtcNow >= anniversary;
        }
    }
}
