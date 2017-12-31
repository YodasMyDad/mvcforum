namespace MvcForum.Web.ViewModels.PrivateMessage
{
    using Core.Models.Entities;
    using Core.Models.General;

    public class ViewPrivateMessageViewModel
    {
        public PaginatedList<PrivateMessage> PrivateMessages { get; set; }
        public MembershipUser From { get; set; }
        public bool FromUserIsOnline { get; set; }
        public bool IsAjaxRequest { get; set; }
        public bool IsBlocked { get; set; }
    }
}