namespace MvcForum.Web.ViewModels.Member
{
    using System;

    public class ReportMemberViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Reason { get; set; }
    }
}