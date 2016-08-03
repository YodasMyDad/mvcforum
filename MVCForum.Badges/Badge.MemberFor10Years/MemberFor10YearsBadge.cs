using System;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;

namespace Badge.MemberFor10Years
{
    [Id("ed2d5c2a-e6c6-4ca7-ab9a-ca24c8441a70")]
    [Name("MemberFor10Years")]
    [DisplayName("Badge.MemberFor10Years.Name")]
    [Description("Badge.MemberFor10Years.Desc")]
    [Image("MemberFor10YearsBadge.png")]
    [AwardsPoints(10)]
    public class MemberFor10YearsBadge : ITimeBadge
    {
        public bool Rule(MembershipUser user)
        {
            var anniversary = new DateTime(user.CreateDate.Year + 10, user.CreateDate.Month, user.CreateDate.Day);
            return DateTime.UtcNow >= anniversary;
        }
    }
}
