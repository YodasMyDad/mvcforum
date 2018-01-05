namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Constants;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;

    public partial class PollVoteService : IPollVoteService
    {
        private readonly ICacheService _cacheService;
        private readonly IMvcForumContext _context;

        public PollVoteService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
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
                var vote = _context.PollVote.Include(x => x.PollAnswer).Include(x => x.User)
                    .FirstOrDefault(x => x.PollAnswer.Id == answerId && x.User.Id == userId);
                return vote != null;
        }

        public PollVote Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.PollVote.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PollVote.FirstOrDefault(x => x.Id == id));
        }

        public void Delete(PollVote pollVote)
        {
            _context.PollVote.Remove(pollVote);
        }
    }
}