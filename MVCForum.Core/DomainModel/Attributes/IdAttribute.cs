using System;

namespace MVCForum.Domain.DomainModel.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IdAttribute : Attribute
    {
        public Guid Id { get; set; }

        public IdAttribute(string guid)
        {
            Id = new Guid(guid);
        }
    }
}
