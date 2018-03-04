namespace MvcForum.Core.Services.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web.Hosting;
    using Constants;
    using Data.Context;
    using ExtensionMethods;
    using Models.Entities;
    using Utilities;

    public class Configuration : DbMigrationsConfiguration<MvcForumContext>
    {
        private readonly bool _pendingMigrations;

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;

            // Check if there are migrations pending to run, this can happen if database doesn't exists or if there was any
            //  change in the schema
            var migrator = new DbMigrator(this);
            _pendingMigrations = migrator.GetPendingMigrations().Any();

            // If there are pending migrations run migrator.Update() to create/update the database then run the Seed() method to populate
            //  the data if necessary
            if (_pendingMigrations)
            {
                migrator.Update();
                Seed(new MvcForumContext());
            }
        }

        protected override void Seed(MvcForumContext context)
        {
            #region Initial Installer Code

            //var isFirstInstall = false;

            // Add the language - If it's not already there
            const string langCulture = "en-GB";
            var language = context.Language.FirstOrDefault(x => x.LanguageCulture == langCulture);
            if (language == null)
            {
                //isFirstInstall = true;
                var cultureInfo = LanguageUtils.GetCulture(langCulture);
                language = new Language
                {
                    Name = cultureInfo.EnglishName,
                    LanguageCulture = cultureInfo.Name
                };
                context.Language.Add(language);

                // Save the language
                context.SaveChanges();

                // Now add the default language strings
                var file = HostingEnvironment.MapPath(@"~/Installer/en-GB.csv");
                var commaSeparator = new[] {','};
                if (file != null)
                {
                    // Unpack the data
                    var allLines = new List<string>();
                    using (var streamReader = new StreamReader(file, Encoding.UTF8, true))
                    {
                        while (streamReader.Peek() >= 0)
                        {
                            allLines.Add(streamReader.ReadLine());
                        }
                    }

                    // Read the CSV file and import all the keys and values
                    var lineCounter = 0;
                    foreach (var csvline in allLines)
                    {
                        var line = csvline;
                        if (line.StartsWith("\""))
                        {
                            line = line.Replace("\"", "");
                        }

                        lineCounter++;

                        // Only split on the first comma, so the value strings can have commas in
                        var keyValuePair = line.Split(commaSeparator, 2, StringSplitOptions.None);

                        // Get the key and value
                        var key = keyValuePair[0];
                        var value = keyValuePair[1];

                        if (string.IsNullOrWhiteSpace(key))
                        {
                            // Ignore empty keys
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(value))
                        {
                            // Ignore empty values
                            continue;
                        }

                        // Trim both the key and value
                        key = key.Trim();
                        value = value.Trim();

                        // Create the resource key
                        var resourceKey = new LocaleResourceKey
                        {
                            Name = key,
                            DateAdded = DateTime.UtcNow
                        };
                        context.LocaleResourceKey.Add(resourceKey);

                        // Set the value for the resource
                        var stringResource = new LocaleStringResource
                        {
                            Language = language,
                            LocaleResourceKey = resourceKey,
                            ResourceValue = value
                        };
                        context.LocaleStringResource.Add(stringResource);
                    }

                    // Save the language strings
                    context.SaveChanges();
                }


                var saveRoles = false;
                // Create the admin role if it doesn't exist
                var adminRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == Constants.AdminRoleName);
                if (adminRole == null)
                {
                    adminRole = new MembershipRole {RoleName = Constants.AdminRoleName};
                    context.MembershipRole.Add(adminRole);
                    saveRoles = true;
                }

                // Create the Standard role if it doesn't exist
                var standardRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == ForumConfiguration.Instance.StandardMembers);
                if (standardRole == null)
                {
                    standardRole = new MembershipRole {RoleName = ForumConfiguration.Instance.StandardMembers};
                    context.MembershipRole.Add(standardRole);
                    saveRoles = true;
                }

                // Create the Guest role if it doesn't exist
                var guestRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == Constants.GuestRoleName);
                if (guestRole == null)
                {
                    guestRole = new MembershipRole {RoleName = Constants.GuestRoleName};
                    context.MembershipRole.Add(guestRole);
                    saveRoles = true;
                }

                if (saveRoles)
                {
                    context.SaveChanges();
                }

                // Create an example Category

                if (!context.Category.Any())
                {
                    // Doesn't exist so add the example category
                    const string exampleCatName = "Example Category";
                    var exampleCat = new Category
                    {
                        Name = exampleCatName,
                        ModeratePosts = false,
                        ModerateTopics = false,
                        Slug = ServiceHelpers.CreateUrl(exampleCatName),
                        DateCreated = DateTime.UtcNow
                    };

                    context.Category.Add(exampleCat);
                    context.SaveChanges();
                }

                // if the settings already exist then do nothing
                // If not then add default settings
                var currentSettings = context.Setting.FirstOrDefault();
                if (currentSettings == null)
                {
                    // create the settings
                    var settings = new Settings
                    {
                        ForumName = "MvcForum",
                        ForumUrl = "http://www.mydomain.com",
                        IsClosed = false,
                        EnableRSSFeeds = true,
                        DisplayEditedBy = true,
                        EnablePostFileAttachments = false,
                        EnableMarkAsSolution = true,
                        EnableSpamReporting = true,
                        EnableMemberReporting = true,
                        EnableEmailSubscriptions = true,
                        ManuallyAuthoriseNewMembers = false,
                        EmailAdminOnNewMemberSignUp = true,
                        TopicsPerPage = 20,
                        PostsPerPage = 20,
                        EnablePrivateMessages = true,
                        MaxPrivateMessagesPerMember = 50,
                        PrivateMessageFloodControl = 1,
                        EnableSignatures = false,
                        EnablePoints = true,
                        PointsAllowedToVoteAmount = 1,
                        PointsAllowedForExtendedProfile = 1,
                        PointsAddedPerPost = 1,
                        PointsAddedForSolution = 4,
                        PointsDeductedNagativeVote = 2,
                        PointsAddedPostiveVote = 2,
                        AdminEmailAddress = "my@email.com",
                        NotificationReplyEmail = "noreply@myemail.com",
                        SMTPEnableSSL = false,
                        Theme = "Metro",
                        NewMemberStartingRole = standardRole,
                        DefaultLanguage = language,
                        ActivitiesPerPage = 20,
                        EnableAkisment = false,
                        EnableSocialLogins = false,
                        EnablePolls = true,
                        MarkAsSolutionReminderTimeFrame = 7,
                        EnableEmoticons = true,
                        DisableStandardRegistration = false
                    };

                    context.Setting.Add(settings);
                    context.SaveChanges();
                }

                // Create the initial category permissions

                // Edit Posts
                if (context.Permission.FirstOrDefault(x => x.Name == ForumConfiguration.Instance.PermissionEditPosts) ==
                    null)
                {
                    var permission = new Permission {Name = ForumConfiguration.Instance.PermissionEditPosts};
                    context.Permission.Add(permission);

                    // NOTE: Because this is null - We assumed it's a new install so carry on checking and adding the other permissions

                    // Read Only
                    if (context.Permission.FirstOrDefault(x => x.Name == ForumConfiguration.Instance.PermissionReadOnly) ==
                        null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionReadOnly};
                        context.Permission.Add(p);
                    }

                    // Delete Posts
                    if (context.Permission.FirstOrDefault(x =>
                            x.Name == ForumConfiguration.Instance.PermissionDeletePosts) == null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionDeletePosts};
                        context.Permission.Add(p);
                    }

                    // Sticky Topics
                    if (context.Permission.FirstOrDefault(x =>
                            x.Name == ForumConfiguration.Instance.PermissionCreateStickyTopics) ==
                        null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionCreateStickyTopics};
                        context.Permission.Add(p);
                    }

                    // Lock Topics
                    if (context.Permission.FirstOrDefault(x => x.Name == ForumConfiguration.Instance.PermissionLockTopics) ==
                        null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionLockTopics};
                        context.Permission.Add(p);
                    }

                    // Vote In Polls
                    if (context.Permission.FirstOrDefault(x =>
                            x.Name == ForumConfiguration.Instance.PermissionVoteInPolls) == null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionVoteInPolls};
                        context.Permission.Add(p);
                    }

                    // Create Polls
                    if (context.Permission.FirstOrDefault(x =>
                            x.Name == ForumConfiguration.Instance.PermissionCreatePolls) == null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionCreatePolls};
                        context.Permission.Add(p);
                    }

                    // Create Topics
                    if (context.Permission.FirstOrDefault(x =>
                            x.Name == ForumConfiguration.Instance.PermissionCreateTopics) == null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionCreateTopics};
                        context.Permission.Add(p);
                    }

                    // Attach Files
                    if (context.Permission.FirstOrDefault(x =>
                            x.Name == ForumConfiguration.Instance.PermissionAttachFiles) == null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionAttachFiles};
                        context.Permission.Add(p);
                    }

                    // Deny Access
                    if (context.Permission.FirstOrDefault(x => x.Name == ForumConfiguration.Instance.PermissionDenyAccess) ==
                        null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionDenyAccess};
                        context.Permission.Add(p);
                    }

                    // === Global Permissions === //

                    // Deny Access
                    if (context.Permission.FirstOrDefault(x =>
                            x.Name == ForumConfiguration.Instance.PermissionEditMembers) == null)
                    {
                        var p = new Permission {Name = ForumConfiguration.Instance.PermissionEditMembers, IsGlobal = true};
                        context.Permission.Add(p);
                    }

                    // Insert Editor Images
                    AddPermissionInsertEditorImages(context);

                    // Insert Create Tags
                    AddPermissionCreateTags(context);

                    // Save to the database
                    context.SaveChanges();
                }

                // Create a temp password
                var tempAdminPassword = "mvc".AppendUniqueIdentifier();

                // If the admin user exists then don't do anything else
                const string adminUsername = "admin";
                if (context.MembershipUser.FirstOrDefault(x => x.UserName == adminUsername) == null)
                {
                    // create the admin user and put him in the admin role
                    var admin = new MembershipUser
                    {
                        Email = "you@email.com",
                        UserName = adminUsername,
                        Password = tempAdminPassword,
                        IsApproved = true,
                        DisableEmailNotifications = false,
                        DisablePosting = false,
                        DisablePrivateMessages = false,
                        CreateDate = DateTime.UtcNow,
                        LastLockoutDate = (DateTime) SqlDateTime.MinValue,
                        LastPasswordChangedDate = (DateTime) SqlDateTime.MinValue,
                        LastLoginDate = DateTime.UtcNow,
                        LastActivityDate = null,
                        IsLockedOut = false,
                        Slug = ServiceHelpers.CreateUrl(adminUsername)
                    };

                    // Hash the password
                    var salt = StringUtils.CreateSalt(Constants.SaltSize);
                    var hash = StringUtils.GenerateSaltedHash(admin.Password, salt);
                    admin.Password = hash;
                    admin.PasswordSalt = salt;

                    // Put the admin in the admin role
                    admin.Roles = new List<MembershipRole> {adminRole};

                    context.MembershipUser.Add(admin);
                    context.SaveChanges();

                    // Now add read me
                    const string name = "Read Me";
                    var category = context.Category.FirstOrDefault();
                    var topic = new Topic
                    {
                        Category = category,
                        CreateDate = DateTime.UtcNow,
                        User = admin,
                        IsSticky = true,
                        Name = name,
                        Slug = ServiceHelpers.CreateUrl(name)
                    };

                    context.Topic.Add(topic);
                    context.SaveChanges();

                    var readMeText = $@"<h2>Admin Login Details</h2>
<p>We have auto created an admin user for you to manage the site</p>
<p>Username: <strong>admin</strong><br />Password: <strong>{tempAdminPassword}</strong></p>
<p>Once you have logged in, you can manage the forum <a href=""/admin/"">through the admin section</a>. </p>
<p><strong><font color=""#ff0000"">Important:</font> </strong>Please update the admin password and username before putting this site live and delete this post, or you put the security of your forum at risk.</p>
<h2>Permissions</h2> 
<p>You must <a href=""/admin/permissions/"">set the permissions</a> for each Role in the admin section, as <u>by default no permissions are enabled</u>. So for example, you might want to set 'Create Topics' to be 
    allowed for the Standard Role or no one will be able to create topics.</p>
<h2>Documentation</h2>
<p>We have some documentation on Github in the WIKI</p>
<p><a href=""https://github.com/YodasMyDad/mvcforum/wiki"">https://github.com/YodasMyDad/mvcforum/wiki</a></p>
<h2>Issues & Help</h2>
<p>If you general need help then please post on the support forums, but if you find a bug then please either raise an issue on Github or even better fix the issue and create a pull request ;)</p>
<p><a href=""https://github.com/YodasMyDad/mvcforum/pulls"">https://github.com/YodasMyDad/mvcforum/pulls</a></p>
<p><a href=""https://github.com/YodasMyDad/mvcforum/issues"">https://github.com/YodasMyDad/mvcforum/issues</a></p>";

                    var post = new Post
                    {
                        DateCreated = DateTime.UtcNow,
                        DateEdited = DateTime.UtcNow,
                        Topic = topic,
                        IsTopicStarter = true,
                        User = admin,
                        PostContent = readMeText
                    };

                    topic.LastPost = post;

                    context.Post.Add(post);
                    context.SaveChanges();
                }
            }
            else
            {
                // Do upgrades
                UpgradeData(context);
            }

            #endregion
        }

        /// <summary>
        /// Adds any new data for new versions of the forum
        /// </summary>
        /// <param name="context"></param>
        private void UpgradeData(MvcForumContext context)
        {
            // Data to update on versions v1.7+

            AddPermissionInsertEditorImages(context);

            // Version 2.0 Upgrades

            AddPermissionCreateTags(context);
        }


        /// <summary>
        /// Adds permission for users to insert images into the editor
        /// </summary>
        /// <param name="context"></param>
        private void AddPermissionInsertEditorImages(MvcForumContext context)
        {
            if (context.Permission.FirstOrDefault(
                    x => x.Name == ForumConfiguration.Instance.PermissionInsertEditorImages) == null)
            {
                var p = new Permission
                {
                    Name = ForumConfiguration.Instance.PermissionInsertEditorImages,
                    IsGlobal = true
                };
                context.Permission.Add(p);
            }
        }

        /// <summary>
        /// Adds permission for users to create tags
        /// </summary>
        /// <param name="context"></param>
        private void AddPermissionCreateTags(MvcForumContext context)
        {
            if (context.Permission.FirstOrDefault(
                    x => x.Name == ForumConfiguration.Instance.PermissionCreateTags) == null)
            {
                var p = new Permission
                {
                    Name = ForumConfiguration.Instance.PermissionCreateTags,
                    IsGlobal = false
                };
                context.Permission.Add(p);
            }
        }
    }
}