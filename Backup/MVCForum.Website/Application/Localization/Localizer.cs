using MVCForum.Domain;

namespace MVCForum.Website.Application.Localization
{
    public delegate LocalizedString Localizer(string text, params object[] args);
}