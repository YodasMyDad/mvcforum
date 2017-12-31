namespace MvcForum.Web.ViewModels.Topic
{
    using Core.DomainModel.Entities;

    public class CreateTopicButtonViewModel
    {
        public MembershipUser LoggedOnUser { get; set; }
        public bool UserCanPostTopics { get; set; }
    }
}