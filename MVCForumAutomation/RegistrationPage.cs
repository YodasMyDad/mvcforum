using OpenQA.Selenium;

namespace MVCForumAutomation
{
    internal class RegistrationPage
    {
        private readonly IWebDriver _webDriver;

        public RegistrationPage(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public string Username
        {
            get { throw new System.NotImplementedException(); }
            set
            {
                var usernameInput = _webDriver.FindElement(By.Id("UserName"));
                usernameInput.SendKeys(value);
            }
        }

        public string Password
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public string ConfirmPassword
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public string Email
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public void Register()
        {
            throw new System.NotImplementedException();
        }
    }
}