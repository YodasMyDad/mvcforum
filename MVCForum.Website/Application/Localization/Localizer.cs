namespace MvcForum.Web.Application.Localization
{
    using Core;

    public delegate LocalizedString Localizer(string text, params object[] args);
}