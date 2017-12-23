namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Domain.DomainModel;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Domain.Constants;
    using Utilities;

    public partial class PollAnswerService : IPollAnswerService
    {
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        public PollAnswerService(IMVCForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        public List<PollAnswer> GetAllPollAnswers()
        {
            var cacheKey = string.Concat(CacheKeys.PollAnswer.StartsWith, "GetAllPollAnswers");
            return _cacheService.CachePerRequest(cacheKey, () => _context.PollAnswer.Include(x => x.Poll).ToList());
        }

        public PollAnswer Add(PollAnswer pollAnswer)
        {
            pollAnswer.Answer = StringUtils.SafePlainText(pollAnswer.Answer);
            return _context.PollAnswer.Add(pollAnswer);
        }

        public List<PollAnswer> GetAllPollAnswersByPoll(Poll poll)
        {
            var cacheKey = string.Concat(CacheKeys.PollAnswer.StartsWith, "GetAllPollAnswersByPoll-", poll.Id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PollAnswer
                                                                    .Include(x => x.Poll)
                                                                    .AsNoTracking()
                                                                    .Where(x => x.Poll.Id == poll.Id).ToList());            
        }

        public PollAnswer Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.PollAnswer.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.PollAnswer.FirstOrDefault(x => x.Id == id));
        }

        public void Delete(PollAnswer pollAnswer)
        {
            _context.PollAnswer.Remove(pollAnswer);
        }

    }
}
