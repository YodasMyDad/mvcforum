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
            LoggedInUser userA = MVCForum.RegisterNewUserAndLogin();
            Discussion createdDiscussion = userA.CreateDiscussion(Discussion.With.Body(body));

            MVCForumClient anonymousUser = new MVCForumClient();
            DiscussionHeader latestHeader = anonymousUser.LatestDiscussions.Top;
            Assert.AreEqual(createdDiscussion.Title, latestHeader.Title,
                "The title of the latest discussion should match the one we created");
            Discussion viewedDiscussion = latestHeader.OpenDiscussion();
            Assert.AreEqual(body, viewedDiscussion.Body, 
                "The body of the latest discussion should match the one we created");
        }

        public MVCForumClient MVCForum
        {
            get { throw new NotImplementedException(); }
        }
    }
}
