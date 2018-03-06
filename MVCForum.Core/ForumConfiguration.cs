namespace MvcForum.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces.Services;
    using Ioc;
    using Unity;

    public class ForumConfiguration
    {
        private string _mvcForumVersion;

        public string MvcForumVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_mvcForumVersion))
                {
                    _mvcForumVersion = GetConfig("MvcForumVersion");
                }
                return _mvcForumVersion;
            }
        }


        // This is just the initial standard role
        public string StandardMembers => GetConfig("StandardMembers");

        /// <summary>
        ///     Social Login Keys
        /// </summary>
        public string FacebookAppId => GetConfig("FacebookAppId");

        public string FacebookAppSecret => GetConfig("FacebookAppSecret");
        public string MicrosoftAppId => GetConfig("MicrosoftAppId");
        public string MicrosoftAppSecret => GetConfig("MicrosoftAppSecret");
        public string GooglePlusAppId => GetConfig("GooglePlusAppId");
        public string GooglePlusAppSecret => GetConfig("GooglePlusAppSecret");

        /// <summary>
        ///     File Upload Settings
        /// </summary>
        public string FileUploadAllowedExtensions => GetConfig("FileUploadAllowedExtensions");

        public string FileUploadMaximumFileSizeInBytes => GetConfig("FileUploadMaximumFileSizeInBytes");
        public string UploadFolderPath => GetConfig("UploadFolderPath");

        public int PrivateMessageWarningAmountLessThanAllowedSize => Convert.ToInt32(GetConfig("PrivateMessageWarningAmountLessThanAllowedSize"));
        public int LogFileMaxSizeBytes => Convert.ToInt32(GetConfig("LogFileMaxSizeBytes"));

        /// <summary>
        ///     Paging options - Amount per page on different pages.
        /// </summary>
        public int PagingGroupSize => Convert.ToInt32(GetConfig("PagingGroupSize"));

        public int AdminListPageSize => Convert.ToInt32(GetConfig("AdminListPageSize"));
        public int ActiveTopicsListSize => Convert.ToInt32(GetConfig("ActiveTopicsListSize"));
        public int SearchListSize => Convert.ToInt32(GetConfig("SearchListSize"));
        public int MembersActivityListSize => Convert.ToInt32(GetConfig("MembersActivityListSize"));
        public int PrivateMessageListSize => Convert.ToInt32(GetConfig("PrivateMessageListSize"));
        public int SimilarTopicsListSize => Convert.ToInt32(GetConfig("SimilarTopicsListSize"));

        /// <summary>
        ///     Post Settings
        /// </summary>
        public bool IncludeFullPostInEmailNotifications =>
            Convert.ToBoolean(GetConfig("IncludeFullPostInEmailNotifications"));

        public string BannedWordReplaceCharactor => GetConfig("BannedWordReplaceCharactor");

        public int PostSecondsWaitBeforeNewPost => Convert.ToInt32(GetConfig("PostSecondsWaitBeforeNewPost"));

        /// <summary>
        ///     Registration Settings
        /// </summary>
        public bool AutoLoginAfterRegister => Convert.ToBoolean(GetConfig("AutoLoginAfterRegister"));

        /// <summary>
        ///     Social Gravatar size
        /// </summary>
        public int GravatarPostSize => Convert.ToInt32(GetConfig("GravatarPostSize"));

        public int GravatarTopicSize => Convert.ToInt32(GetConfig("GravatarTopicSize"));
        public int GravatarProfileSize => Convert.ToInt32(GetConfig("GravatarProfileSize"));
        public int GravatarLeaderboardSize => Convert.ToInt32(GetConfig("GravatarLeaderboardSize"));
        public int GravatarLikedBySize => Convert.ToInt32(GetConfig("GravatarLikedBySize"));
        public int GravatarLatestBySize => Convert.ToInt32(GetConfig("GravatarLatestBySize"));
        public int GravatarFooterSize => Convert.ToInt32(GetConfig("GravatarFooterSize"));

        // Url names
        public string CategoryUrlIdentifier => GetConfig("CategoryUrlIdentifier");

        public string TopicUrlIdentifier => GetConfig("TopicUrlIdentifier");
        public string TagsUrlIdentifier => GetConfig("TagsUrlIdentifier");
        public string MemberUrlIdentifier => GetConfig("MemberUrlIdentifier");

        /// <summary>
        ///     Which Editor the site should use
        /// </summary>
        public string ChosenEditor => GetConfig("EditorType");


        /// <summary>
        ///     These are the permission names used as keys to return them
        ///     So they must be the same as the database value
        /// </summary>

        // Category Permissions
        public string PermissionReadOnly => GetConfig("PermissionReadOnly");

        public string PermissionDeletePosts => GetConfig("PermissionDeletePosts");
        public string PermissionEditPosts => GetConfig("PermissionEditPosts");
        public string PermissionCreateStickyTopics => GetConfig("PermissionCreateStickyTopics");
        public string PermissionDenyAccess => GetConfig("PermissionDenyAccess");
        public string PermissionLockTopics => GetConfig("PermissionLockTopics");
        public string PermissionVoteInPolls => GetConfig("PermissionVoteInPolls");
        public string PermissionCreatePolls => GetConfig("PermissionCreatePolls");
        public string PermissionCreateTopics => GetConfig("PermissionCreateTopics");
        public string PermissionAttachFiles => GetConfig("PermissionAttachFiles");
        public string PermissionCreateTags => GetConfig("PermissionCreateTags");

        // Global Permissions
        public string PermissionEditMembers => GetConfig("PermissionEditMembers");

        public string PermissionInsertEditorImages => GetConfig("PermissionInsertEditorImages");

        // Database Connection Key
        public string MvcForumContext => GetConfig("MvcForumContext");

        // Default Theme folder
        public string ThemeRootFolder => GetConfig("ThemeRootFolder");

        // Themes
        public string ThemeRootFolderName => GetConfig("ThemeRootFolderName");

        /// <summary>
        ///     Show categories on home page instead of topics
        /// </summary>
        public string ForumIndexView => GetConfig("ForumIndexView");


        /// <summary>
        /// Plugin locations
        /// </summary>
        private IList<string> _pluginSearchLocations;
        public IList<string> PluginSearchLocations
        {
            get
            {
                if (_pluginSearchLocations == null)
                {
                    var pluginStringLocations = GetConfig("PluginSearchLocations");
                    if (!string.IsNullOrWhiteSpace(pluginStringLocations))
                    {
                        _pluginSearchLocations = ConfigToListString(pluginStringLocations);
                    }
                }
                return _pluginSearchLocations;                
            }
        }

        /// <summary>
        /// Get the storage provider for the website
        /// </summary>
        public string StorageProviderType => GetPlugin("StorageProviderType");

        /// <summary>
        /// Get a list of badges
        /// </summary>
        private IList<string> _badges;
        public IList<string> Badges
        {
            get
            {
                if (_badges == null)
                {
                    var allBadges = GetPlugin("Badges");
                    if (!string.IsNullOrWhiteSpace(allBadges))
                    {
                        _badges = ConfigToListString(allBadges);
                    }
                }
                return _badges;
            }
        }

        /// <summary>
        /// Gets the User Create Pipes from the config
        /// </summary>
        private IList<string> _userLoginPipes;
        public IList<string> PipelinesUserLogin
        {
            get
            {
                if (_userLoginPipes == null)
                {
                    var pipes = GetPlugin("PipelinesUserLogin");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _userLoginPipes = ConfigToListString(pipes);
                    }
                }
                return _userLoginPipes;
            }
        }

        /// <summary>
        /// Gets the User Create Pipes from the config
        /// </summary>
        private IList<string> _userCreatePipes;
        public IList<string> PipelinesUserCreate
        {
            get
            {
                if (_userCreatePipes == null)
                {
                    var pipes = GetPlugin("PipelinesUserCreate");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _userCreatePipes = ConfigToListString(pipes);
                    }
                }
                return _userCreatePipes;
            }
        }

        /// <summary>
        /// Gets the User Edit Pipes from the config
        /// </summary>
        private IList<string> _userEditPipes;
        public IList<string> PipelinesUserUpdate
        {
            get
            {
                if (_userEditPipes == null)
                {
                    var pipes = GetPlugin("PipelinesUserUpdate");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _userEditPipes = ConfigToListString(pipes);
                    }
                }
                return _userEditPipes;
            }
        }

        /// <summary>
        /// Gets the User Delete Pipes from the config
        /// </summary>
        private IList<string> _userDeletePipes;
        public IList<string> PipelinesUserDelete
        {
            get
            {
                if (_userDeletePipes == null)
                {
                    var pipes = GetPlugin("PipelinesUserDelete");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _userDeletePipes = ConfigToListString(pipes);
                    }
                }
                return _userDeletePipes;
            }
        }

        /// <summary>
        /// Gets the User Scrub Pipes from the config
        /// </summary>
        private IList<string> _userScrubPipes;
        public IList<string> PipelinesUserScrub
        {
            get
            {
                if (_userScrubPipes == null)
                {
                    var pipes = GetPlugin("PipelinesUserScrub");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _userScrubPipes = ConfigToListString(pipes);
                    }
                }
                return _userScrubPipes;
            }
        }

        /// <summary>
        /// Gets the Topic Create Pipes from the config
        /// </summary>
        private IList<string> _pipelineTopicCreate;
        public IList<string> PipelinesTopicCreate
        {
            get
            {
                if (_pipelineTopicCreate == null)
                {
                    var pipes = GetPlugin("PipelinesTopicCreate");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelineTopicCreate = ConfigToListString(pipes);
                    }
                }
                return _pipelineTopicCreate;
            }
        }

        /// <summary>
        /// Gets the pipes for the topic update
        /// </summary>
        private IList<string> _pipelineTopicUpdate;
        public IList<string> PipelinesTopicUpdate
        {
            get
            {
                if (_pipelineTopicUpdate == null)
                {
                    var pipes = GetPlugin("PipelinesTopicUpdate");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelineTopicUpdate = ConfigToListString(pipes);
                    }
                }
                return _pipelineTopicUpdate;
            }
        }

        /// <summary>
        /// Gets the pipes for the topic delete
        /// </summary>
        private IList<string> _pipelineTopicDelete;
        public IList<string> PipelinesTopicDelete
        {
            get
            {
                if (_pipelineTopicDelete == null)
                {
                    var pipes = GetPlugin("PipelinesTopicDelete");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelineTopicDelete = ConfigToListString(pipes);
                    }
                }
                return _pipelineTopicDelete;
            }
        }

        /// <summary>
        /// Gets the Post Create Pipes from the config
        /// </summary>
        private IList<string> _pipelinePostCreate;
        public IList<string> PipelinesPostCreate
        {
            get
            {
                if (_pipelinePostCreate == null)
                {
                    var pipes = GetPlugin("PipelinesPostCreate");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelinePostCreate = ConfigToListString(pipes);
                    }
                }
                return _pipelinePostCreate;
            }
        }

        /// <summary>
        /// Gets the pipes for the Post update
        /// </summary>
        private IList<string> _pipelinePostUpdate;
        public IList<string> PipelinesPostUpdate
        {
            get
            {
                if (_pipelinePostUpdate == null)
                {
                    var pipes = GetPlugin("PipelinesPostUpdate");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelinePostUpdate = ConfigToListString(pipes);
                    }
                }
                return _pipelinePostUpdate;
            }
        }

        /// <summary>
        /// Gets the pipes for the Post update
        /// </summary>
        private IList<string> _pipelinePostMove;
        public IList<string> PipelinesPostMove
        {
            get
            {
                if (_pipelinePostMove == null)
                {
                    var pipes = GetPlugin("PipelinesPostMove");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelinePostMove = ConfigToListString(pipes);
                    }
                }
                return _pipelinePostMove;
            }
        }

        /// <summary>
        /// Gets the pipes for the Post delete
        /// </summary>
        private IList<string> _pipelinePostDelete;
        public IList<string> PipelinesPostDelete
        {
            get
            {
                if (_pipelinePostDelete == null)
                {
                    var pipes = GetPlugin("PipelinesPostDelete");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelinePostDelete = ConfigToListString(pipes);
                    }
                }
                return _pipelinePostDelete;
            }
        }

        /// <summary>
        /// Gets the Category Create Pipes from the config
        /// </summary>
        private IList<string> _pipelineCategoryCreate;
        public IList<string> PipelinesCategoryCreate
        {
            get
            {
                if (_pipelineCategoryCreate == null)
                {
                    var pipes = GetPlugin("PipelinesCategoryCreate");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelineCategoryCreate = ConfigToListString(pipes);
                    }
                }
                return _pipelineCategoryCreate;
            }
        }

        /// <summary>
        /// Gets the pipes for the Category update
        /// </summary>
        private IList<string> _pipelineCategoryUpdate;
        public IList<string> PipelinesCategoryUpdate
        {
            get
            {
                if (_pipelineCategoryUpdate == null)
                {
                    var pipes = GetPlugin("PipelinesCategoryUpdate");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelineCategoryUpdate = ConfigToListString(pipes);
                    }
                }
                return _pipelineCategoryUpdate;
            }
        }

        /// <summary>
        /// Gets the pipes for the Category delete
        /// </summary>
        private IList<string> _pipelineCategoryDelete;
        public IList<string> PipelinesCategoryDelete
        {
            get
            {
                if (_pipelineCategoryDelete == null)
                {
                    var pipes = GetPlugin("PipelinesCategoryDelete");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelineCategoryDelete = ConfigToListString(pipes);
                    }
                }
                return _pipelineCategoryDelete;
            }
        }

        /// <summary>
        /// Gets the pipes for the points delete
        /// </summary>
        private IList<string> _pipelinePointsDelete;
        public IList<string> PipelinesPointsDelete
        {
            get
            {
                if (_pipelinePointsDelete == null)
                {
                    var pipes = GetPlugin("PipelinesPointsDelete");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelinePointsDelete = ConfigToListString(pipes);
                    }
                }
                return _pipelinePointsDelete;
            }
        }

        /// <summary>
        /// Gets the pipes for the points create
        /// </summary>
        private IList<string> _pipelinePointsCreate;
        public IList<string> PipelinesPointsCreate
        {
            get
            {
                if (_pipelinePointsCreate == null)
                {
                    var pipes = GetPlugin("PipelinesPointsCreate");
                    if (!string.IsNullOrWhiteSpace(pipes))
                    {
                        _pipelinePointsCreate = ConfigToListString(pipes);
                    }
                }
                return _pipelinePointsCreate;
            }
        }

        /// <summary>
        /// Turns a string config into a list
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private static IList<string> ConfigToListString(string config)
        {
            return config.TrimStart(',')
                        .TrimEnd(',')
                        .Split(',')
                        .Select(x => x.Trim())
                        .ToList();
        }

        #region Singleton

        private static ForumConfiguration _instance;
        private static readonly object InstanceLock = new object();
        private static IConfigService _configService;

        private ForumConfiguration(IConfigService configService)
        {
            _configService = configService;
        }

        public static ForumConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                        {
                            var configService = UnityHelper.Container.Resolve<IConfigService>();
                            _instance = new ForumConfiguration(configService);
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Generic Get

        /// <summary>
        ///     This is the generic get config method, you can use this to also get custom config items out
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConfig(string key)
        {
            var dict = _configService.GetForumConfig();
            if (!string.IsNullOrWhiteSpace(key) && dict.ContainsKey(key))
            {
                return dict[key];
            }
            return string.Empty;
        }

        public string GetPlugin(string key)
        {
            var dict = _configService.GetPlugins();
            if (!string.IsNullOrWhiteSpace(key) && dict.ContainsKey(key))
            {
                return dict[key];
            }
            return string.Empty;
        }

        #endregion
    }
}