using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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
            const string email = "abc@def.com";

            var registrationPage = GoToRegistrationPage();
            registrationPage.Username = username;
            registrationPage.Password = password;
            registrationPage.ConfirmPassword = password;
            registrationPage.Email = email;

            registrationPage.Register();

            return new LoggedInUser();
        }

        private RegistrationPage GoToRegistrationPage()
        {
            var registerLink = _webDriver.FindElement(By.ClassName("auto-register"));
            registerLink.Click();

            return new RegistrationPage(_webDriver);
        }

        public LatestDiscussions LatestDiscussions
        {
            get { throw new NotImplementedException(); }
        }

        public LoggedInAdmin LoginAsAdmin()
        {
            return LoginAs<LoggedInAdmin>(_testDefaults.AdminUsername, _testDefaults.AdminPassword);
        }

        private TLoggedInUser LoginAs<TLoggedInUser>(string username, string password)
            where TLoggedInUser : LoggedInUser
        {
            throw new NotImplementedException();
        }
    }
}