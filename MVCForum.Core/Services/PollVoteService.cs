namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data.Entity;
    using Domain.DomainModel;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Domain.Constants;

    public partial class PollVoteService : IPollVoteService
    {
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        public PollVoteService(IMVCForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        public List<PollVote> GetAllPollVotes()
        {
            var cacheKey = string.Concat(CacheKeys.PollVote.StartsWith, "GetAllPollVotes");
            return _cacheService.CachePerRequest(cacheKey, () => _context.PollVote.ToList());
        }

        public PollVote Add(PollVote pollVote)
        {
            return _context.PollVote.Add(pollVote);
        }

        public bool HasUserVotedAlready(Guid answerId, Guid userId)
        {
            var cacheKey = string.Concat(CacheKeys.PollVote.StartsWith, "HasUserVotedAlready-", answerId, "-", userId);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var vote = _context.PollVote.Include(x => x.PollAnswer).Include(x => x.User).FirstOrDefault(x => x.PollAnswer.Id == answerId && x.User.Id == userId);
                return (vote != null);
            });

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
