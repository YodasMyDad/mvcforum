using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MVCForumAutomation
{
    public class MVCForumClient
    {
        private readonly IWebDriver _webDriver;

        public MVCForumClient()
        {
            // TODO: select the type of browser and the URL from a configuration file
            _webDriver = new ChromeDriver();
            _webDriver.Url = "http://localhost:8080";
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