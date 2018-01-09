using OpenQA.Selenium;

namespace MVCForumAutomation
{
    public class LoggedInAdmin : LoggedInUser
    {
        private readonly IWebDriver _webDriver;

        public LoggedInAdmin(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public AdminPage GoToAdminPage()
        {
            var myToolsMenu = _webDriver.FindElement(By.ClassName("mytoolslink"));
            myToolsMenu.Click();

            var adminLink = _webDriver.FindElement(By.CssSelector(".dropdown .auto-admin"));
            adminLink.Click();

            return new AdminPage(_webDriver);
        }
    }
}