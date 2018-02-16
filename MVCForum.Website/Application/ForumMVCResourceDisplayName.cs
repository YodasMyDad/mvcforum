namespace MvcForum.Web.Application
{
    using System.ComponentModel;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Ioc;
    using Unity;

    public class ForumMvcResourceDisplayName : DisplayNameAttribute, IModelAttribute
    {
        private readonly ILocalizationService _localizationService;
        private string _resourceValue = string.Empty;

        public ForumMvcResourceDisplayName(string resourceKey)
            : base(resourceKey)
        {
            ResourceKey = resourceKey;
            _localizationService = UnityHelper.Container.Resolve<ILocalizationService>();
        }

        public string ResourceKey { get; set; }

        public override string DisplayName
        {
            get
            {
                _resourceValue = _localizationService.GetResourceString(ResourceKey.Trim());
                return _resourceValue;
            }
        }

        public string Name => "FMVCResourceDisplayName";
    }
}