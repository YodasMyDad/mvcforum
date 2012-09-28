using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Utilities;

namespace MVCForum.Data.Repositories
{
    public class PollAnswerRepository : IPollAnswerRepository
    {
        private readonly MVCForumContext _context;
        public PollAnswerRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public List<PollAnswer> GetAllPollAnswers()
        {
            return _context.PollAnswer.ToList();
        }

        public PollAnswer Add(PollAnswer pollAnswer)
        {
            return _context.PollAnswer.Add(pollAnswer);
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
