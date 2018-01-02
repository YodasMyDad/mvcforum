namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Constants;
    using Data.Context;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;
    using Models.General;
    using Utilities;

    public partial class BannedEmailService : IBannedEmailService
    {
        private readonly IMvcForumContext _context;
        private readonly ICacheService _cacheService;

        public BannedEmailService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
        }

        public BannedEmail Add(BannedEmail bannedEmail)
        {
            return _context.BannedEmail.Add(bannedEmail);
        }

        public void Delete(BannedEmail bannedEmail)
        {
            _context.BannedEmail.Remove(bannedEmail);
        }

        public IList<BannedEmail> GetAll()
        {
            var cacheKey = string.Concat(CacheKeys.BannedEmail.StartsWith, "GetAll");
            return _cacheService.CachePerRequest(cacheKey, () => _context.BannedEmail.ToList());
        }

        public BannedEmail Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.BannedEmail.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.BannedEmail.FirstOrDefault(x => x.Id == id));
        }

        public async Task<PaginatedList<BannedEmail>> GetAllPaged(int pageIndex, int pageSize)
        {
            var query = _context.BannedEmail.OrderByDescending(x => x.Email);
            return await PaginatedList<BannedEmail>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        public async Task<PaginatedList<BannedEmail>> GetAllPaged(string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);

            var query = _context.BannedEmail
                .Where(x => x.Email.ToLower().Contains(search.ToLower()))
                .OrderByDescending(x => x.Email);
            return await PaginatedList<BannedEmail>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        public IList<BannedEmail> GetAllWildCards()
        {
            var cacheKey = string.Concat(CacheKeys.BannedEmail.StartsWith, "GetAllWildCards");
            return _cacheService.CachePerRequest(cacheKey, () => _context.BannedEmail.Where(x => x.Email.StartsWith("*@")).ToList());
        }

        public IList<BannedEmail> GetAllNonWildCards()
        {
            var cacheKey = string.Concat(CacheKeys.BannedEmail.StartsWith, "GetAllNonWildCards");
            return _cacheService.CachePerRequest(cacheKey, () => _context.BannedEmail.Where(x => !x.Email.StartsWith("*@")).ToList());
        }

        public bool EmailIsBanned(string email)
        {

            var cacheKey = string.Concat(CacheKeys.BannedEmail.StartsWith, "EmailIsBanned-", email);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {

                var domainBanned = false;

                // Sanitise the email
                var sanitisedEmail = StringUtils.SafePlainText(email).ToLower();

                // Split the email so we can get the domain out
                var emailDomain = ReturnDomainOnly(sanitisedEmail).ToLower();

                // Get all banned emails
                var allBannedEmails = GetAll();

                if (allBannedEmails.Any())
                {
                    // Now put them into two groups
                    var wildCardEmails = allBannedEmails.Where(x => x.Email.StartsWith("*@")).ToList();
                    var nonWildCardEmails = allBannedEmails.Except(wildCardEmails).ToList();

                    if (wildCardEmails.Any())
                    {
                        var wildCardDomains = wildCardEmails.Select(x => ReturnDomainOnly(x.Email));

                        // Firstly see if entire domain is banned
                        if (wildCardDomains.Any(domains => domains.ToLower() == emailDomain))
                        {
                            // Found so its banned
                            domainBanned = true;
                        }
                    }

                    // Domain is not banned so see if individual email is banned
                    if (nonWildCardEmails.Any())
                    {
                        if (nonWildCardEmails.Select(x => x.Email).Any(nonWildCardEmail => nonWildCardEmail.ToLower() == sanitisedEmail))
                        {
                            domainBanned = true;
                        }
                    }
                }

                return domainBanned;

            });
        }

        private static string ReturnDomainOnly(string email)
        {
            return email.Split('@')[1];
        }

    }
}
