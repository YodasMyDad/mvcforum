﻿namespace MvcForum.Core.Models.Entities
{
    public partial class PermissionForRole
    {
        public Permission Permission { get; set; }
        public MembershipRole MembershipRole { get; set; }
        public Category Category { get; set; }
        public bool IsTicked { get; set; }
    }
}
