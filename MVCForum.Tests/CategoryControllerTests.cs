using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSubstitute;
using NUnit.Framework;
using n3oWhiteSite.Domain.DomainModel;
using n3oWhiteSite.Domain.Interfaces.Services;
using n3oWhiteSite.Website.Mapping;

namespace n3oWhiteSite.Tests
{
    [TestFixture]
    public class CategoryControllerTests
    {
        private IMembershipService _membershipServiceSub;

        [SetUp]
        public void Init()
        {
            AutoMappingRegistrar.Configure();

            _membershipServiceSub = Substitute.For<IMembershipService>();
            //.Do(x => { return MembershipCreateStatus.ProviderError; });


            var role = new MembershipRole
            {
                Id = Guid.NewGuid(),
                RoleName = "authors",
            };


            var user = new MembershipUser
            {
                UserId = Guid.NewGuid(),
                UserName = "fred",
                Comment = "test user",
                CreateDate = DateTime.Now,
                Email = "fred@test.com",
                FailedPasswordAnswerAttempt = 0,
                FailedPasswordAttemptCount = 0,
                IsApproved = true,
                IsLockedOut = false,
                LastLockoutDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                LastPasswordChangedDate = DateTime.Now,
                Password = "test",
                PasswordQuestion = "question",
                PasswordAnswer = "answer",
                Roles = new List<MembershipRole> { role },
            };

            role.Users = new List<MembershipUser> { user };

            _membershipServiceSub.GetAll().Returns(new List<MembershipUser> { user });

        }
    }
}
