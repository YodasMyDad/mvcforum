namespace MVCForum.Website
{
    public static partial class SiteConstants
    {
        /// <summary>
        /// Social Login Keys
        /// </summary>
        public const string FacebookAppId = "";
        public const string FacebookAppSecret = "";
        public const string GooglePlusAppId = "";
        public const string GooglePlusAppSecret = "";

        /// <summary>
        /// File Upload Settings
        /// </summary>
        public const string FileUploadAllowedExtensions = "";
        public const string FileUploadMaximumFileSizeInBytes = "";

        /// <summary>
        /// Banned work replacement character
        /// </summary>
        public const string BannedWordReplaceCharactor = "";

        /// <summary>
        /// Paging options - Amount per page on different pages.
        /// </summary>
        public const int AdminListPageSize = 30;
        public const int PagingGroupSize = 10;
        public const int ActiveTopicsListSize = 100;
        public const int SearchListSize = 30;
        public const int MembersActivityListSize = 100;
        public const int PrivateMessageListSize = 30;
        public const int SimilarTopicsListSize = 20;

        /// <summary>
        /// Last Activity Time Check. 
        /// </summary>
        public const int TimeSpanInMinutesToDoCheck = 10;

        /// <summary>
        /// Social Gravatar size
        /// </summary>
        public const int GravatarPostSize = 50;
        public const int GravatarTopicSize = 50;
        public const int GravatarProfileSize = 85;
        public const int GravatarLeaderboardSize = 20;
        public const int GravatarLikedBySize = 20;
        public const int GravatarFooterSize = 30;

        /// <summary>
        /// Default Upload Folder Path
        /// </summary>
        public const string UploadFolderPath = "~/content/uploads/";

        /// <summary>
        /// A short cache time to help with speeding up the site
        /// </summary>
        public const int DefaultCacheLengthInSeconds = 1200;
        
        /// <summary>
        /// Which Editor the site should use
        /// </summary>
        public const string EditorType = "tinymceeditor";
        //public const string EditorType = "bbeditor";
        //public const string EditorType = "markdowneditor";
    }
}