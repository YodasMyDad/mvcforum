namespace MVCForum.Domain.Constants
{
    public static class AppConstants
    {
        // Cookie names
        public const string LanguageCultureCookieName = "LanguageCulture";

        // Cache names
        public const string SettingsCacheName = "MainSettings";
        public const string LocalizationCacheName = "Localization";
        public const string MemberCacheName = "#member#-{0}";
        
        // Themes
        public const string ThemeRootFolderName = "Themes";

        // Url names
        public const string CategoryUrlIdentifier = "cat";
        public const string TopicUrlIdentifier = "chat";
        public const string TagsUrlIdentifier = "tagged";
        public const string MemberUrlIdentifier = "profile";

        // View Bag Constants
        public const string MessageViewBagName = "Message";
        public const string DefaultCategoryViewBagName = "DefaultCategory";
        
        // Main admin role [This should never be changed]
        public const string AdminRoleName = "Admin";

        // Main guest role [This should never be changed]
        // This is the role a non logged in user defaults to
        public const string GuestRoleName = "Guest";

        /// <summary>
        ///  These are the permission names used as keys to return them
        ///  So they must be the same as the database value 
        /// </summary>

        //public const string PermissionAttachFiles = "Attach Files";
        public const string PermissionReadOnly = "Read Only";
        public const string PermissionDeletePosts = "Delete Posts";
        public const string PermissionEditPosts = "Edit Posts";
        public const string PermissionCreateStickyTopics = "Sticky Topics";
        public const string PermissionDenyAccess = "Deny Access";
        public const string PermissionLockTopics = "Lock Topics";
        public const string PermissionVoteInPolls = "Vote In Polls";


        // Paging options
        public const int AdminListPageSize = 30;
        public const int PagingGroupSize = 10;
        public const int ActiveTopicsListSize = 20;
        public const int PrivateMessageListSize = 30;
        public const string PagingUrlFormat = "{0}?p={1}";

        // Social
        public const int GravatarPostSize = 45;
        public const int GravatarTopicSize = 32;
        public const int GravatarProfileSize = 70;
        public const int GravatarLeaderboardSize = 22;

        // System and default folder
        public const string ThemeRootFolder = "~/Themes/";
	
        /// <summary>
        /// A short cache time to help with speeding up the site
        /// </summary>
        public const int DefaultCacheLengthInSeconds = 600;

        public const string InstallerName = "YesImAnInstallerSpankMe";
        public const string InMobileView = "InMobileView";

    }
}
