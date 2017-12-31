namespace MvcForum.Core.Models.Entities
{
    using System;
    using Utilities;

    public partial class GlobalPermissionForRole : Entity
    {
        public GlobalPermissionForRole()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual Permission Permission { get; set; }
        public virtual MembershipRole MembershipRole { get; set; }
        public bool IsTicked { get; set; }
    }
}
