using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public class VoteService : IVoteService
    {
        private readonly IVoteRepository _voteRepository;
        private readonly IMVCForumAPI _api;

        public VoteService(IVoteRepository voteRepository, IMVCForumAPI api)
        {
            _voteRepository = voteRepository;
            _api = api;
        }

        public void Delete(Vote vote)
        {
            _voteRepository.Delete(vote);
        }

        /// <summary>
        /// Add a new vote
        /// </summary>
        /// <param name="vote"></param>
        /// <returns></returns>
        public Vote Add(Vote vote)
        {

            var e = new VoteEventArgs {Vote = vote, Api = _api};
            EventManager.Instance.FireBeforeVoteMade(this, e);

            if (!e.Cancel)
            {
                _voteRepository.Add(vote); 

                EventManager.Instance.FireAfterVoteMade(this, new VoteEventArgs {Vote = vote, Api = _api});
            }

            return vote;
        }
    }
}
