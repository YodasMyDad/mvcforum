namespace MvcForum.Core.Models.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class AwardsPointsAttribute : Attribute
    {
        public int Points { get; set; }

        public AwardsPointsAttribute(int points)
        {
            Points = points;
        }
    }
}
