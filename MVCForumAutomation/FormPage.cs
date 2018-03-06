using OpenQA.Selenium;

namespace MVCForumAutomation
{
    internal class FormPage
    {
        protected readonly IWebDriver WebDriver;

        protected FormPage(IWebDriver webDriver)
        {
            WebDriver = webDriver;
        }

        protected void FillInputElement(string id, string value)
        {
            var input = WebDriver.FindElement(By.Id(id));
            input.SendKeys(value);
        }
    }
}