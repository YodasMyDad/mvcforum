namespace MvcForum.Core.Models.Entities
{
    using System;
    using Enums;
    using Interfaces;
    using Utilities;

    public partial class MembershipUserPoints : IBaseEntity
    {
        public MembershipUserPoints()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public int Points { get; set; }
        public DateTime DateAdded { get; set; }
        public PointsFor PointsFor { get; set; }
        public Guid? PointsForId { get; set; }
        public string Notes { get; set; }
        public virtual MembershipUser User { get; set; }
    }
}
