namespace MvcForum.Core.DomainModel.Attributes
{
    using System;
    using System.Web.Mvc;
    using Interfaces.Services;

    [AttributeUsage(AttributeTargets.Class)]
    public class DescriptionAttribute : Attribute
    {
        private readonly ILocalizationService _localizationService;

        public DescriptionAttribute(string desc)
        {
            if (_localizationService == null)
            {
                _localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            }

            Description = _localizationService.GetResourceString(desc.Trim());
        }

        public string Description { get; set; }
    }
}