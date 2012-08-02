using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Events
{
    public class MarkedAsSolutionEventArgs : MVCForumEventArgs
    {
        public Topic Topic { get; set; }
        public Post Post { get; set; }
        public MembershipUser Marker { get; set; }
        public MembershipUser SolutionWriter { get; set; }
    }
}
