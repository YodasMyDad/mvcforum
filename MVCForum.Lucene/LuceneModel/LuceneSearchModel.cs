using System;

namespace MVCForum.Lucene.LuceneModel
{
    public class LuceneSearchModel
    {
        public Guid Id { get; set; }
        public string TopicName { get; set; }
        public string PostContent { get; set; }
    }
}
