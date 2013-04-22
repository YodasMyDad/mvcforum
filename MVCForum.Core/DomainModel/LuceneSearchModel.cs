using System;

namespace MVCForum.Domain.DomainModel
{
    public class LuceneSearchModel
    {
        // Post Stuff
        public Guid Id { get; set; }
        public string PostContent { get; set; }
        public DateTime DateCreated { get; set; }

        //Topic Stuff
        public string TopicName { get; set; }
        public string TopicUrl { get; set; }
        public Guid TopicId { get; set; }
        //public string TopicTags { get; set; }

        // User Stuff
        public string Username { get; set; }
        public Guid UserId { get; set; }

        //Score
        public float Score { get; set; }
    }
}
