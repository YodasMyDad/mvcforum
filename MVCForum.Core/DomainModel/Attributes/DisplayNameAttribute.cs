using System;
using System.Web.Mvc;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Domain.DomainModel.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DisplayNameAttribute : Attribute
    {
        private readonly ILocalizationService _localizationService;
        public DisplayNameAttribute()
        {
            _localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
        }
        public string DisplayName { get; set; }

        public DisplayNameAttribute(string desc)
        {
            DisplayName = _localizationService.GetResourceString(desc.Trim());
        }
    }
}
