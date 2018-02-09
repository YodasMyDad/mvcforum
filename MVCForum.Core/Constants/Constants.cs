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

        /// <summary>
        ///  Paging format
        /// </summary>
        public const string PagingUrlFormat = "{0}?p={1}";

        /// <summary>
        /// How long in minutes to show members
        /// </summary>
        public const int TimeSpanInMinutesToShowMembers = 12;

        /// <summary>
        ///     Last Activity Time Check.
        /// </summary>
        public const int TimeSpanInMinutesToDoCheck = 3;

        /// <summary>
        /// Editor template for colour picker
        /// </summary>
        public const string EditorTemplateColourPicker = "colourpicker";

        /// <summary>
        /// Order posts querystring
        /// </summary>
        public const string PostOrderBy = "order";

        /// <summary>
        /// All posts querystring
        /// </summary>
        public const string AllPosts = "all";

        /// <summary>
        /// Mobile check key
        /// </summary>
        public const string IsMobileDevice = "IsMobileDevice";

        /// <summary>
        /// Lanaguage strings
        /// </summary>
        public static string LanguageStrings = string.Concat(LocalisationCacheName, "LangStrings-");

        /// <summary>
        /// Image extensions
        /// </summary>
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

            /// <summary>
            /// Key for whether to manually authorise members
            /// </summary>
            public const string ManuallyAuthoriseMembers = "ManuallyAuthoriseMembers";

            /// <summary>
            /// Key for MemberEmailAuthorisationNeeded
            /// </summary>
            public const string MemberEmailAuthorisationNeeded = "MemberEmailAuthorisationNeeded";

            /// <summary>
            /// Key for the return url
            /// </summary>
            public const string ReturnUrl = "ReturnUrl";

            /// <summary>
            /// Key for the username
            /// </summary>
            public const string Username = "Username";

            /// <summary>
            /// Key for the password
            /// </summary>
            public const string Password = "Password";

            /// <summary>
            /// Key for a PostedFiles
            /// </summary>
            public const string PostedFiles = "PostedFiles";

            /// <summary>
            /// Key for tags
            /// </summary>
            public const string Tags = "Tags";

            /// <summary>
            /// Key for whether username has changes
            /// </summary>
            public const string UsernameChanged = "UsernameChanged";

            /// <summary>
            /// Key for whether something needs moderating
            /// </summary>
            public const string Moderate = "Moderate";

            /// <summary>
            /// Key for the permission set
            /// </summary>
            public const string PermissionSet = "PermissionSet";

            /// <summary>
            /// Key for whether entity is being edited
            /// </summary>
            public const string IsEdit = "IsEdit";

            /// <summary>
            /// Key for whether to subscribe
            /// </summary>
            public const string Subscribe = "Subscribe";

            /// <summary>
            /// Key for getting entity content
            /// </summary>
            public const string Content = "Content";

            /// <summary>
            /// Keys for getting entity name
            /// </summary>
            public const string Name = "Name";

            /// <summary>
            /// Keys for new poll answers
            /// </summary>
            public const string PollNewAnswers = "PollNewAnswers";

            /// <summary>
            /// Key for poll close after days
            /// </summary>
            public const string PollCloseAfterDays = "PollCloseAfterDays";

            /// <summary>
            /// Key for getting a stored topic ID
            /// </summary>
            public const string TopicId = "TopicId";

            /// <summary>
            /// Key for whether to move posts
            /// </summary>
            public const string MovePosts = "MovePosts";

            /// <summary>
            /// Key to get existing post
            /// </summary>
            public const string Post = "Post";

            /// <summary>
            /// Key to whether to igniore last post
            /// </summary>
            public const string IgnoreLastPost = "IgnoreLastPost";

            /// <summary>
            /// Parent Category
            /// </summary>
            public const string ParentCategory = "ParentCategory";

            /// <summary>
            /// Section 
            /// </summary>
            public const string Section = "Section";
        }
    }
}