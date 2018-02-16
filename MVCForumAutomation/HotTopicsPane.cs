using OpenQA.Selenium;

namespace MVCForumAutomation
{
    public class HotTopicsPane
    {
        private readonly IWebDriver _webDriver;
        private readonly IWebElement _hotTopicsBox;

        public HotTopicsPane(IWebDriver webDriver)
        {
            _webDriver = webDriver;
            _hotTopicsBox = webDriver.FindElement(By.ClassName("hottopics-box"));
        }

        public Discussion Open(string topicSubject)
        {
            var topic = _hotTopicsBox.FindElement(By.LinkText(topicSubject));
            topic.Click();
            return new Discussion(_webDriver);
        }
    }
}