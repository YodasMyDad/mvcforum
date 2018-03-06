namespace MvcForum.Core.Services.Migrations
{
    using System;
    using System.Configuration;
    using System.Data.Entity.Migrations;
    using System.Data.SqlClient;

    public partial class InitialCommit : DbMigration
    {
        /// <summary>
        /// Method to see if a table exists or not
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private bool TableExists(string table)
        {
            const string command = "select * from sys.tables";
            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings[ForumConfiguration.Instance.MvcForumContext].ConnectionString))
            using (var com = new SqlCommand(command, con))
            {
                con.Open();
                var reader = com.ExecuteReader();
                while (reader.Read())
                {
                    if (string.Equals(reader.GetString(0), table, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                }
                reader.Close();
                con.Close();
            }
            return false;
        }

        public override void Up()
        {
            // **NOTE**
            // This here yo, this here, is a damn dirty hack.
            // As I cannot for the life of me get Entity Framework to play nice with upgrades
            // in this refactored application. So this will have to do right now. If you stumble on this, and read this
            // and have a better way of doing this then I'm all ears. If I don't have this, I just constantly get an error for upgraded forums
            // which something like 'Activity table already exists'
            if (!TableExists("Activity"))
            {
                try
                {
                    CreateTable("dbo.Activity",
                            c => new
                            {
                                Id = c.Guid(false),
                                Type = c.String(false, 50),
                                Data = c.String(false),
                                Timestamp = c.DateTime(false)
                            })
                        .PrimaryKey(t => t.Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Badge",
                            c => new
                            {
                                Id = c.Guid(false),
                                Type = c.String(false, 50),
                                Name = c.String(false, 50),
                                DisplayName = c.String(false, 50),
                                Description = c.String(),
                                Image = c.String(maxLength: 50),
                                AwardsPoints = c.Int()
                            })
                        .PrimaryKey(t => t.Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.MembershipUser",
                            c => new
                            {
                                Id = c.Guid(false),
                                UserName = c.String(false, 150),
                                Password = c.String(false, 128),
                                PasswordSalt = c.String(maxLength: 128),
                                Email = c.String(maxLength: 256),
                                PasswordQuestion = c.String(maxLength: 256),
                                PasswordAnswer = c.String(maxLength: 256),
                                IsApproved = c.Boolean(false),
                                IsLockedOut = c.Boolean(false),
                                IsBanned = c.Boolean(false),
                                CreateDate = c.DateTime(false),
                                LastLoginDate = c.DateTime(false),
                                LastPasswordChangedDate = c.DateTime(false),
                                LastLockoutDate = c.DateTime(false),
                                LastActivityDate = c.DateTime(),
                                FailedPasswordAttemptCount = c.Int(false),
                                FailedPasswordAnswerAttempt = c.Int(false),
                                PasswordResetToken = c.String(maxLength: 150),
                                PasswordResetTokenCreatedAt = c.DateTime(),
                                Comment = c.String(),
                                Slug = c.String(false, 150),
                                Signature = c.String(maxLength: 1000),
                                Age = c.Int(),
                                Location = c.String(maxLength: 100),
                                Website = c.String(maxLength: 100),
                                Twitter = c.String(maxLength: 60),
                                Facebook = c.String(maxLength: 60),
                                Avatar = c.String(maxLength: 500),
                                FacebookAccessToken = c.String(maxLength: 300),
                                FacebookId = c.Long(),
                                TwitterAccessToken = c.String(maxLength: 300),
                                TwitterId = c.String(maxLength: 150),
                                GoogleAccessToken = c.String(maxLength: 300),
                                GoogleId = c.String(maxLength: 150),
                                MicrosoftAccessToken = c.String(maxLength: 450),
                                MicrosoftId = c.String(),
                                IsExternalAccount = c.Boolean(),
                                TwitterShowFeed = c.Boolean(),
                                LoginIdExpires = c.DateTime(),
                                MiscAccessToken = c.String(maxLength: 250),
                                DisableEmailNotifications = c.Boolean(),
                                DisablePosting = c.Boolean(),
                                DisablePrivateMessages = c.Boolean(),
                                DisableFileUploads = c.Boolean(),
                                HasAgreedToTermsAndConditions = c.Boolean(),
                                Latitude = c.String(maxLength: 40),
                                Longitude = c.String(maxLength: 40)
                            })
                        .PrimaryKey(t => t.Id)
                        .Index(t => t.UserName, unique: true, name: "IX_MembershipUser_UserName")
                        .Index(t => t.Slug, unique: true, name: "IX_MembershipUser_Slug");
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.BadgeTypeTimeLastChecked",
                            c => new
                            {
                                Id = c.Guid(false),
                                BadgeType = c.String(false, 50),
                                TimeLastChecked = c.DateTime(false),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Block",
                            c => new
                            {
                                Id = c.Guid(false),
                                Date = c.DateTime(false),
                                Blocked_Id = c.Guid(false),
                                Blocker_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.MembershipUser", t => t.Blocked_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.Blocker_Id)
                        .Index(t => t.Blocked_Id)
                        .Index(t => t.Blocker_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.CategoryNotification",
                            c => new
                            {
                                Id = c.Guid(false),
                                Category_Id = c.Guid(false),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Category", t => t.Category_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.Category_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Category",
                            c => new
                            {
                                Id = c.Guid(false),
                                Name = c.String(false, 450),
                                Description = c.String(),
                                IsLocked = c.Boolean(false),
                                ModerateTopics = c.Boolean(false),
                                ModeratePosts = c.Boolean(false),
                                SortOrder = c.Int(false),
                                DateCreated = c.DateTime(false),
                                Slug = c.String(false, 450),
                                PageTitle = c.String(maxLength: 80),
                                Path = c.String(maxLength: 2500),
                                MetaDescription = c.String(maxLength: 200),
                                Colour = c.String(maxLength: 50),
                                Image = c.String(maxLength: 200),
                                Category_Id = c.Guid()
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Category", t => t.Category_Id)
                        .Index(t => t.Slug, unique: true, name: "IX_Category_Slug")
                        .Index(t => t.Category_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.CategoryPermissionForRole",
                            c => new
                            {
                                Id = c.Guid(false),
                                IsTicked = c.Boolean(false),
                                Category_Id = c.Guid(false),
                                MembershipRole_Id = c.Guid(false),
                                Permission_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Category", t => t.Category_Id)
                        .ForeignKey("dbo.MembershipRole", t => t.MembershipRole_Id)
                        .ForeignKey("dbo.Permission", t => t.Permission_Id)
                        .Index(t => t.Category_Id)
                        .Index(t => t.MembershipRole_Id)
                        .Index(t => t.Permission_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.MembershipRole",
                            c => new
                            {
                                Id = c.Guid(false),
                                RoleName = c.String(false, 256)
                            })
                        .PrimaryKey(t => t.Id);

                    CreateTable(
                            "dbo.GlobalPermissionForRole",
                            c => new
                            {
                                Id = c.Guid(false),
                                IsTicked = c.Boolean(false),
                                MembershipRole_Id = c.Guid(false),
                                Permission_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.MembershipRole", t => t.MembershipRole_Id)
                        .ForeignKey("dbo.Permission", t => t.Permission_Id)
                        .Index(t => t.MembershipRole_Id)
                        .Index(t => t.Permission_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Permission",
                            c => new
                            {
                                Id = c.Guid(false),
                                Name = c.String(false, 150),
                                IsGlobal = c.Boolean(false)
                            })
                        .PrimaryKey(t => t.Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Settings",
                            c => new
                            {
                                Id = c.Guid(false),
                                ForumName = c.String(maxLength: 500),
                                ForumUrl = c.String(maxLength: 500),
                                PageTitle = c.String(maxLength: 80),
                                MetaDesc = c.String(maxLength: 200),
                                IsClosed = c.Boolean(),
                                EnableRSSFeeds = c.Boolean(),
                                DisplayEditedBy = c.Boolean(),
                                EnablePostFileAttachments = c.Boolean(),
                                EnableMarkAsSolution = c.Boolean(),
                                MarkAsSolutionReminderTimeFrame = c.Int(),
                                EnableSpamReporting = c.Boolean(),
                                EnableMemberReporting = c.Boolean(),
                                EnableEmailSubscriptions = c.Boolean(),
                                ManuallyAuthoriseNewMembers = c.Boolean(),
                                NewMemberEmailConfirmation = c.Boolean(),
                                EmailAdminOnNewMemberSignUp = c.Boolean(),
                                TopicsPerPage = c.Int(),
                                PostsPerPage = c.Int(),
                                ActivitiesPerPage = c.Int(),
                                EnablePrivateMessages = c.Boolean(),
                                MaxPrivateMessagesPerMember = c.Int(),
                                PrivateMessageFloodControl = c.Int(),
                                EnableSignatures = c.Boolean(),
                                EnablePoints = c.Boolean(),
                                PointsAllowedForExtendedProfile = c.Int(),
                                PointsAllowedToVoteAmount = c.Int(),
                                PointsAddedPerPost = c.Int(),
                                PointsAddedPostiveVote = c.Int(),
                                PointsDeductedNagativeVote = c.Int(),
                                PointsAddedForSolution = c.Int(),
                                AdminEmailAddress = c.String(maxLength: 100),
                                NotificationReplyEmail = c.String(maxLength: 100),
                                SMTP = c.String(maxLength: 100),
                                SMTPUsername = c.String(maxLength: 100),
                                SMTPPassword = c.String(maxLength: 100),
                                SMTPPort = c.String(maxLength: 10),
                                SMTPEnableSSL = c.Boolean(),
                                Theme = c.String(maxLength: 100),
                                EnableSocialLogins = c.Boolean(),
                                SpamQuestion = c.String(maxLength: 500),
                                SpamAnswer = c.String(maxLength: 500),
                                EnableAkisment = c.Boolean(),
                                AkismentKey = c.String(maxLength: 100),
                                CurrentDatabaseVersion = c.String(maxLength: 10),
                                EnablePolls = c.Boolean(),
                                SuspendRegistration = c.Boolean(),
                                CustomHeaderCode = c.String(),
                                CustomFooterCode = c.String(),
                                EnableEmoticons = c.Boolean(),
                                DisableDislikeButton = c.Boolean(false),
                                AgreeToTermsAndConditions = c.Boolean(),
                                TermsAndConditions = c.String(),
                                DisableStandardRegistration = c.Boolean(),
                                DefaultLanguage_Id = c.Guid(false),
                                NewMemberStartingRole = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Language", t => t.DefaultLanguage_Id)
                        .ForeignKey("dbo.MembershipRole", t => t.NewMemberStartingRole)
                        .Index(t => t.DefaultLanguage_Id)
                        .Index(t => t.NewMemberStartingRole);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Language",
                            c => new
                            {
                                Id = c.Guid(false),
                                Name = c.String(false, 100),
                                LanguageCulture = c.String(false, 20),
                                FlagImageFileName = c.String(maxLength: 50),
                                RightToLeft = c.Boolean(false)
                            })
                        .PrimaryKey(t => t.Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.LocaleStringResource",
                            c => new
                            {
                                Id = c.Guid(false),
                                ResourceValue = c.String(false, 1000),
                                LocaleResourceKey_Id = c.Guid(false),
                                Language_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.LocaleResourceKey", t => t.LocaleResourceKey_Id)
                        .ForeignKey("dbo.Language", t => t.Language_Id)
                        .Index(t => t.LocaleResourceKey_Id)
                        .Index(t => t.Language_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.LocaleResourceKey",
                            c => new
                            {
                                Id = c.Guid(false),
                                Name = c.String(false, 200),
                                Notes = c.String(),
                                DateAdded = c.DateTime(false)
                            })
                        .PrimaryKey(t => t.Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Topic",
                            c => new
                            {
                                Id = c.Guid(false),
                                Name = c.String(false, 450),
                                CreateDate = c.DateTime(false),
                                Solved = c.Boolean(false),
                                SolvedReminderSent = c.Boolean(),
                                Slug = c.String(false, 450),
                                Views = c.Int(),
                                IsSticky = c.Boolean(false),
                                IsLocked = c.Boolean(false),
                                Pending = c.Boolean(),
                                Category_Id = c.Guid(false),
                                Post_Id = c.Guid(),
                                Poll_Id = c.Guid(),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Category", t => t.Category_Id)
                        .ForeignKey("dbo.Post", t => t.Post_Id)
                        .ForeignKey("dbo.Poll", t => t.Poll_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.Slug, unique: true, name: "IX_Topic_Slug")
                        .Index(t => t.Category_Id)
                        .Index(t => t.Post_Id)
                        .Index(t => t.Poll_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Favourite",
                            c => new
                            {
                                Id = c.Guid(false),
                                DateCreated = c.DateTime(false),
                                MemberId = c.Guid(false),
                                PostId = c.Guid(false),
                                TopicId = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MemberId)
                        .ForeignKey("dbo.Post", t => t.PostId)
                        .ForeignKey("dbo.Topic", t => t.TopicId)
                        .Index(t => t.MemberId)
                        .Index(t => t.PostId)
                        .Index(t => t.TopicId);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Post",
                            c => new
                            {
                                Id = c.Guid(false),
                                PostContent = c.String(false),
                                DateCreated = c.DateTime(false),
                                VoteCount = c.Int(false),
                                DateEdited = c.DateTime(false),
                                IsSolution = c.Boolean(false),
                                IsTopicStarter = c.Boolean(),
                                FlaggedAsSpam = c.Boolean(),
                                IpAddress = c.String(maxLength: 50),
                                Pending = c.Boolean(),
                                SearchField = c.String(),
                                InReplyTo = c.Guid(),
                                Topic_Id = c.Guid(false),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Topic", t => t.Topic_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.Topic_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.UploadedFile",
                            c => new
                            {
                                Id = c.Guid(false),
                                Filename = c.String(false, 200),
                                DateCreated = c.DateTime(false),
                                Post_Id = c.Guid(),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Post", t => t.Post_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.Post_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.PostEdit",
                            c => new
                            {
                                Id = c.Guid(false),
                                DateEdited = c.DateTime(false),
                                OriginalPostContent = c.String(),
                                EditedPostContent = c.String(),
                                OriginalPostTitle = c.String(maxLength: 500),
                                EditedPostTitle = c.String(maxLength: 500),
                                Post_Id = c.Guid(false),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Post", t => t.Post_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.Post_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Vote",
                            c => new
                            {
                                Id = c.Guid(false),
                                Amount = c.Int(false),
                                DateVoted = c.DateTime(),
                                Post_Id = c.Guid(false),
                                MembershipUser_Id = c.Guid(false),
                                VotedByMembershipUser_Id = c.Guid()
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Post", t => t.Post_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.VotedByMembershipUser_Id)
                        .Index(t => t.Post_Id)
                        .Index(t => t.MembershipUser_Id)
                        .Index(t => t.VotedByMembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Poll",
                            c => new
                            {
                                Id = c.Guid(false),
                                IsClosed = c.Boolean(false),
                                DateCreated = c.DateTime(false),
                                ClosePollAfterDays = c.Int(),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.PollAnswer",
                            c => new
                            {
                                Id = c.Guid(false),
                                Answer = c.String(false, 600),
                                Poll_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Poll", t => t.Poll_Id)
                        .Index(t => t.Poll_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.PollVote",
                            c => new
                            {
                                Id = c.Guid(false),
                                PollAnswer_Id = c.Guid(false),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.PollAnswer", t => t.PollAnswer_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.PollAnswer_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.TopicTag",
                            c => new
                            {
                                Id = c.Guid(false),
                                Tag = c.String(false, 100),
                                Slug = c.String(false, 100),
                                Description = c.String()
                            })
                        .PrimaryKey(t => t.Id)
                        .Index(t => t.Slug, unique: true, name: "IX_Tag_Slug");
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.TagNotification",
                            c => new
                            {
                                Id = c.Guid(false),
                                TopicTag_Id = c.Guid(false),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.TopicTag", t => t.TopicTag_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.TopicTag_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.TopicNotification",
                            c => new
                            {
                                Id = c.Guid(false),
                                Topic_Id = c.Guid(false),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.Topic", t => t.Topic_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.Topic_Id)
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.MembershipUserPoints",
                            c => new
                            {
                                Id = c.Guid(false),
                                Points = c.Int(false),
                                DateAdded = c.DateTime(false),
                                PointsFor = c.Int(false),
                                PointsForId = c.Guid(),
                                Notes = c.String(maxLength: 400),
                                MembershipUser_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                        .Index(t => t.PointsFor, "IX_MembershipUserPoints_PointsFor")
                        .Index(t => t.PointsForId, "IX_MembershipUserPoints_PointsForId")
                        .Index(t => t.MembershipUser_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.PrivateMessage",
                            c => new
                            {
                                Id = c.Guid(false),
                                DateSent = c.DateTime(false),
                                Message = c.String(false),
                                IsRead = c.Boolean(false),
                                IsSentMessage = c.Boolean(false),
                                UserTo_Id = c.Guid(false),
                                UserFrom_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => t.Id)
                        .ForeignKey("dbo.MembershipUser", t => t.UserTo_Id)
                        .ForeignKey("dbo.MembershipUser", t => t.UserFrom_Id)
                        .Index(t => t.UserTo_Id)
                        .Index(t => t.UserFrom_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.BannedEmail",
                            c => new
                            {
                                Id = c.Guid(false),
                                Email = c.String(false, 200),
                                DateAdded = c.DateTime(false)
                            })
                        .PrimaryKey(t => t.Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.BannedWord",
                            c => new
                            {
                                Id = c.Guid(false),
                                Word = c.String(false, 75),
                                IsStopWord = c.Boolean(),
                                DateAdded = c.DateTime(false)
                            })
                        .PrimaryKey(t => t.Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Email",
                            c => new
                            {
                                Id = c.Guid(false),
                                EmailTo = c.String(false, 100),
                                Body = c.String(false),
                                Subject = c.String(false, 200),
                                NameTo = c.String(false, 100),
                                DateCreated = c.DateTime(false)
                            })
                        .PrimaryKey(t => t.Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.MembershipUser_Badge",
                            c => new
                            {
                                MembershipUser_Id = c.Guid(false),
                                Badge_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => new {t.MembershipUser_Id, t.Badge_Id})
                        .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id, true)
                        .ForeignKey("dbo.Badge", t => t.Badge_Id, true)
                        .Index(t => t.MembershipUser_Id)
                        .Index(t => t.Badge_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.Topic_Tag",
                            c => new
                            {
                                Topic_Id = c.Guid(false),
                                TopicTag_Id = c.Guid(false)
                            })
                        .PrimaryKey(t => new {t.Topic_Id, t.TopicTag_Id})
                        .ForeignKey("dbo.Topic", t => t.Topic_Id, true)
                        .ForeignKey("dbo.TopicTag", t => t.TopicTag_Id, true)
                        .Index(t => t.Topic_Id)
                        .Index(t => t.TopicTag_Id);
                }
                catch
                {
                }

                try
                {
                    CreateTable(
                            "dbo.MembershipUsersInRoles",
                            c => new
                            {
                                UserIdentifier = c.Guid(false),
                                RoleIdentifier = c.Guid(false)
                            })
                        .PrimaryKey(t => new {t.UserIdentifier, t.RoleIdentifier})
                        .ForeignKey("dbo.MembershipUser", t => t.UserIdentifier, true)
                        .ForeignKey("dbo.MembershipRole", t => t.RoleIdentifier, true)
                        .Index(t => t.UserIdentifier)
                        .Index(t => t.RoleIdentifier);
                }
                catch
                {
                }
            }
        }

        public override void Down()
        {
            DropForeignKey("dbo.Vote", "VotedByMembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.Vote", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.UploadedFile", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.Topic", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.TopicNotification", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.TagNotification", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.MembershipUsersInRoles", "RoleIdentifier", "dbo.MembershipRole");
            DropForeignKey("dbo.MembershipUsersInRoles", "UserIdentifier", "dbo.MembershipUser");
            DropForeignKey("dbo.PrivateMessage", "UserFrom_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.PrivateMessage", "UserTo_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.Post", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.PostEdit", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.PollVote", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.Poll", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.MembershipUserPoints", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.CategoryNotification", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.TopicNotification", "Topic_Id", "dbo.Topic");
            DropForeignKey("dbo.Topic_Tag", "TopicTag_Id", "dbo.TopicTag");
            DropForeignKey("dbo.Topic_Tag", "Topic_Id", "dbo.Topic");
            DropForeignKey("dbo.TagNotification", "TopicTag_Id", "dbo.TopicTag");
            DropForeignKey("dbo.Post", "Topic_Id", "dbo.Topic");
            DropForeignKey("dbo.Topic", "Poll_Id", "dbo.Poll");
            DropForeignKey("dbo.PollAnswer", "Poll_Id", "dbo.Poll");
            DropForeignKey("dbo.PollVote", "PollAnswer_Id", "dbo.PollAnswer");
            DropForeignKey("dbo.Topic", "Post_Id", "dbo.Post");
            DropForeignKey("dbo.Favourite", "TopicId", "dbo.Topic");
            DropForeignKey("dbo.Favourite", "PostId", "dbo.Post");
            DropForeignKey("dbo.Vote", "Post_Id", "dbo.Post");
            DropForeignKey("dbo.PostEdit", "Post_Id", "dbo.Post");
            DropForeignKey("dbo.UploadedFile", "Post_Id", "dbo.Post");
            DropForeignKey("dbo.Favourite", "MemberId", "dbo.MembershipUser");
            DropForeignKey("dbo.Topic", "Category_Id", "dbo.Category");
            DropForeignKey("dbo.Category", "Category_Id", "dbo.Category");
            DropForeignKey("dbo.CategoryPermissionForRole", "Permission_Id", "dbo.Permission");
            DropForeignKey("dbo.CategoryPermissionForRole", "MembershipRole_Id", "dbo.MembershipRole");
            DropForeignKey("dbo.Settings", "NewMemberStartingRole", "dbo.MembershipRole");
            DropForeignKey("dbo.Settings", "DefaultLanguage_Id", "dbo.Language");
            DropForeignKey("dbo.LocaleStringResource", "Language_Id", "dbo.Language");
            DropForeignKey("dbo.LocaleStringResource", "LocaleResourceKey_Id", "dbo.LocaleResourceKey");
            DropForeignKey("dbo.GlobalPermissionForRole", "Permission_Id", "dbo.Permission");
            DropForeignKey("dbo.GlobalPermissionForRole", "MembershipRole_Id", "dbo.MembershipRole");
            DropForeignKey("dbo.CategoryPermissionForRole", "Category_Id", "dbo.Category");
            DropForeignKey("dbo.CategoryNotification", "Category_Id", "dbo.Category");
            DropForeignKey("dbo.Block", "Blocker_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.Block", "Blocked_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.BadgeTypeTimeLastChecked", "MembershipUser_Id", "dbo.MembershipUser");
            DropForeignKey("dbo.MembershipUser_Badge", "Badge_Id", "dbo.Badge");
            DropForeignKey("dbo.MembershipUser_Badge", "MembershipUser_Id", "dbo.MembershipUser");
            DropIndex("dbo.MembershipUsersInRoles", new[] {"RoleIdentifier"});
            DropIndex("dbo.MembershipUsersInRoles", new[] {"UserIdentifier"});
            DropIndex("dbo.Topic_Tag", new[] {"TopicTag_Id"});
            DropIndex("dbo.Topic_Tag", new[] {"Topic_Id"});
            DropIndex("dbo.MembershipUser_Badge", new[] {"Badge_Id"});
            DropIndex("dbo.MembershipUser_Badge", new[] {"MembershipUser_Id"});
            DropIndex("dbo.PrivateMessage", new[] {"UserFrom_Id"});
            DropIndex("dbo.PrivateMessage", new[] {"UserTo_Id"});
            DropIndex("dbo.MembershipUserPoints", new[] {"MembershipUser_Id"});
            DropIndex("dbo.MembershipUserPoints", "IX_MembershipUserPoints_PointsForId");
            DropIndex("dbo.MembershipUserPoints", "IX_MembershipUserPoints_PointsFor");
            DropIndex("dbo.TopicNotification", new[] {"MembershipUser_Id"});
            DropIndex("dbo.TopicNotification", new[] {"Topic_Id"});
            DropIndex("dbo.TagNotification", new[] {"MembershipUser_Id"});
            DropIndex("dbo.TagNotification", new[] {"TopicTag_Id"});
            DropIndex("dbo.TopicTag", "IX_Tag_Slug");
            DropIndex("dbo.PollVote", new[] {"MembershipUser_Id"});
            DropIndex("dbo.PollVote", new[] {"PollAnswer_Id"});
            DropIndex("dbo.PollAnswer", new[] {"Poll_Id"});
            DropIndex("dbo.Poll", new[] {"MembershipUser_Id"});
            DropIndex("dbo.Vote", new[] {"VotedByMembershipUser_Id"});
            DropIndex("dbo.Vote", new[] {"MembershipUser_Id"});
            DropIndex("dbo.Vote", new[] {"Post_Id"});
            DropIndex("dbo.PostEdit", new[] {"MembershipUser_Id"});
            DropIndex("dbo.PostEdit", new[] {"Post_Id"});
            DropIndex("dbo.UploadedFile", new[] {"MembershipUser_Id"});
            DropIndex("dbo.UploadedFile", new[] {"Post_Id"});
            DropIndex("dbo.Post", new[] {"MembershipUser_Id"});
            DropIndex("dbo.Post", new[] {"Topic_Id"});
            DropIndex("dbo.Favourite", new[] {"TopicId"});
            DropIndex("dbo.Favourite", new[] {"PostId"});
            DropIndex("dbo.Favourite", new[] {"MemberId"});
            DropIndex("dbo.Topic", new[] {"MembershipUser_Id"});
            DropIndex("dbo.Topic", new[] {"Poll_Id"});
            DropIndex("dbo.Topic", new[] {"Post_Id"});
            DropIndex("dbo.Topic", new[] {"Category_Id"});
            DropIndex("dbo.Topic", "IX_Topic_Slug");
            DropIndex("dbo.LocaleStringResource", new[] {"Language_Id"});
            DropIndex("dbo.LocaleStringResource", new[] {"LocaleResourceKey_Id"});
            DropIndex("dbo.Settings", new[] {"NewMemberStartingRole"});
            DropIndex("dbo.Settings", new[] {"DefaultLanguage_Id"});
            DropIndex("dbo.GlobalPermissionForRole", new[] {"Permission_Id"});
            DropIndex("dbo.GlobalPermissionForRole", new[] {"MembershipRole_Id"});
            DropIndex("dbo.CategoryPermissionForRole", new[] {"Permission_Id"});
            DropIndex("dbo.CategoryPermissionForRole", new[] {"MembershipRole_Id"});
            DropIndex("dbo.CategoryPermissionForRole", new[] {"Category_Id"});
            DropIndex("dbo.Category", new[] {"Category_Id"});
            DropIndex("dbo.Category", "IX_Category_Slug");
            DropIndex("dbo.CategoryNotification", new[] {"MembershipUser_Id"});
            DropIndex("dbo.CategoryNotification", new[] {"Category_Id"});
            DropIndex("dbo.Block", new[] {"Blocker_Id"});
            DropIndex("dbo.Block", new[] {"Blocked_Id"});
            DropIndex("dbo.BadgeTypeTimeLastChecked", new[] {"MembershipUser_Id"});
            DropIndex("dbo.MembershipUser", "IX_MembershipUser_Slug");
            DropIndex("dbo.MembershipUser", "IX_MembershipUser_UserName");
            DropTable("dbo.MembershipUsersInRoles");
            DropTable("dbo.Topic_Tag");
            DropTable("dbo.MembershipUser_Badge");
            DropTable("dbo.Email");
            DropTable("dbo.BannedWord");
            DropTable("dbo.BannedEmail");
            DropTable("dbo.PrivateMessage");
            DropTable("dbo.MembershipUserPoints");
            DropTable("dbo.TopicNotification");
            DropTable("dbo.TagNotification");
            DropTable("dbo.TopicTag");
            DropTable("dbo.PollVote");
            DropTable("dbo.PollAnswer");
            DropTable("dbo.Poll");
            DropTable("dbo.Vote");
            DropTable("dbo.PostEdit");
            DropTable("dbo.UploadedFile");
            DropTable("dbo.Post");
            DropTable("dbo.Favourite");
            DropTable("dbo.Topic");
            DropTable("dbo.LocaleResourceKey");
            DropTable("dbo.LocaleStringResource");
            DropTable("dbo.Language");
            DropTable("dbo.Settings");
            DropTable("dbo.Permission");
            DropTable("dbo.GlobalPermissionForRole");
            DropTable("dbo.MembershipRole");
            DropTable("dbo.CategoryPermissionForRole");
            DropTable("dbo.Category");
            DropTable("dbo.CategoryNotification");
            DropTable("dbo.Block");
            DropTable("dbo.BadgeTypeTimeLastChecked");
            DropTable("dbo.MembershipUser");
            DropTable("dbo.Badge");
            DropTable("dbo.Activity");
        }
    }
}