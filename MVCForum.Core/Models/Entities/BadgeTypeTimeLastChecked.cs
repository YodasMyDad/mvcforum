namespace MvcForum.Core.Models.Entities
{
    using System;
    using Interfaces;
    using Utilities;

    public partial class BadgeTypeTimeLastChecked : IBaseEntity
    {
        public BadgeTypeTimeLastChecked()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }
        public string BadgeType { get; set; }
        public DateTime TimeLastChecked { get; set; }

        public virtual MembershipUser User { get; set; }
    }
}