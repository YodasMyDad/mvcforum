namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.Entities;
    using Models.General;

    public partial interface IBannedWordService : IContextService
    {
        BannedWord Add(BannedWord bannedWord);
        void Delete(BannedWord bannedWord);
        IList<BannedWord> GetAll(bool onlyStopWords = false);
        BannedWord Get(Guid id);
        Task<PaginatedList<BannedWord>> GetAllPaged(int pageIndex, int pageSize);
        Task<PaginatedList<BannedWord>> GetAllPaged(string search, int pageIndex, int pageSize);
        string SanitiseBannedWords(string content);
        string SanitiseBannedWords(string content, IList<string> words);
        bool ContainsStopWords(string content, IList<string> words);
    }
}