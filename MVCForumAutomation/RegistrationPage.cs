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
            set
            {
                var passwordInput = _webDriver.FindElement(By.Id("Password"));
                passwordInput.SendKeys(value);
            }
        }

        public string ConfirmPassword
        {
            get { throw new System.NotImplementedException(); }
            set
            {
                var confirmPasswordInput = _webDriver.FindElement(By.Id("ConfirmPassword"));
                confirmPasswordInput.SendKeys(value);
            }
        }

        public string Email
        {
            get { throw new System.NotImplementedException(); }
            set
            {
                var emailInput = _webDriver.FindElement(By.Id("Email"));
                emailInput.SendKeys(value);
            }
        }

        public void Register()
        {
            throw new System.NotImplementedException();
        }
    }
}