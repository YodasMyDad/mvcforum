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
    using Utilities;

    public partial class PollAnswerService : IPollAnswerService
    {
        private readonly IMvcForumContext _context;
        private readonly ICacheService _cacheService;

        public PollAnswerService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
        }

        public List<PollAnswer> GetAllPollAnswers()
        {
            return _context.PollAnswer.Include(x => x.Poll).ToList();
        }

        public PollAnswer Add(PollAnswer pollAnswer)
        {
            pollAnswer.Answer = StringUtils.SafePlainText(pollAnswer.Answer);
            return _context.PollAnswer.Add(pollAnswer);
        }

        public List<PollAnswer> GetAllPollAnswersByPoll(Poll poll)
        {
            return _context.PollAnswer
                            .Include(x => x.Poll)
                            .AsNoTracking()
                            .Where(x => x.Poll.Id == poll.Id).ToList();            
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
