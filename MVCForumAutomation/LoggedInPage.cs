using OpenQA.Selenium;

namespace MVCForumAutomation
{
    internal class LoggedInPage
    {
        private readonly IWebDriver _webDriver;

        public LoggedInPage(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public string Username
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public string Password
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public void LogOn()
        {
            throw new System.NotImplementedException();
        }
    }
}