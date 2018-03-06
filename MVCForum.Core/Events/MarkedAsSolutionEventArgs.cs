namespace MvcForum.Core.Events
{
    using Models.Entities;

    public class MarkedAsSolutionEventArgs : MvcForumEventArgs
    {
        public Topic Topic { get; set; }
        public Post Post { get; set; }
        public MembershipUser Marker { get; set; }
        public MembershipUser SolutionWriter { get; set; }
    }
}