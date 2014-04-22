using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class PollVoteService : IPollVoteService
    {
        private readonly IPollVoteRepository _pollVoteRepository;

        public PollVoteService(IPollVoteRepository pollVoteRepository)
        {
            _pollVoteRepository = pollVoteRepository;
        }

        public List<PollVote> GetAllPollVotes()
        {
            return _pollVoteRepository.GetAllPollVotes();
        }

        public PollVote Add(PollVote pollVote)
        {
            return _pollVoteRepository.Add(pollVote);
        }

        public bool HasUserVotedAlready(Guid answerId, Guid userId)
        {
            return _pollVoteRepository.HasUserVotedAlready(answerId, userId);
        }

        public PollVote Get(Guid id)
        {
            return _pollVoteRepository.Get(id);
        }

        public void Delete(PollVote pollVote)
        {
            _pollVoteRepository.Delete(pollVote);
        }

    }
}
