using System;
using OpenQA.Selenium.Chrome;

namespace MVCForumAutomation
{
    public class MVCForumClient
    {
        public MVCForumClient()
        {
            var webDriver = new ChromeDriver();
        }

        public LoggedInUser RegisterNewUserAndLogin()
        {
            throw new NotImplementedException();
        }

        public LatestDiscussions LatestDiscussions
        {
            get { throw new NotImplementedException(); }
        }
    }
}