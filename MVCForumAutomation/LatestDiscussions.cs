using OpenQA.Selenium;

namespace MVCForumAutomation
{
    public class LatestDiscussions
    {
        private readonly IWebDriver _webDriver;

        public LatestDiscussions(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public DiscussionHeader Top
        {
            get
            {
                Activate();
                var latestTopicRow = _webDriver.FindElement(By.ClassName("topicrow"));
                return new DiscussionHeader(latestTopicRow);
            }
        }

        private void Activate()
        {
            var latestMenuItem = _webDriver.FindElement(By.CssSelector(".sub-nav-container .auto-latest"));
            latestMenuItem.Click();
        }
    }
}