namespace MvcForum.Web.ViewModels.Topic
{
    public class CheckCreateTopicPermissions
    {
        public bool CanUploadFiles { get; set; }
        public bool CanStickyTopic { get; set; }
        public bool CanLockTopic { get; set; }
        public bool CanCreatePolls { get; set; }
        public bool CanInsertImages { get; set; }
        public bool CanCreateTags { get; set; }
    }
}