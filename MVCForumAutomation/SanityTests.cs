using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MVCForumAutomation
{
    [TestClass]
    public class SanityTests
    {
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

        public MVCForumClient MVCForum
        {
            get { throw new NotImplementedException(); }
        }
    }
}
