using System;

namespace MVCForumAutomation
{
    public class Discussion
    {
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
        }
    }
}