namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Entities;

    public partial interface IPollAnswerService
    {
        List<PollAnswer> GetAllPollAnswers();
        PollAnswer Add(PollAnswer pollAnswer);
        List<PollAnswer> GetAllPollAnswersByPoll(Poll poll);
        PollAnswer Get(Guid id);
        void Delete(PollAnswer pollAnswer);
    }
}