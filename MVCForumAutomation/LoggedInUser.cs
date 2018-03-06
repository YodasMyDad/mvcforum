using OpenQA.Selenium;

namespace MVCForumAutomation
{
    public class LoggedInUser
    {
        protected readonly IWebDriver WebDriver;
        private readonly TestDefaults _testDefaults;

        public LoggedInUser(IWebDriver webDriver, TestDefaults testDefaults)
        {
            WebDriver = webDriver;
            _testDefaults = testDefaults;
        }

        public Discussion CreateDiscussion(Discussion.DiscussionBuilder builder)
        {
            var newDiscussionButton = WebDriver.FindElement(By.ClassName("createtopicbutton"));
            newDiscussionButton.Click();

            var createDisucssionPage = new CreateDiscussionPage(WebDriver);
            builder.Fill(createDisucssionPage);
            createDisucssionPage.CreateDiscussion();

            return new Discussion(WebDriver);
        }

        public void Logout()
        {
            var dropdownMenu = WebDriver.FindElement(By.ClassName("dropdown"));
            dropdownMenu.Click();

            var logoffMenuItem = dropdownMenu.FindElement(By.ClassName("auto-logoff"));
            logoffMenuItem.Click();
        }
    }
}