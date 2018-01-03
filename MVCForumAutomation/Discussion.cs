using System;

namespace MVCForumAutomation
{
    public class Discussion
    {
        public static DiscussionBuilder With
        {
            get { throw new NotImplementedException(); }
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
            public DiscussionBuilder Body(string body)
            {
                throw new NotImplementedException();
            }
        }
    }
}