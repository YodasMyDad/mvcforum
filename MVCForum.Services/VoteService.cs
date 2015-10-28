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
        private readonly IMembershipUserPointsService _membershipUserPointsService;

        public VoteService(IVoteRepository voteRepository, IMembershipUserPointsService membershipUserPointsService)
        {
            _voteRepository = voteRepository;
            _membershipUserPointsService = membershipUserPointsService;
        }

        public void Delete(Vote vote)
        {
            // Delete any points associated with this vote
            _membershipUserPointsService.Delete(PointsFor.Vote, vote.Id);

            // Delete the vote
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

        public List<Vote> GetVotesByPost(Guid postId)
        {
            return _voteRepository.GetVotesByPost(postId);
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
