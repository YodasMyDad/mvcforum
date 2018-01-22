namespace MvcForum.Core.Constants
{
    public static class Constants
    {
        public const int SaltSize = 24;
        public const string EditorType = "forumeditor";

        // Scheduled Tasks
        public const string DefaultTaskGroup = "MvcForumTaskGroup";

        // Cookie names
        public const string LanguageIdCookieName = "LanguageCulture";

        public const string MemberEmailConfirmationCookieName = "MvcForumEmailConfirmation";

        // Cache names
        //TODO - Move to cache keys
        public const string LocalisationCacheName = "Localization-";

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

        //------------ End Permissions ----------

        // Paging options
        public const string PagingUrlFormat = "{0}?p={1}";

        // How long 
        public const int TimeSpanInMinutesToShowMembers = 12;

        /// <summary>
        ///     Last Activity Time Check.
        /// </summary>
        public const int TimeSpanInMinutesToDoCheck = 3;


        public const string EditorTemplateColourPicker = "colourpicker";

        //Querystring names
        public const string PostOrderBy = "order";

        public const string AllPosts = "all";

        //Mobile Check Name
        public const string IsMobileDevice = "IsMobileDevice";

        public static string LanguageStrings = string.Concat(LocalisationCacheName, "LangStrings-");

        public const string ImageExtensions = "jpg,jpeg,png,gif";

        public class ExtendedDataKeys
        {
            /// <summary>
            ///     The key thats used to pull out a guid to check email confirmation during registration
            /// </summary>
            public const string RegistrationEmailConfirmationKey = "RegistrationEmailConfirmationKey";

            /// <summary>
            ///     Gets the login type out the extended data
            /// </summary>
            public const string LoginType = "LoginType";

            /// <summary>
            ///     Key for the SocialProfileImageUrl
            /// </summary>
            public const string SocialProfileImageUrl = "SocialProfileImageUrl";

            public const string ManuallyAuthoriseMembers = "ManuallyAuthoriseMembers";
            public const string MemberEmailAuthorisationNeeded = "MemberEmailAuthorisationNeeded";
            public const string ReturnUrl = "ReturnUrl";

            public const string Username = "Username";
            public const string Password = "Password";

            public const string UserObject = "UserObject";

            public const string ImageBase64 = "ImageBase64";

            public const string ImagesBase64 = "ImagesBase64";
        }
    }
}