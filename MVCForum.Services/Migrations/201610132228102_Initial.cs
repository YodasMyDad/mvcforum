namespace MVCForum.Services.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Activity",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(nullable: false, maxLength: 50),
                        Data = c.String(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Badge",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(nullable: false, maxLength: 50),
                        Name = c.String(nullable: false, maxLength: 50),
                        DisplayName = c.String(nullable: false, maxLength: 50),
                        Description = c.String(),
                        Image = c.String(maxLength: 50),
                        AwardsPoints = c.Decimal(precision: 10, scale: 0),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MembershipUser",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 150),
                        Password = c.String(nullable: false, maxLength: 128),
                        PasswordSalt = c.String(maxLength: 128),
                        Email = c.String(maxLength: 256),
                        PasswordQuestion = c.String(maxLength: 256),
                        PasswordAnswer = c.String(maxLength: 256),
                        IsApproved = c.Decimal(nullable: false, precision: 1, scale: 0),
                        IsLockedOut = c.Decimal(nullable: false, precision: 1, scale: 0),
                        IsBanned = c.Decimal(nullable: false, precision: 1, scale: 0),
                        CreateDate = c.DateTime(nullable: false),
                        LastLoginDate = c.DateTime(nullable: false),
                        LastPasswordChangedDate = c.DateTime(),
                        LastLockoutDate = c.DateTime(),
                        LastActivityDate = c.DateTime(),
                        FailedPasswordAttemptCount = c.Decimal(nullable: false, precision: 10, scale: 0),
                        FailedPasswordAnswerAttempt = c.Decimal(nullable: false, precision: 10, scale: 0),
                        PasswordResetToken = c.String(maxLength: 150),
                        PasswordResetTokenCreatedAt = c.DateTime(),
                        Comment = c.String(),
                        Slug = c.String(nullable: false, maxLength: 150),
                        Signature = c.String(maxLength: 1000),
                        Age = c.Decimal(precision: 10, scale: 0),
                        Location = c.String(maxLength: 100),
                        Website = c.String(maxLength: 100),
                        Twitter = c.String(maxLength: 60),
                        Facebook = c.String(maxLength: 60),
                        Avatar = c.String(maxLength: 500),
                        FacebookAccessToken = c.String(maxLength: 300),
                        FacebookId = c.Decimal(precision: 19, scale: 0),
                        TwitterAccessToken = c.String(maxLength: 300),
                        TwitterId = c.String(maxLength: 150),
                        GoogleAccessToken = c.String(maxLength: 300),
                        GoogleId = c.String(maxLength: 150),
                        MicrosoftAccessToken = c.String(maxLength: 450),
                        MicrosoftId = c.String(),
                        IsExternalAccount = c.Decimal(precision: 1, scale: 0),
                        TwitterShowFeed = c.Decimal(precision: 1, scale: 0),
                        LoginIdExpires = c.DateTime(),
                        MiscAccessToken = c.String(maxLength: 250),
                        DisableEmailNotifications = c.Decimal(precision: 1, scale: 0),
                        DisablePosting = c.Decimal(precision: 1, scale: 0),
                        DisablePrivateMessages = c.Decimal(precision: 1, scale: 0),
                        DisableFileUploads = c.Decimal(precision: 1, scale: 0),
                        HasAgreedToTermsConditions = c.Decimal(precision: 1, scale: 0),
                        Latitude = c.String(maxLength: 40),
                        Longitude = c.String(maxLength: 40),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "IX_MembershipUser_UserName")
                .Index(t => t.Slug, unique: true, name: "IX_MembershipUser_Slug");
            
            CreateTable(
                "dbo.BadgeTypeTimeLastChecked",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BadgeType = c.String(nullable: false, maxLength: 50),
                        TimeLastChecked = c.DateTime(nullable: false),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.Block",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Blocked_Id = c.Guid(nullable: false),
                        Blocker_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MembershipUser", t => t.Blocked_Id)
                .ForeignKey("dbo.MembershipUser", t => t.Blocker_Id)
                .Index(t => t.Blocked_Id)
                .Index(t => t.Blocker_Id);
            
            CreateTable(
                "dbo.CategoryNotification",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Category_Id = c.Guid(nullable: false),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.Category_Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.Category_Id)
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.Category",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 450),
                        Description = c.String(),
                        IsLocked = c.Decimal(nullable: false, precision: 1, scale: 0),
                        ModerateTopics = c.Decimal(nullable: false, precision: 1, scale: 0),
                        ModeratePosts = c.Decimal(nullable: false, precision: 1, scale: 0),
                        SortOrder = c.Decimal(nullable: false, precision: 10, scale: 0),
                        DateCreated = c.DateTime(nullable: false),
                        Slug = c.String(nullable: false, maxLength: 450),
                        PageTitle = c.String(maxLength: 80),
                        Path = c.String(maxLength: 2000),
                        MetaDescription = c.String(maxLength: 200),
                        Colour = c.String(maxLength: 50),
                        Image = c.String(maxLength: 200),
                        Category_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.Category_Id)
                .Index(t => t.Slug, unique: true, name: "IX_Category_Slug")
                .Index(t => t.Category_Id);
            
            CreateTable(
                "dbo.CategoryPermissionForRole",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IsTicked = c.Decimal(nullable: false, precision: 1, scale: 0),
                        Category_Id = c.Guid(nullable: false),
                        MembershipRole_Id = c.Guid(nullable: false),
                        Permission_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.Category_Id)
                .ForeignKey("dbo.MembershipRole", t => t.MembershipRole_Id)
                .ForeignKey("dbo.Permission", t => t.Permission_Id)
                .Index(t => t.Category_Id)
                .Index(t => t.MembershipRole_Id)
                .Index(t => t.Permission_Id);
            
            CreateTable(
                "dbo.MembershipRole",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        RoleName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GlobalPermissionForRole",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IsTicked = c.Decimal(nullable: false, precision: 1, scale: 0),
                        MembershipRole_Id = c.Guid(nullable: false),
                        Permission_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MembershipRole", t => t.MembershipRole_Id)
                .ForeignKey("dbo.Permission", t => t.Permission_Id)
                .Index(t => t.MembershipRole_Id)
                .Index(t => t.Permission_Id);
            
            CreateTable(
                "dbo.Permission",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 150),
                        IsGlobal = c.Decimal(nullable: false, precision: 1, scale: 0),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ForumName = c.String(maxLength: 500),
                        ForumUrl = c.String(maxLength: 500),
                        PageTitle = c.String(maxLength: 80),
                        MetaDesc = c.String(maxLength: 200),
                        IsClosed = c.Decimal(precision: 1, scale: 0),
                        EnableRSSFeeds = c.Decimal(precision: 1, scale: 0),
                        DisplayEditedBy = c.Decimal(precision: 1, scale: 0),
                        EnablePostFileAttachments = c.Decimal(precision: 1, scale: 0),
                        EnableMarkAsSolution = c.Decimal(precision: 1, scale: 0),
                        MarkSolutionReminderTime = c.Decimal(precision: 10, scale: 0),
                        EnableSpamReporting = c.Decimal(precision: 1, scale: 0),
                        EnableMemberReporting = c.Decimal(precision: 1, scale: 0),
                        EnableEmailSubscriptions = c.Decimal(precision: 1, scale: 0),
                        ManuallyAuthoriseNewMembers = c.Decimal(precision: 1, scale: 0),
                        NewMemberEmailConfirmation = c.Decimal(precision: 1, scale: 0),
                        EmailAdminOnNewMemberSignUp = c.Decimal(precision: 1, scale: 0),
                        TopicsPerPage = c.Decimal(precision: 10, scale: 0),
                        PostsPerPage = c.Decimal(precision: 10, scale: 0),
                        ActivitiesPerPage = c.Decimal(precision: 10, scale: 0),
                        EnablePrivateMessages = c.Decimal(precision: 1, scale: 0),
                        MaxPrivateMessagesPerMember = c.Decimal(precision: 10, scale: 0),
                        PrivateMessageFloodControl = c.Decimal(precision: 10, scale: 0),
                        EnableSignatures = c.Decimal(precision: 1, scale: 0),
                        EnablePoints = c.Decimal(precision: 1, scale: 0),
                        PointsAllowedExtendedProfile = c.Decimal(precision: 10, scale: 0),
                        PointsAllowedToVoteAmount = c.Decimal(precision: 10, scale: 0),
                        PointsAddedPerPost = c.Decimal(precision: 10, scale: 0),
                        PointsAddedPostiveVote = c.Decimal(precision: 10, scale: 0),
                        PointsDeductedNagativeVote = c.Decimal(precision: 10, scale: 0),
                        PointsAddedForSolution = c.Decimal(precision: 10, scale: 0),
                        AdminEmailAddress = c.String(maxLength: 100),
                        NotificationReplyEmail = c.String(maxLength: 100),
                        SMTP = c.String(maxLength: 100),
                        SMTPUsername = c.String(maxLength: 100),
                        SMTPPassword = c.String(maxLength: 100),
                        SMTPPort = c.String(maxLength: 10),
                        SMTPEnableSSL = c.Decimal(precision: 1, scale: 0),
                        Theme = c.String(maxLength: 100),
                        EnableSocialLogins = c.Decimal(precision: 1, scale: 0),
                        SpamQuestion = c.String(maxLength: 500),
                        SpamAnswer = c.String(maxLength: 500),
                        EnableAkisment = c.Decimal(precision: 1, scale: 0),
                        AkismentKey = c.String(maxLength: 100),
                        CurrentDatabaseVersion = c.String(maxLength: 10),
                        EnablePolls = c.Decimal(precision: 1, scale: 0),
                        SuspendRegistration = c.Decimal(precision: 1, scale: 0),
                        CustomHeaderCode = c.String(),
                        CustomFooterCode = c.String(),
                        EnableEmoticons = c.Decimal(precision: 1, scale: 0),
                        DisableDislikeButton = c.Decimal(nullable: false, precision: 1, scale: 0),
                        AgreeToTermsAndConditions = c.Decimal(precision: 1, scale: 0),
                        TermsAndConditions = c.String(),
                        DisableStandardRegistration = c.Decimal(precision: 1, scale: 0),
                        DefaultLanguage_Id = c.Guid(nullable: false),
                        NewMemberStartingRole = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Language", t => t.DefaultLanguage_Id)
                .ForeignKey("dbo.MembershipRole", t => t.NewMemberStartingRole)
                .Index(t => t.DefaultLanguage_Id)
                .Index(t => t.NewMemberStartingRole);
            
            CreateTable(
                "dbo.Language",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        LanguageCulture = c.String(nullable: false, maxLength: 20),
                        FlagImageFileName = c.String(maxLength: 50),
                        RightToLeft = c.Decimal(nullable: false, precision: 1, scale: 0),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LocaleStringResource",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ResourceValue = c.String(nullable: false, maxLength: 1000),
                        LocaleResourceKey_Id = c.Guid(nullable: false),
                        Language_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LocaleResourceKey", t => t.LocaleResourceKey_Id)
                .ForeignKey("dbo.Language", t => t.Language_Id)
                .Index(t => t.LocaleResourceKey_Id)
                .Index(t => t.Language_Id);
            
            CreateTable(
                "dbo.LocaleResourceKey",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 200),
                        Notes = c.String(),
                        DateAdded = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Topic",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 450),
                        CreateDate = c.DateTime(nullable: false),
                        Solved = c.Decimal(nullable: false, precision: 1, scale: 0),
                        SolvedReminderSent = c.Decimal(precision: 1, scale: 0),
                        Slug = c.String(nullable: false, maxLength: 450),
                        Views = c.Decimal(precision: 10, scale: 0),
                        IsSticky = c.Decimal(nullable: false, precision: 1, scale: 0),
                        IsLocked = c.Decimal(nullable: false, precision: 1, scale: 0),
                        Pending = c.Decimal(precision: 1, scale: 0),
                        Category_Id = c.Guid(nullable: false),
                        Post_Id = c.Guid(),
                        Poll_Id = c.Guid(),
                        MembershipUser_Id = c.Guid(nullable: false),
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
            
            CreateTable(
                "dbo.Favourite",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        MemberId = c.Guid(nullable: false),
                        PostId = c.Guid(nullable: false),
                        TopicId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MembershipUser", t => t.MemberId)
                .ForeignKey("dbo.Post", t => t.PostId)
                .ForeignKey("dbo.Topic", t => t.TopicId)
                .Index(t => t.MemberId)
                .Index(t => t.PostId)
                .Index(t => t.TopicId);
            
            CreateTable(
                "dbo.Post",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PostContent = c.String(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        VoteCount = c.Decimal(nullable: false, precision: 10, scale: 0),
                        DateEdited = c.DateTime(nullable: false),
                        IsSolution = c.Decimal(nullable: false, precision: 1, scale: 0),
                        IsTopicStarter = c.Decimal(precision: 1, scale: 0),
                        FlaggedAsSpam = c.Decimal(precision: 1, scale: 0),
                        IpAddress = c.String(maxLength: 50),
                        Pending = c.Decimal(precision: 1, scale: 0),
                        SearchField = c.String(),
                        InReplyTo = c.Guid(),
                        Topic_Id = c.Guid(nullable: false),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Topic", t => t.Topic_Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.Topic_Id)
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.UploadedFile",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Filename = c.String(nullable: false, maxLength: 200),
                        DateCreated = c.DateTime(nullable: false),
                        Post_Id = c.Guid(),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Post", t => t.Post_Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.Post_Id)
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.PostEdit",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DateEdited = c.DateTime(nullable: false),
                        OriginalPostContent = c.String(),
                        EditedPostContent = c.String(),
                        OriginalPostTitle = c.String(maxLength: 500),
                        EditedPostTitle = c.String(maxLength: 500),
                        Post_Id = c.Guid(nullable: false),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Post", t => t.Post_Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.Post_Id)
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.Vote",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 10, scale: 0),
                        DateVoted = c.DateTime(),
                        Post_Id = c.Guid(nullable: false),
                        MembershipUser_Id = c.Guid(nullable: false),
                        VotedByMembershipUser_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Post", t => t.Post_Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .ForeignKey("dbo.MembershipUser", t => t.VotedByMembershipUser_Id)
                .Index(t => t.Post_Id)
                .Index(t => t.MembershipUser_Id)
                .Index(t => t.VotedByMembershipUser_Id);
            
            CreateTable(
                "dbo.Poll",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IsClosed = c.Decimal(nullable: false, precision: 1, scale: 0),
                        DateCreated = c.DateTime(nullable: false),
                        ClosePollAfterDays = c.Decimal(precision: 10, scale: 0),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.PollAnswer",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Answer = c.String(nullable: false, maxLength: 600),
                        Poll_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Poll", t => t.Poll_Id)
                .Index(t => t.Poll_Id);
            
            CreateTable(
                "dbo.PollVote",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PollAnswer_Id = c.Guid(nullable: false),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PollAnswer", t => t.PollAnswer_Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.PollAnswer_Id)
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.TopicTag",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Tag = c.String(nullable: false, maxLength: 100),
                        Slug = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Slug, unique: true, name: "IX_Tag_Slug");
            
            CreateTable(
                "dbo.TagNotification",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TopicTag_Id = c.Guid(nullable: false),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.TopicTag", t => t.TopicTag_Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.TopicTag_Id)
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.TopicNotification",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Topic_Id = c.Guid(nullable: false),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Topic", t => t.Topic_Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.Topic_Id)
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.MembershipUserPoints",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Points = c.Decimal(nullable: false, precision: 10, scale: 0),
                        DateAdded = c.DateTime(nullable: false),
                        PointsFor = c.Decimal(nullable: false, precision: 10, scale: 0),
                        PointsForId = c.Guid(),
                        Notes = c.String(maxLength: 400),
                        MembershipUser_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id)
                .Index(t => t.PointsFor, name: "IX_MemUserPoints_PointsFor")
                .Index(t => t.PointsForId, name: "IX_MemUserPoints_PointsForId")
                .Index(t => t.MembershipUser_Id);
            
            CreateTable(
                "dbo.PrivateMessage",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DateSent = c.DateTime(nullable: false),
                        Message = c.String(nullable: false),
                        IsRead = c.Decimal(nullable: false, precision: 1, scale: 0),
                        IsSentMessage = c.Decimal(nullable: false, precision: 1, scale: 0),
                        UserTo_Id = c.Guid(nullable: false),
                        UserFrom_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MembershipUser", t => t.UserTo_Id)
                .ForeignKey("dbo.MembershipUser", t => t.UserFrom_Id)
                .Index(t => t.UserTo_Id)
                .Index(t => t.UserFrom_Id);
            
            CreateTable(
                "dbo.BannedEmail",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Email = c.String(nullable: false, maxLength: 200),
                        DateAdded = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BannedWord",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Word = c.String(nullable: false, maxLength: 75),
                        IsStopWord = c.Decimal(precision: 1, scale: 0),
                        DateAdded = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Email",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        EmailTo = c.String(nullable: false, maxLength: 100),
                        Body = c.String(nullable: false),
                        Subject = c.String(nullable: false, maxLength: 200),
                        NameTo = c.String(nullable: false, maxLength: 100),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MembershipUser_Badge",
                c => new
                    {
                        MembershipUser_Id = c.Guid(nullable: false),
                        Badge_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.MembershipUser_Id, t.Badge_Id })
                .ForeignKey("dbo.MembershipUser", t => t.MembershipUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.Badge", t => t.Badge_Id, cascadeDelete: true)
                .Index(t => t.MembershipUser_Id)
                .Index(t => t.Badge_Id);
            
            CreateTable(
                "dbo.Topic_Tag",
                c => new
                    {
                        Topic_Id = c.Guid(nullable: false),
                        TopicTag_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.Topic_Id, t.TopicTag_Id })
                .ForeignKey("dbo.Topic", t => t.Topic_Id, cascadeDelete: true)
                .ForeignKey("dbo.TopicTag", t => t.TopicTag_Id, cascadeDelete: true)
                .Index(t => t.Topic_Id)
                .Index(t => t.TopicTag_Id);
            
            CreateTable(
                "dbo.MembershipUsersInRoles",
                c => new
                    {
                        UserIdentifier = c.Guid(nullable: false),
                        RoleIdentifier = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserIdentifier, t.RoleIdentifier })
                .ForeignKey("dbo.MembershipUser", t => t.UserIdentifier, cascadeDelete: true)
                .ForeignKey("dbo.MembershipRole", t => t.RoleIdentifier, cascadeDelete: true)
                .Index(t => t.UserIdentifier)
                .Index(t => t.RoleIdentifier);
            
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
            DropIndex("dbo.MembershipUsersInRoles", new[] { "RoleIdentifier" });
            DropIndex("dbo.MembershipUsersInRoles", new[] { "UserIdentifier" });
            DropIndex("dbo.Topic_Tag", new[] { "TopicTag_Id" });
            DropIndex("dbo.Topic_Tag", new[] { "Topic_Id" });
            DropIndex("dbo.MembershipUser_Badge", new[] { "Badge_Id" });
            DropIndex("dbo.MembershipUser_Badge", new[] { "MembershipUser_Id" });
            DropIndex("dbo.PrivateMessage", new[] { "UserFrom_Id" });
            DropIndex("dbo.PrivateMessage", new[] { "UserTo_Id" });
            DropIndex("dbo.MembershipUserPoints", new[] { "MembershipUser_Id" });
            DropIndex("dbo.MembershipUserPoints", "IX_MemUserPoints_PointsForId");
            DropIndex("dbo.MembershipUserPoints", "IX_MemUserPoints_PointsFor");
            DropIndex("dbo.TopicNotification", new[] { "MembershipUser_Id" });
            DropIndex("dbo.TopicNotification", new[] { "Topic_Id" });
            DropIndex("dbo.TagNotification", new[] { "MembershipUser_Id" });
            DropIndex("dbo.TagNotification", new[] { "TopicTag_Id" });
            DropIndex("dbo.TopicTag", "IX_Tag_Slug");
            DropIndex("dbo.PollVote", new[] { "MembershipUser_Id" });
            DropIndex("dbo.PollVote", new[] { "PollAnswer_Id" });
            DropIndex("dbo.PollAnswer", new[] { "Poll_Id" });
            DropIndex("dbo.Poll", new[] { "MembershipUser_Id" });
            DropIndex("dbo.Vote", new[] { "VotedByMembershipUser_Id" });
            DropIndex("dbo.Vote", new[] { "MembershipUser_Id" });
            DropIndex("dbo.Vote", new[] { "Post_Id" });
            DropIndex("dbo.PostEdit", new[] { "MembershipUser_Id" });
            DropIndex("dbo.PostEdit", new[] { "Post_Id" });
            DropIndex("dbo.UploadedFile", new[] { "MembershipUser_Id" });
            DropIndex("dbo.UploadedFile", new[] { "Post_Id" });
            DropIndex("dbo.Post", new[] { "MembershipUser_Id" });
            DropIndex("dbo.Post", new[] { "Topic_Id" });
            DropIndex("dbo.Favourite", new[] { "TopicId" });
            DropIndex("dbo.Favourite", new[] { "PostId" });
            DropIndex("dbo.Favourite", new[] { "MemberId" });
            DropIndex("dbo.Topic", new[] { "MembershipUser_Id" });
            DropIndex("dbo.Topic", new[] { "Poll_Id" });
            DropIndex("dbo.Topic", new[] { "Post_Id" });
            DropIndex("dbo.Topic", new[] { "Category_Id" });
            DropIndex("dbo.Topic", "IX_Topic_Slug");
            DropIndex("dbo.LocaleStringResource", new[] { "Language_Id" });
            DropIndex("dbo.LocaleStringResource", new[] { "LocaleResourceKey_Id" });
            DropIndex("dbo.Settings", new[] { "NewMemberStartingRole" });
            DropIndex("dbo.Settings", new[] { "DefaultLanguage_Id" });
            DropIndex("dbo.GlobalPermissionForRole", new[] { "Permission_Id" });
            DropIndex("dbo.GlobalPermissionForRole", new[] { "MembershipRole_Id" });
            DropIndex("dbo.CategoryPermissionForRole", new[] { "Permission_Id" });
            DropIndex("dbo.CategoryPermissionForRole", new[] { "MembershipRole_Id" });
            DropIndex("dbo.CategoryPermissionForRole", new[] { "Category_Id" });
            DropIndex("dbo.Category", new[] { "Category_Id" });
            DropIndex("dbo.Category", "IX_Category_Slug");
            DropIndex("dbo.CategoryNotification", new[] { "MembershipUser_Id" });
            DropIndex("dbo.CategoryNotification", new[] { "Category_Id" });
            DropIndex("dbo.Block", new[] { "Blocker_Id" });
            DropIndex("dbo.Block", new[] { "Blocked_Id" });
            DropIndex("dbo.BadgeTypeTimeLastChecked", new[] { "MembershipUser_Id" });
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
