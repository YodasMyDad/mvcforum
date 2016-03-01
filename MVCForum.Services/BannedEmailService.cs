﻿using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class BannedEmailService : IBannedEmailService
    {
        private readonly MVCForumContext _context;

        public BannedEmailService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
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
            return _context.BannedEmail.ToList();
        }

        public BannedEmail Get(Guid id)
        {
            return _context.BannedEmail.FirstOrDefault(x => x.Id == id);
        }

        public PagedList<BannedEmail> GetAllPaged(int pageIndex, int pageSize)
        {
            var total = _context.BannedEmail.Count();

            var results = _context.BannedEmail
                                .OrderByDescending(x => x.Email)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<BannedEmail>(results, pageIndex, pageSize, total);
        }

        public PagedList<BannedEmail> GetAllPaged(string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);
            var total = _context.BannedEmail.Count(x => x.Email.ToLower().Contains(search.ToLower()));

            var results = _context.BannedEmail
                                .Where(x => x.Email.ToLower().Contains(search.ToLower()))
                                .OrderByDescending(x => x.Email)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<BannedEmail>(results, pageIndex, pageSize, total);
        }

        public IList<BannedEmail> GetAllWildCards()
        {
            return _context.BannedEmail.Where(x => x.Email.StartsWith("*@")).ToList();
        }

        public IList<BannedEmail> GetAllNonWildCards()
        {
            return _context.BannedEmail.Where(x => !x.Email.StartsWith("*@")).ToList();
        }

        public bool EmailIsBanned(string email)
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
        }

        private static string ReturnDomainOnly(string email)
        {
            return email.Split('@')[1];
        }

    }
}
