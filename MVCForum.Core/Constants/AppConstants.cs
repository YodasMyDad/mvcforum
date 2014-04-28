namespace MVCForum.Domain.Constants
{
    public static class AppConstants
    {
        // Lucene String NAmes
        public const string LucId = "Id";
        public const string LucPostContent = "PostContent";
        public const string LucDateCreated = "DateCreated";
        public const string LucTopicName = "TopicName";
        public const string LucTopicId = "TopicId";
        public const string LucTopicUrl = "TopicUrl";
        public const string LucUsername = "Username";
        public const string LucUserId = "UserId";
        //private const string LucTopicTags = "TopicTags";

        //Mobile Check Name
        public const string IsMobileDevice = "IsMobileDevice";

        // Cookie names
        public const string LanguageIdCookieName = "LanguageCulture";
        public const string MemberEmailConfirmationCookieName = "MVCForumEmailConfirmation";

        // Cache names
        public const string SettingsCacheName = "MainSettings";
        public const string LocalizationCacheName = "Localization";
        public const string MemberCacheName = "#member#-{0}";
        
        //Querystring names
        public const string PostOrderBy = "order";

        // Themes
        public const string ThemeRootFolderName = "Themes";

        // Editor
        //public const string EditorType = "bbeditor";
        //public const string EditorType = "tinymceeditor";
        public const string EditorType = "markdowneditor";   

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

        // Main moderator Role [This should never be changed]
        public const string ModeratorRoleName = "Moderator";

        /// <summary>
        ///  These are the permission names used as keys to return them
        ///  So they must be the same as the database value 
        /// </summary>

        public const string PermissionReadOnly = "Read Only";
        public const string PermissionDeletePosts = "Delete Posts";
        public const string PermissionEditPosts = "Edit Posts";
        public const string PermissionCreateStickyTopics = "Sticky Topics";
        public const string PermissionDenyAccess = "Deny Access";
        public const string PermissionLockTopics = "Lock Topics";
        public const string PermissionVoteInPolls = "Vote In Polls";
        //Added in v1.3
        public const string PermissionCreatePolls = "Create Polls";
        public const string PermissionCreateTopics = "Create Topics";
        public const string PermissionAttachFiles = "Attach Files";

        // Paging options
        public const int AdminListPageSize = 30;
        public const int PagingGroupSize = 10;
        public const int ActiveTopicsListSize = 50;
        public const int PrivateMessageListSize = 30;
        public const string PagingUrlFormat = "{0}?p={1}";
        public const int SimilarTopicsListSize = 20;

        // Last Activity Time Check
        public const int TimeSpanInMinutesToDoCheck = 10;
        public const int TimeSpanInMinutesToShowMembers = 20;

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
        public const string GoToInstaller = "GoToInstaller";

        //Uploads
        public const string UploadFolderPath = "~/content/uploads/";

        //Social login Attributes
        public const string LoginGoogleFirstName = "http://axschema.org/namePerson/first";
        public const string LoginGoogleLastName = "http://axschema.org/namePerson/last";

    }
}
