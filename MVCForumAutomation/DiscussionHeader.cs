using System;
using OpenQA.Selenium;

namespace MVCForumAutomation
{
    public class DiscussionHeader
    {
        private readonly IWebElement _topicRow;

        public DiscussionHeader(IWebElement topicRow)
        {
            _topicRow = topicRow;
        }

        public string Title
        {
            get
            {
                var titleElement = _topicRow.FindElement(By.TagName("h3"));
                return titleElement.Text;
            }
        }

        public Discussion OpenDiscussion()
        {
            throw new NotImplementedException();
        }
    }
}