using System;
using System.Collections.Generic;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.DomainModel;

namespace MVCForum.API
{
    public class VoteAPI : IVoteAPI
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IVoteRepository _voteRepository;

        public VoteAPI(IVoteRepository voteRepository, IMembershipRepository membershipRepository)
        {
            _voteRepository = voteRepository;
            _membershipRepository = membershipRepository;
        }

        public IEnumerable<Vote> GetAllVotesReceived(Guid memberId)
        {
            var member = _membershipRepository.Get(memberId);
            return member.Votes;
        }

        public IEnumerable<Vote> GetAllVotesGiven(Guid memberId)
        {
            return _voteRepository.GetAllVotesByUser(memberId);
        }
    }
}
