using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface IPollAnswerRepository
    {
        List<PollAnswer> GetAllPollAnswers();
        PollAnswer Add(PollAnswer pollAnswer);
        PollAnswer Get(Guid id);
        void Delete(PollAnswer pollAnswer);
    }
}
