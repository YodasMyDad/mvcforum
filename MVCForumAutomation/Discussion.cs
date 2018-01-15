using System;
using OpenQA.Selenium;

namespace MVCForumAutomation
{
    public class Discussion
    {
        private readonly IWebDriver _webDriver;

        public Discussion(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public static DiscussionBuilder With
        {
            get { return new DiscussionBuilder(); }
        }

        public string Title
        {
            get { throw new NotImplementedException(); }
        }

        public string Body
        {
            get { throw new NotImplementedException(); }
        }

        public class DiscussionBuilder
        {
            private string _body;

            public DiscussionBuilder Body(string body)
            {
                _body = body;
                return this;
            }

            public void Fill(CreateDiscussionPage createDiscussionPage)
            {
                createDiscussionPage.Title = Guid.NewGuid().ToString();
                createDiscussionPage.Body = _body;

                createDiscussionPage.CreateDiscussion();
            }
        }
    }
}