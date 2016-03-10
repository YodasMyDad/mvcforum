using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public enum PostOrderBy
    {
        Standard,
        Newest,
        Votes
    }

    public partial class Post : Entity
    {
        public Post()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string PostContent { get; set; }
        public DateTime DateCreated { get; set; }
        public int VoteCount { get; set; }
        public DateTime DateEdited { get; set; }
        public bool IsSolution { get; set; }
        public bool IsTopicStarter { get; set; }
        public bool? FlaggedAsSpam { get; set; }
        public string IpAddress { get; set; }
        public bool? Pending { get; set; }
        public string SearchField { get; set; }
        public Guid? InReplyTo { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual MembershipUser User { get; set; }
        public virtual IList<Vote> Votes { get; set; }
        public virtual IList<UploadedFile> Files { get; set; }
        public virtual IList<Favourite> Favourites { get; set; }
        public virtual IList<PostEdit> PostEdits { get; set; }
    }
}
