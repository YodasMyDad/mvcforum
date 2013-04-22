using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IBannedWordService
    {
        BannedWord Add(BannedWord bannedWord);
        void Delete(BannedWord bannedWord);
        IList<BannedWord> GetAll();
        BannedWord Get(Guid id);
        PagedList<BannedWord> GetAllPaged(int pageIndex, int pageSize);
        PagedList<BannedWord> GetAllPaged(string search, int pageIndex, int pageSize);
        string SanitiseBannedWords(string content);
        string SanitiseBannedWords(string content, IList<string> words);
    }
}
