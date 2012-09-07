using System.ComponentModel;
using System.Web.Mvc;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Website.Application
{
    public class ForumMvcResourceDisplayName: DisplayNameAttribute, IModelAttribute
    {
            private string _resourceValue = string.Empty;

            public ForumMvcResourceDisplayName(string resourceKey)
                : base(resourceKey)
            {
                ResourceKey = resourceKey;
            }

            public string ResourceKey { get; set; }

            public override string DisplayName
            {
                get
                {
                    var localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
                    _resourceValue = localizationService.GetResourceString(localizationService.CurrentLanguage, ResourceKey.Trim());
                    return _resourceValue;
                }
            }

            public string Name
            {
                get { return "FMVCResourceDisplayName"; }
            }

    }
}

