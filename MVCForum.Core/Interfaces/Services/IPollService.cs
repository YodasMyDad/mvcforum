namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Entities;

    public partial interface IPollService : IContextService
    {
        #region Poll

        List<Poll> GetAllPolls();
        Poll Add(Poll poll);
        Poll Get(Guid id);
        void Delete(Poll item);
        void RefreshEditedPoll(Topic originalTopic, IList<PollAnswer> pollAnswers, int pollCloseAfterDays);

        #endregion

        #region Poll Answer

        List<PollAnswer> GetAllPollAnswers();
        PollAnswer Add(PollAnswer pollAnswer);
        List<PollAnswer> GetAllPollAnswersByPoll(Poll poll);
        PollAnswer GetPollAnswer(Guid id);
        void Delete(PollAnswer pollAnswer);

        #endregion

        #region Poll Votes

        List<PollVote> GetAllPollVotes();
        PollVote Add(PollVote pollVote);
        bool HasUserVotedAlready(Guid answerId, Guid userId);
        PollVote GetPollVote(Guid id);
        void Delete(PollVote pollVote);

        #endregion
    }
}