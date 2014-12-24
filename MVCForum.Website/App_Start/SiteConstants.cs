namespace MVCForum.Website
{
    public static class SiteConstants
    {
        // Paging options
        public const int AdminListPageSize = 30;
        public const int PagingGroupSize = 10;
        public const int ActiveTopicsListSize = 50;
        public const int PrivateMessageListSize = 30;
        public const int SimilarTopicsListSize = 20;

        // Last Activity Time Check
        public const int TimeSpanInMinutesToDoCheck = 10;

        // Social
        public const int GravatarPostSize = 50;
        public const int GravatarTopicSize = 35;
        public const int GravatarProfileSize = 85;
        public const int GravatarLeaderboardSize = 25;

        //Uploads
        public const string UploadFolderPath = "~/content/uploads/";

        /// <summary>
        /// A short cache time to help with speeding up the site
        /// </summary>
        public const int DefaultCacheLengthInSeconds = 600;

        // System and default folder
        public const string ThemeRootFolder = "~/Themes/";

        // Themes
        public const string ThemeRootFolderName = "Themes"; 

        // Editor
        //public const string EditorType = "bbeditor";
        //public const string EditorType = "tinymceeditor";
        public const string EditorType = "markdowneditor";

        //Querystring names
        public const string PostOrderBy = "order";
        public const string AllPosts = "all";

        //Mobile Check Name
        public const string IsMobileDevice = "IsMobileDevice";
    }
}