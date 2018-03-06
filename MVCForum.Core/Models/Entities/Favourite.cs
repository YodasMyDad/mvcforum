namespace MvcForum.Core.Models.Entities
{
    using System;
    using Interfaces;
    using Utilities;

    public partial class Favourite : IBaseEntity
    {
        public Favourite()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public DateTime DateCreated { get; set; }
        public virtual MembershipUser Member { get; set; }
        public virtual Post Post { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
