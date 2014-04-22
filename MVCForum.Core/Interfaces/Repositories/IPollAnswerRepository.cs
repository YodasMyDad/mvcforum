using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IPollAnswerRepository
    {
        List<PollAnswer> GetAllPollAnswers();
        List<PollAnswer> GetAllPollAnswersByPoll(Poll poll);
        PollAnswer Add(PollAnswer pollAnswer);
        PollAnswer Get(Guid id);
        void Delete(PollAnswer pollAnswer);
    }
}
