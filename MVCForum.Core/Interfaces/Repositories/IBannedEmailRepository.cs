using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface IBannedEmailRepository
    {
        BannedEmail Add(BannedEmail bannedEmail);
        void Delete(BannedEmail bannedEmail);
        IList<BannedEmail> GetAll();
        PagedList<BannedEmail> GetAllPaged(int pageIndex, int pageSize);
        IList<BannedEmail> GetAllWildCards();
        IList<BannedEmail> GetAllNonWildCards();
    }
}
