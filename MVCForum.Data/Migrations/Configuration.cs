using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using System.Web.Mvc;
using MVCForum.Data.Context;
using System.Data.Entity.Migrations;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services;
using MVCForum.Utilities;

namespace MVCForum.Data.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<MVCForumContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(MVCForumContext context)
        {

            #region Initial Installer Code
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );

            // Do everything in a unitofwork transaction
            //var unitOfWorkManager = new UnitOfWorkManager(context);
            //using (var uow = unitOfWorkManager.NewUnitOfWork())
            //{
            // Get services
            // var localisationService = DependencyResolver.Current.GetService<ILocalizationService>();
            var loggingService = DependencyResolver.Current.GetService<ILoggingService>();

            // Add the language - If it's not already there
            const string langCulture = "en-GB";
            var language = context.Language.FirstOrDefault(x => x.LanguageCulture == langCulture);
            if (language == null)
            {
                var cultureInfo = LanguageUtils.GetCulture(langCulture);
                language = new Language
                {
                    Name = cultureInfo.EnglishName,
                    LanguageCulture = cultureInfo.Name,
                };
                context.Language.Add(language);

                // Save the language
                context.SaveChanges();

                // Now add the default language strings
                var file = HostingEnvironment.MapPath(@"~/Installer/en-GB.csv");
                var commaSeparator = new[] { ',' };
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

                        if (string.IsNullOrEmpty(key))
                        {
                            loggingService.Error(string.Format("Empty Key on line {0} when importing initial data", lineCounter));
                            // Ignore empty keys
                            continue;
                        }

                        if (string.IsNullOrEmpty(value))
                        {
                            loggingService.Error(string.Format("Empty Value on line {0} when importing initial data", lineCounter));
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
                            DateAdded = DateTime.UtcNow,
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
                var adminRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == AppConstants.AdminRoleName);
                if (adminRole == null)
                {
                    adminRole = new MembershipRole { RoleName = AppConstants.AdminRoleName };
                    context.MembershipRole.Add(adminRole);
                    saveRoles = true;
                }

                // Create the Standard role if it doesn't exist
                var standardRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == AppConstants.StandardMembers);
                if (standardRole == null)
                {
                    standardRole = new MembershipRole { RoleName = AppConstants.StandardMembers };
                    context.MembershipRole.Add(standardRole);
                    saveRoles = true;
                }

                // Create the Guest role if it doesn't exist
                var guestRole = context.MembershipRole.FirstOrDefault(x => x.RoleName == AppConstants.GuestRoleName);
                if (guestRole == null)
                {
                    guestRole = new MembershipRole { RoleName = AppConstants.GuestRoleName };
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
                        ForumName = "MVCForum",
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
                        PointsAddedPerPost = 1,
                        PointsAddedForSolution = 4,
                        PointsDeductedNagativeVote = 2,
                        AdminEmailAddress = "my@email.com",
                        NotificationReplyEmail = "noreply@myemail.com",
                        SMTPEnableSSL = false,
                        Theme = "Metro",
                        NewMemberStartingRole = standardRole,
                        DefaultLanguage = language,
                        ActivitiesPerPage = 20,
                        EnableAkisment = false,
                        EnableSocialLogins = false,
                        EnablePolls = true
                    };

                    context.Setting.Add(settings);
                    context.SaveChanges();
                }

                // Create the initial category permissions

                // Edit Posts
                if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionEditPosts) == null)
                {
                    var permission = new Permission { Name = AppConstants.PermissionEditPosts };
                    context.Permission.Add(permission);

                    // NOTE: Because this is null - We assumed it's a new install so carry on checking and adding the other permissions

                    // Read Only
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionReadOnly) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionReadOnly };
                        context.Permission.Add(p);
                    }

                    // Delete Posts
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionDeletePosts) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionDeletePosts };
                        context.Permission.Add(p);
                    }

                    // Sticky Topics
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionCreateStickyTopics) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionCreateStickyTopics };
                        context.Permission.Add(p);
                    }

                    // Lock Topics
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionLockTopics) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionLockTopics };
                        context.Permission.Add(p);
                    }

                    // Vote In Polls
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionVoteInPolls) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionVoteInPolls };
                        context.Permission.Add(p);
                    }

                    // Create Polls
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionCreatePolls) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionCreatePolls };
                        context.Permission.Add(p);
                    }

                    // Create Topics
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionCreateTopics) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionCreateTopics };
                        context.Permission.Add(p);
                    }

                    // Attach Files
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionAttachFiles) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionAttachFiles };
                        context.Permission.Add(p);
                    }

                    // Deny Access
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionDenyAccess) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionDenyAccess };
                        context.Permission.Add(p);
                    }

                    // === Global Permissions === //

                    // Deny Access
                    if (context.Permission.FirstOrDefault(x => x.Name == AppConstants.PermissionEditMembers) == null)
                    {
                        var p = new Permission { Name = AppConstants.PermissionEditMembers, IsGlobal = true };
                        context.Permission.Add(p);
                    }

                    // Save to the database
                    context.SaveChanges();
                }

                // If the admin user exists then don't do anything else
                const string adminUsername = "admin";
                if (context.MembershipUser.FirstOrDefault(x => x.UserName == adminUsername) == null)
                {
                    // create the admin user and put him in the admin role
                    var admin = new MembershipUser
                    {
                        Email = "you@email.com",
                        UserName = adminUsername,
                        Password = "password",
                        IsApproved = true,
                        DisableEmailNotifications = false,
                        DisablePosting = false,
                        DisablePrivateMessages = false,
                        CreateDate = DateTime.UtcNow,
                        LastLockoutDate = (DateTime)SqlDateTime.MinValue,
                        LastPasswordChangedDate = (DateTime)SqlDateTime.MinValue,
                        LastLoginDate = DateTime.UtcNow,
                        LastActivityDate = null,
                        IsLockedOut = false,
                        Slug = ServiceHelpers.CreateUrl(adminUsername)
                    };

                    // Hash the password
                    var salt = StringUtils.CreateSalt(AppConstants.SaltSize);
                    var hash = StringUtils.GenerateSaltedHash(admin.Password, salt);
                    admin.Password = hash;
                    admin.PasswordSalt = salt;

                    // Put the admin in the admin role
                    admin.Roles = new List<MembershipRole> { adminRole };

                    context.MembershipUser.Add(admin);
                    context.SaveChanges();
                }

            }

            #endregion
        }
    }
}
