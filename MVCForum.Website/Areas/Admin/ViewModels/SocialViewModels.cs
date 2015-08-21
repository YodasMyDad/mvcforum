namespace MVCForum.Website.Areas.Admin.ViewModels
{
    public class SocialSettingsViewModel
    {
        public bool EnableSocialLogins { get; set; }
        public string FacebookAppId { get; set; }
        public string FacebookAppSecret { get; set; }
        public string GooglePlusAppId { get; set; }
        public string GooglePlusAppSecret { get; set; }
    }
}