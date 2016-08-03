using System;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;

namespace Badge.MemberFor15Years
{
    [Id("f61e0338-5f76-451b-a10b-0b5c07462506")]
    [Name("MemberFor15Years")]
    [DisplayName("Badge.MemberFor15Years.Name")]
    [Description("Badge.MemberFor15Years.Desc")]
    [Image("MemberFor15YearsBadge.png")]
    [AwardsPoints(15)]
    public class MemberFor15YearsBadge : ITimeBadge
    {
        public bool Rule(MembershipUser user)
        {
            var anniversary = new DateTime(user.CreateDate.Year + 15, user.CreateDate.Month, user.CreateDate.Day);
            return DateTime.UtcNow >= anniversary;
        }
    }
}
