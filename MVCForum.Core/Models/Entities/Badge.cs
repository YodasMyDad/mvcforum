namespace MvcForum.Core.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using Interfaces;
    using Utilities;

    public partial class Badge : ExtendedDataEntity, IBaseEntity
    {
        public Badge()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int Milestone { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int? AwardsPoints { get; set; }
        public virtual IList<MembershipUser> Users { get; set; }
    }
}