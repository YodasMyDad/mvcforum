using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class PollAnswerService : IPollAnswerService
    {
        private readonly IPollAnswerRepository _pollAnswerRepository;

        public PollAnswerService(IPollAnswerRepository pollAnswerRepository)
        {
            _pollAnswerRepository = pollAnswerRepository;
        }

        public List<PollAnswer> GetAllPollAnswers()
        {
            return _pollAnswerRepository.GetAllPollAnswers();
        }

        public PollAnswer Add(PollAnswer pollAnswer)
        {
            pollAnswer.Answer = StringUtils.SafePlainText(pollAnswer.Answer);
            return _pollAnswerRepository.Add(pollAnswer);
        }

        public List<PollAnswer> GetAllPollAnswersByPoll(Poll poll)
        {
            return _pollAnswerRepository.GetAllPollAnswersByPoll(poll);
        }

        public PollAnswer Get(Guid id)
        {
            return _pollAnswerRepository.Get(id);
        }

        public void Delete(PollAnswer pollAnswer)
        {
            _pollAnswerRepository.Delete(pollAnswer);
        }

    }
}
