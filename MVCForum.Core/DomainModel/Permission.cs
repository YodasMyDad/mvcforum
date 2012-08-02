using System;

namespace MVCForum.Domain.DomainModel
{
    public class Permission : Entity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
