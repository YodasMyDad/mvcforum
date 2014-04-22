using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class Permission : Entity
    {
        public Permission()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
