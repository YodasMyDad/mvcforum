using OpenQA.Selenium;

namespace MVCForumAutomation
{
    internal class RegistrationPage : FormPage
    {
        public RegistrationPage(IWebDriver webDriver)
            : base(webDriver)
        {
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
            var form = WebDriver.FindElement(By.ClassName("form-register"));
            form.Submit();
        }
    }
}