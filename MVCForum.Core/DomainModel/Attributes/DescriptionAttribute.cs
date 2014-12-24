using System;
using System.Web.Mvc;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Domain.DomainModel.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public DescriptionAttribute(string desc)
        {
            var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            Description = localizationService.GetResourceString(desc.Trim());
        }
    }
}
