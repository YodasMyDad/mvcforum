namespace MVCForum.Domain.Constants
{
    public static class AppConstants
    {
        // Cookie names
        public const string LanguageIdCookieName = "LanguageCulture";
        public const string MemberEmailConfirmationCookieName = "MVCForumEmailConfirmation";

        // Cache names
        public const string SettingsCacheName = "MainSettings";
        public const string LocalizationCacheName = "Localization";
        public const string MemberCacheName = "#member#-{0}";       

        // Url names
        public const string CategoryUrlIdentifier = "cat";
        public const string TopicUrlIdentifier = "topic";
        public const string TagsUrlIdentifier = "tagged";
        public const string MemberUrlIdentifier = "profile";

        // View Bag / Temp Data Constants
        public const string MessageViewBagName = "Message";
        public const string DefaultCategoryViewBagName = "DefaultCategory";
        public const string GlobalClass = "GlobalClass";
        public const string CurrentAction = "CurrentAction";
        public const string CurrentController = "CurrentController";
        public const string MemberRegisterViewModel = "MemberRegisterViewModel";
        
        // Main admin role [This should never be changed]
        public const string AdminRoleName = "Admin";

        // Main guest role [This should never be changed]
        // This is the role a non logged in user defaults to
        public const string GuestRoleName = "Guest";

        // This is just the initial standard role
        public const string StandardMembers = "Standard Members";

        /// <summary>
        ///  These are the permission names used as keys to return them
        ///  So they must be the same as the database value 
        /// </summary>
        
        // Category Permissions
        public const string PermissionReadOnly = "Read Only";
        public const string PermissionDeletePosts = "Delete Posts";
        public const string PermissionEditPosts = "Edit Posts";
        public const string PermissionCreateStickyTopics = "Sticky Topics";
        public const string PermissionDenyAccess = "Deny Access";
        public const string PermissionLockTopics = "Lock Topics";
        public const string PermissionVoteInPolls = "Vote In Polls";
        public const string PermissionCreatePolls = "Create Polls";
        public const string PermissionCreateTopics = "Create Topics";
        public const string PermissionAttachFiles = "Attach Files";

        // Global Permissions
        public const string PermissionEditMembers = "Edit Members";

        //------------ End Permissions ----------

        // Paging options
        public const string PagingUrlFormat = "{0}?p={1}";

        // How long 
        public const int TimeSpanInMinutesToShowMembers = 12;

        /// <summary>
        /// Last Activity Time Check. 
        /// </summary>
        public const int TimeSpanInMinutesToDoCheck = 3;

        // Installer Stuff
        public const string InstallerName = "YesImAnInstallerSpankMe";
        public const string InstallerUrl = "/install/";
        public const string InMobileView = "InMobileView";
        public const string GoToInstaller = "GoToInstaller";
        public const string SuccessDbFile = "SuccessDbFile.txt";

        // Database Connection Key
        public const string MvcForumContext = "MVCForumContext";


        // Default Theme folder
        public const string ThemeRootFolder = "~/Themes/";

        // Themes
        public const string ThemeRootFolderName = "Themes";

        
        public const string EditorTemplateColourPicker = "colourpicker";

        //Querystring names
        public const string PostOrderBy = "order";
        public const string AllPosts = "all";

        //Mobile Check Name
        public const string IsMobileDevice = "IsMobileDevice";

        /// <summary>
        /// The default cache length time
        /// </summary>
        public const int ShortCacheTime = 900;

        public const int LongCacheTime = 10800;
    }
}
