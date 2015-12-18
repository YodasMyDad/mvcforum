using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{
    public partial class PollService : IPollService
    {
        private readonly MVCForumContext _context;
        public PollService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public List<Poll> GetAllPolls()
        {
            return _context.Poll.ToList();
        }

        public Poll Add(Poll poll)
        {
            poll.DateCreated = DateTime.UtcNow;
            poll.IsClosed = false;
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
