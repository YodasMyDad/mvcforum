namespace MvcForum.Web.ViewModels.PrivateMessage
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class ListPrivateMessageViewModel
    {
        public IList<PrivateMessageListItem> Messages { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}