using System;
using OpenQA.Selenium.Chrome;

namespace MVCForumAutomation
{
    public class MVCForumClient
    {
        private readonly ChromeDriver _webDriver;

        public MVCForumClient()
        {
            _webDriver = new ChromeDriver();
        }

        ~MVCForumClient()
        {
            _webDriver.Quit();
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