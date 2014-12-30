using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class VoteService : IVoteService
    {
        private readonly IVoteRepository _voteRepository;

        public VoteService(IVoteRepository voteRepository)
        {
            _voteRepository = voteRepository;
        }

        public void Delete(Vote vote)
        {
            _voteRepository.Delete(vote);
        }

        public IList<Vote> GetAllVotesByUser(Guid membershipId)
        {
            return _voteRepository.GetAllVotesByUser(membershipId);
        }

        public List<Vote> GetVotesByPosts(List<Guid> postIds)
        {
            return _voteRepository.GetVotesByPosts(postIds);
        }

        /// <summary>
        /// Add a new vote
        /// </summary>
        /// <param name="vote"></param>
        /// <returns></returns>
        public Vote Add(Vote vote)
        {

            var e = new VoteEventArgs {Vote = vote};
            EventManager.Instance.FireBeforeVoteMade(this, e);

            if (!e.Cancel)
            {
                _voteRepository.Add(vote); 

                EventManager.Instance.FireAfterVoteMade(this, new VoteEventArgs {Vote = vote});
            }

            return vote;
        }
    }
}
