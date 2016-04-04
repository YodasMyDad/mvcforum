using System;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.Badges;

namespace Badge.MemberFor5Years
{
    [Id("09e758db-c8b9-4c70-959e-02832c6ad5ca")]
    [Name("MemberFor5Years")]
    [DisplayName("Badge.MemberFor5Years.Name")]
    [Description("Badge.MemberFor5Years.Desc")]
    [Image("MemberFor5YearsBadge.png")]
    [AwardsPoints(5)]
    public class MemberFor5YearsBadge : ITimeBadge
    {
        public bool Rule(MembershipUser user)
        {
            var anniversary = new DateTime(user.CreateDate.Year + 5, user.CreateDate.Month, user.CreateDate.Day);
            return DateTime.UtcNow >= anniversary;
        }
    }
}
