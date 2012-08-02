using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using n3oWhiteSite.Domain.Interfaces.Services;
using n3oWhiteSite.Domain.DomainModel;
using n3oWhiteSite.Website.Areas.Admin.Controllers;
using n3oWhiteSite.Website.Areas.Admin.ViewModels;
using n3oWhiteSite.Website.Mapping;

// For an overview: http://msdn.microsoft.com/en-us/magazine/cc163904.aspx

namespace n3oWhiteSite.Tests
{
    [TestFixture]
    public class AuthorControllerAddTests
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
                               Roles = new List<MembershipRole> {role},
                               Stories = new List<NewsItem>()
                           };

            var newsItem = new NewsItem
            {
                Id = Guid.NewGuid(),
                Title = "test title",
                Body = "body",
                Authors = new List<MembershipUser> { user }
            };

            role.Users = new List<MembershipUser> {user};
            user.Stories = new List<NewsItem> {newsItem};

            _membershipServiceSub.GetAll().Returns(new List<MembershipUser>{user});
            
        }

        [Test]
        public void ManageTest()
        {
            var controller = new AccountController(_membershipServiceSub);
            var result = controller.Manage();

            Assert.IsNotNull(result);
        }

        [Test]        
        public void Add_User_Fail()
        {
            _membershipServiceSub
                .CreateUser(Arg.Any<MembershipUser>())
                .Returns(x => MembershipCreateStatus.ProviderError);

            var viewModel = new MemberAddViewModel
                                {
                                    Comment = "Test"
                                };
            var controller = new AccountController(_membershipServiceSub);
            controller.Add(viewModel);

            Assert.IsFalse(controller.ModelState.IsValid);
        }

        [Test]
        //[ExpectedException(typeof(Exception))]
        public void Add_User_Success()
        {
            _membershipServiceSub
                .CreateUser(Arg.Any<MembershipUser>())
                .Returns(x => MembershipCreateStatus.Success);

            var viewModel = new MemberAddViewModel
            {
                Comment = "Test"
            };
            var controller = new AccountController(_membershipServiceSub);
            controller.Add(viewModel);

            Assert.IsTrue(controller.ModelState.IsValid);
        }
    }
}
