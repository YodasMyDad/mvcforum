namespace MvcForum.Core.DomainModel.Entities
{
    using System;
    using Utilities;

    public class Block : Entity
    {
        public Block()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }
        public MembershipUser Blocker { get; set; }
        public MembershipUser Blocked { get; set; }
        public DateTime Date { get; set; }
    }
}