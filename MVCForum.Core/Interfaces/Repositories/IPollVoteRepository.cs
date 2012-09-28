using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface IPollVoteRepository
    {
        List<PollVote> GetAllPollVotes();
        PollVote Add(PollVote pollVote);
        PollVote Get(Guid id);
        void Delete(PollVote pollVote);
    }
}
