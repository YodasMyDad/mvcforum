using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Extensions;

namespace MVCForumAutomation
{
    public class MVCForumClient
    {
        private readonly TestDefaults _testDefaults;
        private readonly IWebDriver _webDriver;

        public MVCForumClient(TestDefaults testDefaults)
        {
            _testDefaults = testDefaults;
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
            var username = Guid.NewGuid().ToString();
            const string password = "123456";
            var email = $"abc@{Guid.NewGuid()}.com";

            var registrationPage = GoToRegistrationPage();
            registrationPage.Username = username;
            registrationPage.Password = password;
            registrationPage.ConfirmPassword = password;
            registrationPage.Email = email;

            registrationPage.Register();

            return new LoggedInUser(_webDriver, _testDefaults);
        }

        private RegistrationPage GoToRegistrationPage()
        {
            var registerLink = _webDriver.FindElement(By.ClassName("auto-register"));
            registerLink.Click();

            return new RegistrationPage(_webDriver);
        }

        private LoggedInPage GoToLoginPage()
        {
            var logonLink = _webDriver.FindElement(By.ClassName("auto-logon"));
            logonLink.Click();

            return new LoggedInPage(_webDriver);
        }

        public LatestDiscussions LatestDiscussions
        {
            get { return new LatestDiscussions(_webDriver); }
        }

        public HotTopicsPane HotTopics
        {
            get { return new HotTopicsPane(_webDriver); }
        }

        public LoggedInAdmin LoginAsAdmin(string password)
        {
            return LoginAs(_testDefaults.AdminUsername, password, () => new LoggedInAdmin(_webDriver, _testDefaults));
        }

        private TLoggedInUser LoginAs<TLoggedInUser>(string username, string password, Func<TLoggedInUser> createLoggedInUser)
            where TLoggedInUser : LoggedInUser
        {
            var loginPage = GoToLoginPage();
            loginPage.Username = username;
            loginPage.Password = password;
            loginPage.LogOn();

            return createLoggedInUser();
        }

        public void TakeScreenshot(string screenshotFilename)
        {
            _webDriver.TakeScreenshot().SaveAsFile(screenshotFilename);
        }
    }
}