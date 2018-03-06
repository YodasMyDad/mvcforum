namespace MvcForum.Core.Models.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class NameAttribute : Attribute
    {
        public NameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}