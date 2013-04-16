using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

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

        public PagedList<BannedEmail> GetAllPaged(int pageIndex, int pageSize)
        {
            return _bannedEmailRepository.GetAllPaged(pageIndex, pageSize);
        }

        public IList<BannedEmail> GetAllWildCards()
        {
            return _bannedEmailRepository.GetAllWildCards();
        }

        public IList<BannedEmail> GetAllNonWildCards()
        {
            return _bannedEmailRepository.GetAllNonWildCards();
        }
    }
}
