using System;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;

namespace Badge.MemberFor20Years
{
    [Id("1e58355f-d426-4722-9208-5e9cd2f912f0")]
    [Name("MemberFor20Years")]
    [DisplayName("Badge.MemberFor20Years.Name")]
    [Description("Badge.MemberFor20Years.Desc")]
    [Image("MemberFor20YearsBadge.png")]
    [AwardsPoints(20)]
    public class MemberFor20YearsBadge : ITimeBadge
    {
        public bool Rule(MembershipUser user)
        {
            var anniversary = new DateTime(user.CreateDate.Year + 20, user.CreateDate.Month, user.CreateDate.Day);
            return DateTime.UtcNow >= anniversary;
        }
    }
}
