using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IPollAnswerService
    {
        List<PollAnswer> GetAllPollAnswers();
        PollAnswer Add(PollAnswer pollAnswer);
        PollAnswer Get(Guid id);
        void Delete(PollAnswer pollAnswer);
    }
}
