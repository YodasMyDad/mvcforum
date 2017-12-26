namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using DomainModel.Entities;
    using DomainModel.General;

    public partial interface IBannedWordService
    {
        BannedWord Add(BannedWord bannedWord);
        void Delete(BannedWord bannedWord);
        IList<BannedWord> GetAll(bool onlyStopWords = false);
        BannedWord Get(Guid id);
        PagedList<BannedWord> GetAllPaged(int pageIndex, int pageSize);
        PagedList<BannedWord> GetAllPaged(string search, int pageIndex, int pageSize);
        string SanitiseBannedWords(string content);
        string SanitiseBannedWords(string content, IList<string> words);
    }
}