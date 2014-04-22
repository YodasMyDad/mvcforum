using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class BannedEmailService : IBannedEmailService
    {
        private readonly IBannedEmailRepository _bannedEmailRepository;

        public BannedEmailService(IBannedEmailRepository bannedEmailRepository)
        {
            _bannedEmailRepository = bannedEmailRepository;
        }
        public BannedEmail Add(BannedEmail bannedEmail)
        {
            return _bannedEmailRepository.Add(bannedEmail);
        }

        public void Delete(BannedEmail bannedEmail)
        {
            _bannedEmailRepository.Delete(bannedEmail);
        }

        public IList<BannedEmail> GetAll()
        {
            return _bannedEmailRepository.GetAll();
        }

        public BannedEmail Get(Guid id)
        {
            return _bannedEmailRepository.Get(id);
        }

        public PagedList<BannedEmail> GetAllPaged(int pageIndex, int pageSize)
        {
            return _bannedEmailRepository.GetAllPaged(pageIndex, pageSize);
        }

        public PagedList<BannedEmail> GetAllPaged(string search, int pageIndex, int pageSize)
        {
            return _bannedEmailRepository.GetAllPaged(StringUtils.SafePlainText(search), pageIndex, pageSize);
        }

        public IList<BannedEmail> GetAllWildCards()
        {
            return _bannedEmailRepository.GetAllWildCards();
        }

        public IList<BannedEmail> GetAllNonWildCards()
        {
            return _bannedEmailRepository.GetAllNonWildCards();
        }

        public bool EmailIsBanned(string email)
        {
            var domainBanned = false;

            // Sanitise the email
            var sanitisedEmail = StringUtils.SafePlainText(email).ToLower();

            // Split the email so we can get the domain out
            var emailDomain = ReturnDomainOnly(sanitisedEmail).ToLower();
            
            // Get all banned emails
            var allBannedEmails = _bannedEmailRepository.GetAll();

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
        }

        private string ReturnDomainOnly(string email)
        {
            return email.Split('@')[1];
        }

    }
}
