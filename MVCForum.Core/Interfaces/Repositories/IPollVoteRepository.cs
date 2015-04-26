using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IPollVoteRepository
    {
        List<PollVote> GetAllPollVotes();
        PollVote Add(PollVote pollVote);
        bool HasUserVotedAlready(List<Guid> answerIds, Guid userId);
        PollVote Get(Guid id);
        void Delete(PollVote pollVote);
    }
}
