namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.Entities;
    using Models.General;

    public partial interface IBannedEmailService : IContextService
    {
        BannedEmail Add(BannedEmail bannedEmail);
        void Delete(BannedEmail bannedEmail);
        IList<BannedEmail> GetAll();
        BannedEmail Get(Guid id);
        Task<PaginatedList<BannedEmail>> GetAllPaged(int pageIndex, int pageSize);
        Task<PaginatedList<BannedEmail>> GetAllPaged(string search, int pageIndex, int pageSize);
        IList<BannedEmail> GetAllWildCards();
        IList<BannedEmail> GetAllNonWildCards();
        bool EmailIsBanned(string email);
    }
}