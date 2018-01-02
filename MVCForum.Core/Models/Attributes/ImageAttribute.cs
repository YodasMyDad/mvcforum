namespace MvcForum.Core.Models.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class ImageAttribute : Attribute
    {
        public ImageAttribute(string name)
        {
            Image = name;
        }

        public string Image { get; set; }
    }
}