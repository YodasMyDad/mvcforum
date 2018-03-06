namespace MvcForum.Core.Models.Attributes
{
    using System;
    using Interfaces.Services;
    using Ioc;
    using Unity;

    [AttributeUsage(AttributeTargets.Class)]
    public class DescriptionAttribute : Attribute
    {
        private readonly ILocalizationService _localizationService;

        public DescriptionAttribute(string desc)
        {
            if (_localizationService == null)
            {
                _localizationService = UnityHelper.Container.Resolve<ILocalizationService>();
            }

            Description = _localizationService.GetResourceString(desc.Trim());
        }

        public string Description { get; set; }
    }
}