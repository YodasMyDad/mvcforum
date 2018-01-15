using System;

namespace MVCForumAutomation
{
    public class Discussion
    {
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
            private readonly TestDefaults _testDefaults;
            private string _body;

            public DiscussionBuilder(TestDefaults testDefaults)
            {
                _testDefaults = testDefaults;
            }

            public DiscussionBuilder Body(string body)
            {
                _body = body;
                return this;
            }

            public void Fill(CreateDiscussionPage createDiscussionPage)
            {
                createDiscussionPage.Title = Guid.NewGuid().ToString();
                createDiscussionPage.SelectCategory(_testDefaults.ExampleCategory);
                createDiscussionPage.Body = _body;

                createDiscussionPage.CreateDiscussion();
            }
        }
    }
}