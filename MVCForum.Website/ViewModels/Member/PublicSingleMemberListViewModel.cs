namespace MvcForum.Web.ViewModels.Member
{
    using System;
    using Application;

    public class PublicSingleMemberListViewModel
    {
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        public string UserName { get; set; }

        public string NiceUrl { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.DateJoined")]
        public DateTime CreateDate { get; set; }

        public int TotalPoints { get; set; }
    }
}