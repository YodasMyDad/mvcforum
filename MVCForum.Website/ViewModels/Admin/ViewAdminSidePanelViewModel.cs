namespace MvcForum.Web.ViewModels.Admin
{
    using Core.Models.Entities;

    public class ViewAdminSidePanelViewModel
    {
        public MembershipUser CurrentUser { get; set; }
        public int NewPrivateMessageCount { get; set; }
        public bool CanViewPrivateMessages { get; set; }
        public bool IsDropDown { get; set; }
        public int ModerateCount { get; set; }
    }
}