namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class MarkedAsSolutionEventArgs : MVCForumEventArgs
    {
        public Topic Topic { get; set; }
        public Post Post { get; set; }
        public MembershipUser Marker { get; set; }
        public MembershipUser SolutionWriter { get; set; }
    }
}