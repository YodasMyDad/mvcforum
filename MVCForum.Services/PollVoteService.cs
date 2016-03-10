using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{
    public partial class PollVoteService : IPollVoteService
    {
        private readonly MVCForumContext _context;
        public PollVoteService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public List<PollVote> GetAllPollVotes()
        {
            return _context.PollVote.ToList();
        }

        public PollVote Add(PollVote pollVote)
        {
            return _context.PollVote.Add(pollVote);
        }

        public bool HasUserVotedAlready(Guid answerId, Guid userId)
        {
            var vote = _context.PollVote.FirstOrDefault(x => x.PollAnswer.Id == answerId && x.User.Id == userId);
            return (vote != null);
        }

        public PollVote Get(Guid id)
        {
            return _context.PollVote.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(PollVote pollVote)
        {
            _context.PollVote.Remove(pollVote);
        }

    }
}
