using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MVCForumAutomation
{
    [TestClass]
    public class SanityTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            var adminUser = MVCForum.LoginAsAdmin();
            var adminPage = adminUser.GoToAdminPage();
            var permissions = adminPage.GetPermissionsFor(TestDefaults.StandardMembers);
            permissions.AddToCategory(TestDefaults.ExampleCategory, PermissionTypes.CreateTopics);
            adminUser.Logout();
        }

        public TestDefaults TestDefaults
        {
            get { throw new NotImplementedException(); }
        }
        
        [TestMethod]
        public void WhenARegisteredUserStartsADiscussionOtherAnonymousUsersCanSeeIt()
        {
            const string body = "dummy body";
            var userA = MVCForum.RegisterNewUserAndLogin();
            var createdDiscussion = userA.CreateDiscussion(Discussion.With.Body(body));

            var anonymousUser = new MVCForumClient();
            var latestHeader = anonymousUser.LatestDiscussions.Top;
            Assert.AreEqual(createdDiscussion.Title, latestHeader.Title,
                "The title of the latest discussion should match the one we created");
            var viewedDiscussion = latestHeader.OpenDiscussion();
            Assert.AreEqual(body, viewedDiscussion.Body, 
                "The body of the latest discussion should match the one we created");
        }

        public MVCForumClient MVCForum { get; } = new MVCForumClient();
    }

    public class TestDefaults
    {
        public Role StandardMembers
        {
            get { throw new NotImplementedException(); }
        }
        
        public Category ExampleCategory
        {
            get { throw new NotImplementedException(); }
        }
    }
}
