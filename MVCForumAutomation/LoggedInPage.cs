using OpenQA.Selenium;

namespace MVCForumAutomation
{
    internal class LoggedInPage : FormPage
    {
        public LoggedInPage(IWebDriver webDriver) 
            : base(webDriver)
        {
        }

        public string Username
        {
            get { throw new System.NotImplementedException(); }
            set
            {
                FillInputElement("UserName", value);
            }
        }

        public string Password
        {
            get { throw new System.NotImplementedException(); }
            set
            {
                FillInputElement("Password", value);
            }
        }

        public void LogOn()
        {
            var loginForm = WebDriver.FindElement(By.ClassName("form-login"));
            loginForm.Submit();
        }
    }
}