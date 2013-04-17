using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public class BannedEmailService : IBannedEmailService
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
            throw new NotImplementedException();
        }
    }
}
