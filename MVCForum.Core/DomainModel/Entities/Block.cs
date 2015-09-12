using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel.Entities
{
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
