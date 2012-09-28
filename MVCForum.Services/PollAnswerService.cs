using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public class PollAnswerService : IPollAnswerService
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

        public PollAnswer Add(PollAnswer PollAnswer)
        {
            return _pollAnswerRepository.Add(PollAnswer);
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
