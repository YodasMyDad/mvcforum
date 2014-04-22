using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IPollVoteService
    {
        List<PollVote> GetAllPollVotes();
        PollVote Add(PollVote pollVote);
        bool HasUserVotedAlready(Guid answerId, Guid userId);
        PollVote Get(Guid id);
        void Delete(PollVote pollVote);
    }
}
