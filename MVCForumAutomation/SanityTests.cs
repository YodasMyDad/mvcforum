using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MVCForumAutomation
{
    [TestClass]
    public class SanityTests
    {
        /*
        Login as a registered user		                            // MVCForum (the application)
        Start a discussion titled “Hi!” and body “dummy body” 		// Logged-in user
        Enter the site as an anonymous user (from another browser)	// MVCForum (new instance)
        Verify that a discussion titled “Hi!” appears       		// MVCForum.LatestDiscussions.Top (*)
        Open that discussion                                		// Discussion header
        Verify that the body of the discussion is “dummy body”		// Discussion
             */
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
    }
}
