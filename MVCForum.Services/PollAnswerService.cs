using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class PollAnswerService : IPollAnswerService
    {
        private readonly MVCForumContext _context;
        public PollAnswerService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public List<PollAnswer> GetAllPollAnswers()
        {
            return _context.PollAnswer
                    .Include(x => x.Poll).ToList();
        }

        public PollAnswer Add(PollAnswer pollAnswer)
        {
            pollAnswer.Answer = StringUtils.SafePlainText(pollAnswer.Answer);
            return _context.PollAnswer.Add(pollAnswer);
        }

        public List<PollAnswer> GetAllPollAnswersByPoll(Poll poll)
        {
            var answers = _context.PollAnswer
                    .Include(x => x.Poll)
                    .AsNoTracking()
                    .Where(x => x.Poll.Id == poll.Id).ToList();
            return answers;
        }

        public PollAnswer Get(Guid id)
        {
            return _context.PollAnswer.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(PollAnswer pollAnswer)
        {
            _context.PollAnswer.Remove(pollAnswer);
        }

    }
}
