using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class PollRepository : IPollRepository
    {
        private readonly MVCForumContext _context;
        public PollRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public List<Poll> GetAllPolls()
        {
            return _context.Poll.ToList();
        }

        public Poll Add(Poll poll)
        {
            return _context.Poll.Add(poll);
        }

        public Poll Get(Guid id)
        {
            return _context.Poll.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Poll item)
        {
            _context.Poll.Remove(item);
        }
    }
}
