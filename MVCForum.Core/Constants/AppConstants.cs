using System.Collections.Generic;
using System.Linq.Expressions;

namespace MVCForum.Domain.Constants
{
    public static class AppConstants
    {
        public const int SaltSize = 24;

        // Cookie names
        public const string LanguageIdCookieName = "LanguageCulture";
        public const string MemberEmailConfirmationCookieName = "MVCForumEmailConfirmation";

        // Cache names
        public const string SettingsCacheName = "MainSettings";
        public const string LocalizationCacheName = "Localization";
        public const string MemberCacheName = "#member#-{0}";       

        // Url names
        public const string CategoryUrlIdentifier = "cat";
        public const string TopicUrlIdentifier = "thread";
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
        public const string PermissionInsertEditorImages = "Insert Editor Images";

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
        /// Cache lengths
        /// </summary>
        public const int ShortCacheTime = 900;
        public const int LongCacheTime = 10800;

        /// <summary>
        /// Cache lengths
        /// </summary>
        public static List<string> ReflectionDllsToAvoid = new List<string>
        {
            "EcmaScript.NET.dll",
            "Unity.WebApi.dll",
            "Skybrud.Social.dll",
            "Antlr3.Runtime.dll",
            "WebGrease.dll",
            "System.Web.Optimization.dll",
            "Common.Logging.Core.dll",
            "Common.Logging.dll",
            "EntityFramework.dll",
            "EntityFramework.SqlServer.dll",
            "Newtonsoft.Json.dll",
            "Quartz.dll",
            "SquishIt.Framework.dll",
            "SquishIt.Mvc.dll",
            "ImageProcessor.Web.dll",
            "ImageProcessor.dll",
            "AntiXssLibrary.dll",
            "HtmlSanitizationLibrary.dll",
            "System.Web.Http.dll",
            "System.Net.Http.Formatting.dll",
            "System.Web.Helpers.dll",
            "System.Web.Mvc.dll",
            "System.Web.WebPages.Deployment.dll",
            "System.Web.WebPages.dll",
            "System.Web.WebPages.Razor.dll",
            "System.Web.Razor.dll",
            "Quartz.Unity.45.dll",
            "EFCache.dll",
            "HtmlAgilityPack.dll",
            "Microsoft.Practices.Unity.Configuration.dll",
            "Microsoft.Practices.Unity.dll",
            "Microsoft.Practices.Unity.RegistrationByConvention.dll",
            "Microsoft.Practices.ServiceLocation.dll",
            "Unity.WebApi.dll",
            "Unity.Mvc4.dll",
            "System.Web.Http.WebHost.dll",
            "WebActivator.dll",
            "System.Net.Http.dll",
            "System.Net.Http.WebRequest.dll",
            "AjaxMin.dll",
            "Iesi.Collections.dll",
            "Yahoo.Yui.Compressor.dll",
            "Microsoft.Web.Services3.dll",
            "Microsoft.Web.Infrastructure.dll"
        };

    }
}
