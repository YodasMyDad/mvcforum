namespace MvcForum.Core.Models.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class IdAttribute : Attribute
    {
        public IdAttribute(string guid)
        {
            Id = new Guid(guid);
        }

        public Guid Id { get; set; }
    }
}