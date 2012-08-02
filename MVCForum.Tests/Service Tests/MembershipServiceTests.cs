using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    [TestFixture]
    public class MembershipServiceTests
    {
        private IMVCForumAPI _api;
        private IActivityService _activityService;

        [SetUp]
        public void Init()
        {
            _api = Substitute.For<IMVCForumAPI>();
            _activityService = Substitute.For<IActivityService>();
        }

        [Test]
        public void ValidateUserUserNotFound()
        {
            const string userName = "SpongeBob";
            var user = new MembershipUser {UserName = userName};

            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser(user.UserName).Returns(x => null);
            
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var status =  membershipService.ValidateUser(userName, "password", 1);
            Assert.IsFalse(status);
            Assert.AreEqual(membershipService.LastLoginStatus, LoginAttemptStatus.UserNotFound);
        }

        [Test]
        public void ValidateUserUserIsLockedOut()
        {
            const string userName = "SpongeBob";
            var user = new MembershipUser { UserName = userName, IsLockedOut = true };

            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser(user.UserName).Returns(user);

            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var status = membershipService.ValidateUser(userName, "password", 1);
            Assert.IsFalse(status);
            Assert.AreEqual(membershipService.LastLoginStatus, LoginAttemptStatus.UserLockedOut);
        }

        [Test]
        public void ValidateUserUserNotApproved()
        {
            const string userName = "SpongeBob";
            var user = new MembershipUser { UserName = userName, IsApproved = false };

            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser(user.UserName).Returns(user);

            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var status = membershipService.ValidateUser(userName, "password", 1);
            Assert.IsFalse(status);
            Assert.AreEqual(membershipService.LastLoginStatus, LoginAttemptStatus.UserNotApproved);
        }

        [Test]
        public void ValidateUserMaxInvalidPasswordAttempts()
        {
            const string userName = "SpongeBob";
            const int passwordAttempts = 1;
            var user = new MembershipUser { UserName = userName, 
                IsApproved = true, 
                Password = "xxx", 
                FailedPasswordAttemptCount = passwordAttempts,
                IsLockedOut = false,
            };

            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser(user.UserName).Returns(user);

            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var status = membershipService.ValidateUser(userName, "password", 1);
            Assert.IsFalse(status);
            Assert.AreEqual(membershipService.LastLoginStatus, LoginAttemptStatus.PasswordAttemptsExceeded);
        }

        [Test]
        public void ValidateUserMaxFailedPasswordAttempt()
        {
            const string userName = "SpongeBob";
            const int passwordAttempts = 1;
            var user = new MembershipUser { UserName = userName, 
                IsApproved = true, 
                Password = "xxx", 
                FailedPasswordAttemptCount = passwordAttempts,
                PasswordSalt = "aaaa"};

            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser(user.UserName).Returns(user);

            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var status = membershipService.ValidateUser(userName, "password", 3);
            Assert.IsFalse(status);
            Assert.AreEqual(membershipService.LastLoginStatus, LoginAttemptStatus.PasswordIncorrect);
            Assert.AreEqual(user.FailedPasswordAttemptCount, passwordAttempts + 1);
        }


        [Test]
        public void ValidateUserMaxFailedPasswordAttemptMaxExceeded()
        {
            const string userName = "SpongeBob";
            const int passwordAttempts = 1;
            var user = new MembershipUser
            {
                UserName = userName,
                IsApproved = true,
                Password = "xxx",
                FailedPasswordAttemptCount = passwordAttempts,
                PasswordSalt = "aaaa"
            };

            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser(user.UserName).Returns(user);

            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var status = membershipService.ValidateUser(userName, "password", 2);
            Assert.IsFalse(status);
            Assert.IsTrue(user.IsLockedOut);
            Assert.AreEqual(membershipService.LastLoginStatus, LoginAttemptStatus.PasswordIncorrect);
            Assert.AreEqual(user.FailedPasswordAttemptCount, passwordAttempts + 1);
        }

        #region Import / Export

        [Test]
        public void ImportSingleUser()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser("Henry").Returns(x => null);
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var testData = new List<string>
                                {
                                    "Henry,h@h.com,02/04/2011 17:30,18,henry location,www.henry.com,facebook,signature",                                  
                                };
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var report = membershipService.FromCsv(testData);

            Assert.IsTrue(report.Errors.Count == 0 && report.Warnings.Count == 0);

        }

        [Test]
        public void ImportSingleUserTwice()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser("Henry").Returns(x => null);
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var testData = new List<string>
                                {
                                    "Henry,h@h.com,02/04/2011 17:30,18,henry location,www.henry.com,facebook,signature",   
                                    "Henry,h@h.com,02/04/2011 17:30,18,henry location,www.henry.com,facebook,signature", 
                                };
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var report = membershipService.FromCsv(testData);

            membershipRepository.Received().Add(Arg.Is<MembershipUser>(x => x.UserName == "Henry"));

            Assert.IsTrue(report.Errors.Count == 1 && report.Errors[0].ErrorWarningType == CsvErrorWarningType.AlreadyExists && report.Warnings.Count == 0);
        }

        [Test]
        public void ImportTwoUsers()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser("Henry").Returns(x => null);
            membershipRepository.GetUser("Alison").Returns(x => null);
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var testData = new List<string>
                                {
                                    "Henry,h@h.com,02/04/2011 17:30,18,henry location,www.henry.com,facebook,signature",    
                                    "Alison,h@h.com,02/04/2011 17:30,18,henry location,www.henry.com,facebook,signature"
                                };
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var report = membershipService.FromCsv(testData);

            Assert.IsTrue(report.Errors.Count == 0 && report.Warnings.Count == 0);

        }

        [Test]
        public void ImportSingleUserWithoutAllData()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser("Henry").Returns(x => null);
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var testData = new List<string>
                                {
                                    "Henry,h@h.com,02/04/2011 17:30,18,henry location,,,signature",                                  
                                };
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var report = membershipService.FromCsv(testData);

            membershipRepository.Received().Add(Arg.Is<MembershipUser>(x => x.UserName == "Henry" && 
                x.Email == "h@h.com" &&
                x.Age == 18 &&
                x.Location == "henry location" &&
                x.Website == string.Empty &&
                x.Facebook == string.Empty &&
                x.Signature == "signature"));
            Assert.IsTrue(report.Errors.Count == 0 && report.Warnings.Count == 0);

        }

        [Test]
        public void ImportSingleUserInsufficientValues()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser("Henry").Returns(x => null);
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var testData = new List<string>
                                {
                                    "Henry",                                  
                                };
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var report = membershipService.FromCsv(testData);

            Assert.IsTrue(report.Errors.Count == 1 && report.Errors[0].ErrorWarningType == CsvErrorWarningType.MissingKeyOrValue && report.Warnings.Count == 0);

        }

        [Test]
        public void ImportSingleUserMissingUser()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser("Henry").Returns(x => null);
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var testData = new List<string>
                                {
                                  ",,",  
                                };
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var report = membershipService.FromCsv(testData);

            Assert.IsTrue(report.Errors.Count == 1 && report.Errors[0].ErrorWarningType == CsvErrorWarningType.MissingKeyOrValue && report.Warnings.Count == 0);

        }

        [Test]
        public void ImportSingleUserMissingEmail()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser("Henry").Returns(x => null);
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var testData = new List<string>
                                {
                                  "Henry,,",  
                                };
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var report = membershipService.FromCsv(testData);

            Assert.IsTrue(report.Errors.Count == 1 && report.Errors[0].ErrorWarningType == CsvErrorWarningType.MissingKeyOrValue && report.Warnings.Count == 0);

        }

        [Test]
        public void ImportUserExists()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            membershipRepository.GetUser("Henry").Returns(x => new MembershipUser{UserName="Henry"});
            membershipRepository.GetUser("Alison").Returns(x => null);
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var testData = new List<string>
                                {
                                    "Henry,h@h.com,02/04/2011 17:30,18,henry location,,,signature",  
                                    "Alison,h@h.com,02/04/2011 17:30,18,henry location,,,signature"
                                };
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var report = membershipService.FromCsv(testData);

            // Second user ok
            membershipRepository.Received().Add(Arg.Is<MembershipUser>(x => x.UserName == "Alison" && 
                x.Email == "h@h.com" &&
                x.Age == 18 &&
                x.Location == "henry location" &&
                x.Website == string.Empty &&
                x.Facebook == string.Empty &&
                x.Signature == "signature"));

            // First user failed
            Assert.IsTrue(report.Errors.Count == 1 && report.Errors[0].ErrorWarningType == CsvErrorWarningType.AlreadyExists && report.Warnings.Count == 0);
        }

        [Test]
        public void ImportUserExceptionThrown()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();

            membershipRepository.GetUser("Henry").Returns(x => null);
            membershipRepository.GetUser("Alison").Returns(x => null);
            membershipRepository.When(x => x.Add(Arg.Any<MembershipUser>())).Do(x => { throw new Exception(); });

            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var testData = new List<string>
                                {
                                    "Henry,h@h.com,02/04/2011 17:30,18,henry location,,,signature", 
                                    "Alison,h@h.com,02/04/2011 17:30,18,henry location,,,signature"
                                };
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var report = membershipService.FromCsv(testData);

            // Second user still processed (but will fail) - checks code does not break out of loop
            membershipRepository.Received().Add(Arg.Is<MembershipUser>(x => x.UserName == "Alison" &&
                x.Email == "h@h.com" &&
                x.Age == 18 &&
                x.Location == "henry location" &&
                x.Website == string.Empty &&
                x.Facebook == string.Empty &&
                x.Signature == "signature"));

            Assert.IsTrue(report.Errors.Count == 2 && report.Errors[0].ErrorWarningType == CsvErrorWarningType.GeneralError && report.Warnings.Count == 0);
        }

        [Test]
        public void ExportUsers()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            var usersFromDb = new List<MembershipUser>
                                  {
                                      new MembershipUser
                                          {
                                              UserName = "Henry",
                                              Email = "h@h.com",
                                              CreateDate = DateTime.Parse("02/04/2011 17:30"),
                                              Age = 18,
                                              Location = "henry location",
                                              Website = "www.henry.com",
                                              Facebook = "facebook",
                                              Signature = "signature"
                                          },
                                      new MembershipUser
                                          {
                                              UserName = "Alison",
                                              Email = "a@h.com",
                                              CreateDate = DateTime.Parse("02/04/2011 17:30"),
                                              Age = 45,
                                              Location = "alison location",
                                              Website = "www.alison.com",
                                              Facebook = "facebook",
                                              Signature = "signature"
                                          }
                                  };
            membershipRepository.GetAll().Returns(x => usersFromDb);

            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();

            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _api);

            var export = membershipService.ToCsv();

            Assert.AreEqual(export,
                 "Henry,h@h.com,02/04/2011 17:30:00,18,henry location,www.henry.com,facebook,signature\r\nAlison,a@h.com,02/04/2011 17:30:00,45,alison location,www.alison.com,facebook,signature\r\n");
        }
        #endregion

    }
}
