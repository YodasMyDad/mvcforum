using System;
using System.Web.Mvc;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Domain.DomainModel.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName { get; set; }

        public DisplayNameAttribute(string desc)
        {
            var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            DisplayName = localizationService.GetResourceString(desc.Trim());
        }
    }
}
