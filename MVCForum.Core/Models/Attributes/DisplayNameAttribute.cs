namespace MvcForum.Core.Models.Attributes
{
    using System;
    using System.Web.Mvc;
    using Interfaces.Services;

    [AttributeUsage(AttributeTargets.Class)]
    public class DisplayNameAttribute : Attribute
    {
        private readonly ILocalizationService _localizationService;

        public DisplayNameAttribute(string desc)
        {
            if (_localizationService == null)
            {
                _localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            }
            DisplayName = _localizationService.GetResourceString(desc.Trim());
        }

        public string DisplayName { get; set; }
    }
}