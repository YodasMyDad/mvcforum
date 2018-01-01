namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Constants;
    using Data.Context;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;

    public partial class PollService : IPollService
    {
        private readonly ICacheService _cacheService;
        private readonly IMvcForumContext _context;

        public PollService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
        }

        public List<Poll> GetAllPolls()
        {
            var cacheKey = string.Concat(CacheKeys.Poll.StartsWith, "GetAllPolls");
            return _cacheService.CachePerRequest(cacheKey, () => _context.Poll.ToList());
        }

        public Poll Add(Poll poll)
        {
            poll.DateCreated = DateTime.UtcNow;
            poll.IsClosed = false;
            return _context.Poll.Add(poll);
        }

        public Poll Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.Poll.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Poll.FirstOrDefault(x => x.Id == id));
        }

        public void Delete(Poll item)
        {
            _context.Poll.Remove(item);
        }
    }
}