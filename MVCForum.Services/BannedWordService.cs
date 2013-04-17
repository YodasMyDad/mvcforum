using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public class BannedWordService : IBannedWordService
    {
        private readonly IBannedWordRepository _bannedWordRepository;

        public BannedWordService(IBannedWordRepository bannedWordRepository)
        {
            _bannedWordRepository = bannedWordRepository;
        }

        public BannedWord Add(BannedWord bannedWord)
        {
            return _bannedWordRepository.Add(bannedWord);
        }

        public void Delete(BannedWord bannedWord)
        {
            _bannedWordRepository.Delete(bannedWord);
        }

        public IList<BannedWord> GetAll()
        {
            return _bannedWordRepository.GetAll();
        }

        public BannedWord Get(Guid id)
        {
            return _bannedWordRepository.Get(id);
        }

        public PagedList<BannedWord> GetAllPaged(int pageIndex, int pageSize)
        {
            return _bannedWordRepository.GetAllPaged(pageIndex, pageSize);
        }

        public PagedList<BannedWord> GetAllPaged(string search, int pageIndex, int pageSize)
        {
            return _bannedWordRepository.GetAllPaged(search, pageIndex, pageSize);
        }

        public string SanitiseBannedWords(string content)
        {
            var bannedWords = GetAll();
            if (bannedWords.Any())
            {
                var censor = new CensorUtils(bannedWords.Select(x => x.Word).ToList());
                return censor.CensorText(content);   
            }
            return content;
        }
    }
}
