using System;
using System.Web.Mvc;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Website.Application
{
    public class SiteConstants
    {
        #region Singleton
        private static SiteConstants _instance;
        private static readonly object InstanceLock = new object();
        private static IConfigService _configService;
        private SiteConstants(IConfigService configService)
        {
            _configService = configService;
        }

        public static SiteConstants Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                        {
                            var configService = DependencyResolver.Current.GetService<IConfigService>();
                            _instance = new SiteConstants(configService);
                        }
                    }
                }

                return _instance;
            }
        }
        #endregion

        #region Generic Get

        /// <summary>
        /// This is the generic get config method, you can use this to also get custom config items out
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConfig(string key)
        {
            var dict = _configService.GetForumConfig();
            if (!string.IsNullOrEmpty(key) && dict.ContainsKey(key))
            {
                return dict[key];
            }
            return string.Empty;
        }


        #endregion

        public string MvcForumVersion => GetConfig("MVCForumVersion");

        /// <summary>
        /// Social Login Keys
        /// </summary>
        public string FacebookAppId => GetConfig("FacebookAppId");
        public string FacebookAppSecret => GetConfig("FacebookAppSecret");
        public string MicrosoftAppId => GetConfig("MicrosoftAppId");
        public string MicrosoftAppSecret => GetConfig("MicrosoftAppSecret");
        public string GooglePlusAppId => GetConfig("GooglePlusAppId");
        public string GooglePlusAppSecret => GetConfig("GooglePlusAppSecret");

        /// <summary>
        /// File Upload Settings
        /// </summary>
        public string FileUploadAllowedExtensions => GetConfig("FileUploadAllowedExtensions");
        public string FileUploadMaximumFileSizeInBytes => GetConfig("FileUploadMaximumFileSizeInBytes");
        public string UploadFolderPath => GetConfig("UploadFolderPath");
        public int PrivateMessageWarningAmountLessThanAllowedSize => Convert.ToInt32(GetConfig("PrivateMessageWarningAmountLessThanAllowedSize"));

        /// <summary>
        /// Paging options - Amount per page on different pages.
        /// </summary>
        public int PagingGroupSize => Convert.ToInt32(GetConfig("PagingGroupSize"));
        public int AdminListPageSize => Convert.ToInt32(GetConfig("AdminListPageSize"));
        public int ActiveTopicsListSize => Convert.ToInt32(GetConfig("ActiveTopicsListSize"));
        public int SearchListSize => Convert.ToInt32(GetConfig("SearchListSize"));
        public int MembersActivityListSize => Convert.ToInt32(GetConfig("MembersActivityListSize"));
        public int PrivateMessageListSize => Convert.ToInt32(GetConfig("PrivateMessageListSize"));
        public int SimilarTopicsListSize => Convert.ToInt32(GetConfig("SimilarTopicsListSize"));

        /// <summary>
        /// Social Gravatar size
        /// </summary>
        public int GravatarPostSize => Convert.ToInt32(GetConfig("GravatarPostSize"));
        public int GravatarTopicSize => Convert.ToInt32(GetConfig("GravatarTopicSize"));
        public int GravatarProfileSize => Convert.ToInt32(GetConfig("GravatarProfileSize"));
        public int GravatarLeaderboardSize => Convert.ToInt32(GetConfig("GravatarLeaderboardSize"));
        public int GravatarLikedBySize => Convert.ToInt32(GetConfig("GravatarLikedBySize"));
        public int GravatarLatestBySize => Convert.ToInt32(GetConfig("GravatarLatestBySize"));
        public int GravatarFooterSize => Convert.ToInt32(GetConfig("GravatarFooterSize"));


        /// <summary>
        /// Which Editor the site should use
        /// </summary>
        public string ChosenEditor => GetConfig("EditorType");
    }
}