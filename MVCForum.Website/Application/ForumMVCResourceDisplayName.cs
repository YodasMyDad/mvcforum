using System.ComponentModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Website.Application
{
    public class ForumMvcResourceDisplayName: DisplayNameAttribute, IModelAttribute
    {
            private string _resourceValue = string.Empty;
            private readonly ILocalizationService _localizationService;

            public ForumMvcResourceDisplayName(string resourceKey)
                : base(resourceKey)
            {
                ResourceKey = resourceKey;
                _localizationService = ServiceFactory.Get<ILocalizationService>();
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

            public string Name
            {
                get { return "FMVCResourceDisplayName"; }
            }

    }
}

