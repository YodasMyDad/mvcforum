namespace MvcForum.Core.Models.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class FilePathAttribute : Attribute
    {
        public FilePathAttribute(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; set; }
    }
}