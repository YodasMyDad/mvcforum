using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MVCForumAutomation
{
    [TestClass]
    public class SanityTests
    {
        public SanityTests()
        {
            TestDefaults = new TestDefaults();
            MVCForum = new MVCForumClient(TestDefaults);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var adminUser = MVCForum.LoginAsAdmin();
            var adminPage = adminUser.GoToAdminPage();
            var permissions = adminPage.GetPermissionsFor(TestDefaults.StandardMembers);
            permissions.AddToCategory(TestDefaults.ExampleCategory, PermissionTypes.CreateTopics);
            adminUser.Logout();
        }

        [TestMethod]
        public void WhenARegisteredUserStartsADiscussionOtherAnonymousUsersCanSeeIt()
        {
            const string body = "dummy body";
            var userA = MVCForum.RegisterNewUserAndLogin();
            var createdDiscussion = userA.CreateDiscussion(Discussion.With.Body(body));

            var anonymousUser = OpenNewMVCForumClient();
            var latestHeader = anonymousUser.LatestDiscussions.Top;
            Assert.AreEqual(createdDiscussion.Title, latestHeader.Title,
                "The title of the latest discussion should match the one we created");
            var viewedDiscussion = latestHeader.OpenDiscussion();
            Assert.AreEqual(body, viewedDiscussion.Body, 
                "The body of the latest discussion should match the one we created");
        }

        private MVCForumClient OpenNewMVCForumClient()
        {
            return new MVCForumClient(TestDefaults);
        }

        public TestDefaults TestDefaults { get; }

        public MVCForumClient MVCForum { get; }
    }
}
