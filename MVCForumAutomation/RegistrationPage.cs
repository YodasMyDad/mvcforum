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
            set { FillInputElement("UserName", value); }
        }

        public string Password
        {
            get { throw new System.NotImplementedException(); }
            set
            {
                FillInputElement("Password", value);
            }
        }

        public string ConfirmPassword
        {
            get { throw new System.NotImplementedException(); }
            set
            {
                FillInputElement("ConfirmPassword", value);
            }
        }

        public string Email
        {
            get { throw new System.NotImplementedException(); }
            set
            {
                FillInputElement("Email", value);
            }
        }

        public void Register()
        {
            var form = _webDriver.FindElement(By.ClassName("form-register"));
            form.Submit();
        }

        private void FillInputElement(string id, string value)
        {
            var input = _webDriver.FindElement(By.Id(id));
            input.SendKeys(value);
        }
    }
}