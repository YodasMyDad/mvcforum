using System;
using System.Configuration;

namespace MVCForum.Website.Application
{
    public static partial class SiteConstants
    {
        /// <summary>
        /// Social Login Keys
        /// </summary>
        public static string FacebookAppId
        {
            get
            {
                return ConfigurationManager.AppSettings["FacebookAppId"];
            }
        }
        public static string FacebookAppSecret
        {
            get
            {
                return ConfigurationManager.AppSettings["FacebookAppSecret"];
            }
        }
        public static string GooglePlusAppId
        {
            get
            {
                return ConfigurationManager.AppSettings["GooglePlusAppId"];
            }
        }
        public static string GooglePlusAppSecret
        {
            get
            {
                return ConfigurationManager.AppSettings["GooglePlusAppSecret"];
            }
        }
        
        /// <summary>
        /// File Upload Settings
        /// </summary>
        public static string FileUploadAllowedExtensions
        {
            get
            {
                return ConfigurationManager.AppSettings["FileUploadAllowedExtensions"];
            }
        }
        public static string FileUploadMaximumFileSizeInBytes
        {
            get
            {
                return ConfigurationManager.AppSettings["FileUploadMaximumFileSizeInBytes"];
            }
        }
        public static string UploadFolderPath
        {
            get
            {
                return ConfigurationManager.AppSettings["UploadFolderPath"];
            }
        }
        public static int PrivateMessageWarningAmountLessThanAllowedSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["PrivateMessageWarningAmountLessThanAllowedSize"]);
            }
        }


        /// <summary>
        /// Paging options - Amount per page on different pages.
        /// </summary>
        public static int PagingGroupSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["PagingGroupSize"]);
            }
        }


        public static int AdminListPageSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["AdminListPageSize"]);
            }
        }

        public static int ActiveTopicsListSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["ActiveTopicsListSize"]);
            }
        }

        public static int SearchListSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["SearchListSize"]);
            }
        }

        public static int MembersActivityListSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["MembersActivityListSize"]);
            }
        }

        public static int PrivateMessageListSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["PrivateMessageListSize"]);
            }
        }

        public static int SimilarTopicsListSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["SimilarTopicsListSize"]);
            }
        }

        /// <summary>
        /// Social Gravatar size
        /// </summary>
        public static int GravatarPostSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["GravatarPostSize"]);
            }
        }
        public static int GravatarTopicSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["GravatarTopicSize"]);
            }
        }
        public static int GravatarProfileSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["GravatarProfileSize"]);
            }
        }
        public static int GravatarLeaderboardSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["GravatarLeaderboardSize"]);
            }
        }
        public static int GravatarLikedBySize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["GravatarLikedBySize"]);
            }
        }

        public static int GravatarLatestBySize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["GravatarLatestBySize"]);
            }
        }

        public static int GravatarFooterSize
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["GravatarFooterSize"]);
            }
        }

        
        /// <summary>
        /// Which Editor the site should use
        /// </summary>
        public static string ChosenEditor
        {
            get
            {
                return ConfigurationManager.AppSettings["EditorType"];
            }
        }

        public const string EditorType = "forumeditor";

        // Misc
        public static int EmailsToSendPerJob
        {
            get
            {
                return Convert.ToInt32(ConfigurationManager.AppSettings["EmailsToSendPerJob"]);
            }
        }

        private static bool? _IsWIFRelyingParty = null;
        /// <summary>
        /// Whether the site will use wsFederation through Windows Identity Foundation to redirect to the wsFederation issuer (website) for authentication, bypassing MVCForum's forms authentication
        /// as well as gmail and facebook authentication.  (When using WIF on an SSO Site, authentication is handled by the issuer, if facebook/gmail is desired it should be implemented on the issuer.
        /// </summary>
        public static bool IsWIFRelyingParty
        {
            get
            {
                if (_IsWIFRelyingParty == null)
                {
                    bool ret = false;
                    bool.TryParse(ConfigurationManager.AppSettings["IsWIFRelyingParty"], out ret);
                    _IsWIFRelyingParty = ret;
                }
                return _IsWIFRelyingParty.Value;
            }
        }

        private static string _WIFAdminUser = null;
        public static string WIFAdminUser
        {
            get
            {
                if (string.IsNullOrEmpty(_WIFAdminUser))
                {
                    _WIFAdminUser = ConfigurationManager.AppSettings["WIFAdminUser"];
                }
                return _WIFAdminUser;
            }
        }
    }
}