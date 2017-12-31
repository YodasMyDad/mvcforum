namespace MvcForum.Web.ViewModels.Poll
{
    using Core.DomainModel.Entities;

    public class PollViewModel
    {
        public Poll Poll { get; set; }
        public bool UserHasAlreadyVoted { get; set; }
        public int TotalVotesInPoll { get; set; }
        public bool UserAllowedToVote { get; set; }
    }
}