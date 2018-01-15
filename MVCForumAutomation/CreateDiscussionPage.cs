using OpenQA.Selenium;

namespace MVCForumAutomation
{
    public class CreateDiscussionPage
    {
        private readonly IWebDriver _webDriver;

        public CreateDiscussionPage(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public string Title
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public string Body
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public void CreateDiscussion()
        {
            throw new System.NotImplementedException();
        }
    }
}