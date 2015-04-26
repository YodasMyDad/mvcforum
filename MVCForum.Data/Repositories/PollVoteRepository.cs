using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class PollVoteRepository : IPollVoteRepository
    {
        private readonly MVCForumContext _context;
        public PollVoteRepository(IMVCForumContext context)
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

        public bool HasUserVotedAlready(List<Guid> answerIds, Guid userId)
        {
            //Checks each potential answer/userid combination to see if user has already voted
            foreach(var answerId in answerIds)
            {
                if(_context.PollVote.Any(x => x.PollAnswer.Id == answerId && x.User.Id == userId))
                {
                    return true;
                }
            }
            return false;
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
