namespace MvcForum.Web.ViewModels.Member
{
    using System.Collections.Generic;
    using Topic;

    public class ViewMemberDiscussionsViewModel
    {
        public IList<TopicViewModel> Topics { get; set; }
    }
}