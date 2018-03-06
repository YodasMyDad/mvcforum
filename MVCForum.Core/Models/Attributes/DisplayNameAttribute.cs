namespace MvcForum.Core.Models.Attributes
{
    using System;
    using Interfaces.Services;
    using Ioc;
    using Unity;

    [AttributeUsage(AttributeTargets.Class)]
    public class DisplayNameAttribute : Attribute
    {
        private readonly ILocalizationService _localizationService;

        public DisplayNameAttribute(string desc)
        {
            if (_localizationService == null)
            {
                _localizationService = UnityHelper.Container.Resolve<ILocalizationService>();
            }
            DisplayName = _localizationService.GetResourceString(desc.Trim());
        }

        public string DisplayName { get; set; }
    }
}