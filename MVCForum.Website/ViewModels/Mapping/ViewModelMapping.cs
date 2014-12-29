using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.ViewModels.Mapping
{
    public static class ViewModelMapping
    {
        #region Membership

        public static SingleMemberListViewModel UserToSingleMemberListViewModel(MembershipUser user)
        {
            var viewModel = new SingleMemberListViewModel
            {
                IsApproved = user.IsApproved,
                Id = user.Id,
                IsLockedOut = user.IsLockedOut,
                Roles = user.Roles.Select(x => x.RoleName).ToArray(),
                UserName = user.UserName
            };
            return viewModel;
        }

        public static MemberEditViewModel UserToMemberEditViewModel(MembershipUser user)
        {
            var viewModel = new MemberEditViewModel
            {
                IsApproved = user.IsApproved,
                Id = user.Id,
                IsLockedOut = user.IsLockedOut,
                Roles = user.Roles.Select(x => x.RoleName).ToArray(),
                UserName = user.UserName,
                Age = user.Age,
                Comment = user.Comment,
                Email = user.Email,
                Facebook = user.Facebook,
                Location = user.Location,
                PasswordAnswer = user.PasswordAnswer,
                PasswordQuestion = user.PasswordQuestion,
                Signature = user.Signature,
                Twitter = user.Twitter,
                Website = user.Website,
                DisableEmailNotifications = (user.DisableEmailNotifications == true),
                DisablePosting = (user.DisablePosting == true),
                DisablePrivateMessages = (user.DisablePrivateMessages == true),
                DisableFileUploads = (user.DisableFileUploads == true),
                Avatar = user.Avatar
            };
            return viewModel;
        }

        public static RoleViewModel RoleToRoleViewModel(MembershipRole role)
        {
            var viewModel = new RoleViewModel
            {
                Id = role.Id,
                RoleName = role.RoleName
            };
            return viewModel;
        }

        public static MembershipRole RoleViewModelToRole(RoleViewModel roleViewModel)
        {
            var viewModel = new MembershipRole
            {
                RoleName = roleViewModel.RoleName
            };
            return viewModel;
        }

        #endregion

        #region Settings
        public static Settings SettingsViewModelToSettings(EditSettingsViewModel settingsViewModel, Settings existingSettings)
        {
            existingSettings.Id = settingsViewModel.Id;
            existingSettings.ForumName = settingsViewModel.ForumName;
            existingSettings.ForumUrl = settingsViewModel.ForumUrl;
            existingSettings.IsClosed = settingsViewModel.IsClosed;
            existingSettings.EnableRSSFeeds = settingsViewModel.EnableRSSFeeds;
            existingSettings.DisplayEditedBy = settingsViewModel.DisplayEditedBy;
            existingSettings.EnableMarkAsSolution = settingsViewModel.EnableMarkAsSolution;
            existingSettings.EnableSpamReporting = settingsViewModel.EnableSpamReporting;
            existingSettings.EnableMemberReporting = settingsViewModel.EnableMemberReporting;
            existingSettings.EnableEmailSubscriptions = settingsViewModel.EnableEmailSubscriptions;
            existingSettings.ManuallyAuthoriseNewMembers = settingsViewModel.ManuallyAuthoriseNewMembers;
            existingSettings.EmailAdminOnNewMemberSignUp = settingsViewModel.EmailAdminOnNewMemberSignUp;
            existingSettings.TopicsPerPage = settingsViewModel.TopicsPerPage;
            existingSettings.PostsPerPage = settingsViewModel.PostsPerPage;
            existingSettings.ActivitiesPerPage = settingsViewModel.ActivitiesPerPage;
            existingSettings.EnablePrivateMessages = settingsViewModel.EnablePrivateMessages;
            existingSettings.MaxPrivateMessagesPerMember = settingsViewModel.MaxPrivateMessagesPerMember;
            existingSettings.PrivateMessageFloodControl = settingsViewModel.PrivateMessageFloodControl;
            existingSettings.EnableSignatures = settingsViewModel.EnableSignatures;
            existingSettings.EnablePoints = settingsViewModel.EnablePoints;
            existingSettings.PointsAllowedToVoteAmount = settingsViewModel.PointsAllowedToVoteAmount;
            existingSettings.PointsAddedPerPost = settingsViewModel.PointsAddedPerPost;
            existingSettings.PointsAddedPostiveVote = settingsViewModel.PointsAddedPostiveVote;
            existingSettings.PointsDeductedNagativeVote = settingsViewModel.PointsDeductedNagativeVote;
            existingSettings.PointsAddedForSolution = settingsViewModel.PointsAddedForSolution;
            existingSettings.AdminEmailAddress = settingsViewModel.AdminEmailAddress;
            existingSettings.NotificationReplyEmail = settingsViewModel.NotificationReplyEmail;
            existingSettings.SMTP = settingsViewModel.SMTP;
            existingSettings.SMTPUsername = settingsViewModel.SMTPUsername;
            existingSettings.SMTPPassword = settingsViewModel.SMTPPassword;
            existingSettings.Theme = settingsViewModel.Theme;
            existingSettings.AkismentKey = settingsViewModel.AkismentKey;
            existingSettings.EnableAkisment = settingsViewModel.EnableAkisment;
            existingSettings.SMTPPort = settingsViewModel.SMTPPort.ToString();
            existingSettings.SpamQuestion = settingsViewModel.SpamQuestion;
            existingSettings.SpamAnswer = settingsViewModel.SpamAnswer;
            existingSettings.SMTPEnableSSL = settingsViewModel.SMTPEnableSSL;
            existingSettings.EnableSocialLogins = settingsViewModel.EnableSocialLogins;
            existingSettings.EnablePolls = settingsViewModel.EnablePolls;
            existingSettings.SuspendRegistration = settingsViewModel.SuspendRegistration;
            existingSettings.NewMemberEmailConfirmation = settingsViewModel.NewMemberEmailConfirmation;
            existingSettings.PageTitle = settingsViewModel.PageTitle;
            existingSettings.MetaDesc = settingsViewModel.MetaDesc;
            return existingSettings;
        }

        public static EditSettingsViewModel SettingsToSettingsViewModel(Settings currentSettings)
        {
            var settingViewModel = new EditSettingsViewModel
            {
                Id = currentSettings.Id,
                ForumName = currentSettings.ForumName,
                ForumUrl = currentSettings.ForumUrl,
                IsClosed = currentSettings.IsClosed,
                EnableRSSFeeds = currentSettings.EnableRSSFeeds,
                DisplayEditedBy = currentSettings.DisplayEditedBy,
                EnableMarkAsSolution = currentSettings.EnableMarkAsSolution,
                EnableSpamReporting = currentSettings.EnableSpamReporting,
                EnableMemberReporting = currentSettings.EnableMemberReporting,
                EnableEmailSubscriptions = currentSettings.EnableEmailSubscriptions,
                ManuallyAuthoriseNewMembers = currentSettings.ManuallyAuthoriseNewMembers,
                EmailAdminOnNewMemberSignUp = currentSettings.EmailAdminOnNewMemberSignUp,
                TopicsPerPage = currentSettings.TopicsPerPage,
                PostsPerPage = currentSettings.PostsPerPage,
                ActivitiesPerPage = currentSettings.ActivitiesPerPage,
                EnablePrivateMessages = currentSettings.EnablePrivateMessages,
                MaxPrivateMessagesPerMember = currentSettings.MaxPrivateMessagesPerMember,
                PrivateMessageFloodControl = currentSettings.PrivateMessageFloodControl,
                EnableSignatures = currentSettings.EnableSignatures,
                EnablePoints = currentSettings.EnablePoints,
                PointsAllowedToVoteAmount = currentSettings.PointsAllowedToVoteAmount,
                PointsAddedPerPost = currentSettings.PointsAddedPerPost,
                PointsAddedPostiveVote = currentSettings.PointsAddedPostiveVote,
                PointsDeductedNagativeVote = currentSettings.PointsDeductedNagativeVote,
                PointsAddedForSolution = currentSettings.PointsAddedForSolution,
                AdminEmailAddress = currentSettings.AdminEmailAddress,
                NotificationReplyEmail = currentSettings.NotificationReplyEmail,
                SMTP = currentSettings.SMTP,
                SMTPUsername = currentSettings.SMTPUsername,
                SMTPPassword = currentSettings.SMTPPassword,
                AkismentKey = currentSettings.AkismentKey,
                EnableAkisment = currentSettings.EnableAkisment != null && (bool)currentSettings.EnableAkisment,
                NewMemberEmailConfirmation = currentSettings.NewMemberEmailConfirmation != null && (bool)currentSettings.NewMemberEmailConfirmation,
                Theme = currentSettings.Theme,
                SMTPPort = string.IsNullOrEmpty(currentSettings.SMTPPort) ? null : (int?)(Convert.ToInt32(currentSettings.SMTPPort)),
                SpamQuestion = currentSettings.SpamQuestion,
                SpamAnswer = currentSettings.SpamAnswer,
                Themes = AppHelpers.GetThemeFolders(),
                SMTPEnableSSL = currentSettings.SMTPEnableSSL ?? false,
                EnableSocialLogins = currentSettings.EnableSocialLogins ?? false,
                EnablePolls = currentSettings.EnablePolls ?? false,
                SuspendRegistration = currentSettings.SuspendRegistration ?? false,
                PageTitle = currentSettings.PageTitle,
                MetaDesc = currentSettings.MetaDesc
            };

            return settingViewModel;
        }        
        #endregion

        #region Topics
        public static Dictionary<Category, PermissionSet> GetPermissionsForTopics(IEnumerable<Topic> topics, IRoleService roleService, MembershipRole usersRole)
        {
            // Get all the categories for this topic collection
            var categories = topics.Select(x => x.Category).Distinct();

            // Permissions
            // loop through the categories and get the permissions
            var permissions = new Dictionary<Category, PermissionSet>();
            foreach (var category in categories)
            {
                var permissionSet = roleService.GetPermissions(category, usersRole);
                permissions.Add(category, permissionSet);
            }
            return permissions;
        }

        public static List<TopicViewModel> CreateTopicViewModels(List<Topic> topics, 
                                                                IRoleService roleService, 
                                                                MembershipRole usersRole,
                                                                MembershipUser loggedOnUser,
                                                                ITopicNotificationService topicNotificationService,
                                                                IPollAnswerService pollAnswerService)
        {
            // Get Permissions
            var permissions = GetPermissionsForTopics(topics, roleService, usersRole);
            var viewModels = new List<TopicViewModel>();
            foreach (var topic in topics)
            {
                var permission = permissions[topic.Category];
                viewModels.Add(CreateTopicViewModel(topic, permission, topic.Posts.ToList(), null, null, null, null, loggedOnUser, topicNotificationService, pollAnswerService));
            }
            return viewModels;
        }

        public static TopicViewModel CreateTopicViewModel(Topic topic,
                                                    PermissionSet permission,
                                                    List<Post> posts,
                                                    Post starterPost,
                                                    int? pageIndex,
                                                    int? totalCount,
                                                    int? totalPages,
                                                    MembershipUser loggedOnUser,
                                                    ITopicNotificationService topicNotificationService,
                                                    IPollAnswerService pollAnswerService,
                                                    bool getExtendedData = false)
        {
            var userIsAuthenticated = loggedOnUser != null;
            var viewModel = new TopicViewModel
            {
                Permissions = permission,
                Topic = topic,
                Views = topic.Views,
                DisablePosting = loggedOnUser != null && (loggedOnUser.DisablePosting == true),
                PageIndex = pageIndex,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            if (starterPost == null)
            {
                starterPost = posts.FirstOrDefault(x => x.IsTopicStarter);
            }
            viewModel.StarterPost = CreatePostViewModel(starterPost, permission, topic);
            viewModel.VotesUp = starterPost.Votes.Count(x => x.Amount > 0);
            viewModel.VotesDown = starterPost.Votes.Count(x => x.Amount < 0);
            viewModel.Answers = totalCount != null ? (int)totalCount : posts.Count() - 1;
            viewModel.Posts = CreatePostViewModels(posts, permission, topic);

            // ########### Full topic need everything   

            if (getExtendedData)
            {
                // See if the user has subscribed to this topic or not
                var isSubscribed = userIsAuthenticated && (topicNotificationService.GetByUserAndTopic(loggedOnUser, topic).Any());
                viewModel.IsSubscribed = isSubscribed;

                // See if the topic has a poll, and if so see if this user viewing has already voted
                if (topic.Poll != null)
                {
                    // There is a poll and a user
                    // see if the user has voted or not

                    viewModel.Poll = new PollViewModel
                    {
                        Poll = topic.Poll,
                        UserAllowedToVote = permission[AppConstants.PermissionVoteInPolls].IsTicked
                    };

                    var answers = pollAnswerService.GetAllPollAnswersByPoll(topic.Poll);
                    if (answers.Any())
                    {
                        var votes = answers.SelectMany(x => x.PollVotes).ToList();
                        if (userIsAuthenticated)
                        {
                            viewModel.Poll.UserHasAlreadyVoted = (votes.Count(x => x.User.Id == loggedOnUser.Id) > 0);
                        }
                        viewModel.Poll.TotalVotesInPoll = votes.Count();
                    }
                }
            }

            return viewModel;
        }
        #endregion

        #region Post
        public static PostViewModel CreatePostViewModel(Post post, PermissionSet permission, Topic topic)
        {
            return new PostViewModel
            {
                Permissions = permission,
                Post = post,
                ParentTopic = topic
            };
        }
        public static List<PostViewModel> CreatePostViewModels(IEnumerable<Post> posts, PermissionSet permission, Topic topic)
        {
            var viewModels = new List<PostViewModel>();
            foreach (var post in posts)
            {
                viewModels.Add(CreatePostViewModel(post, permission, topic));
            }
            return viewModels;
        }
        #endregion

        #region Category

        #endregion
    }
}