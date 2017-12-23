namespace MVCForum.Services
{
    using Domain.Constants;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Domain.DomainModel;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Utilities;

    public partial class BannedWordService : IBannedWordService
    {
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        public BannedWordService(IMVCForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        public BannedWord Add(BannedWord bannedWord)
        {
            return _context.BannedWord.Add(bannedWord);
        }

        public void Delete(BannedWord bannedWord)
        {
            _context.BannedWord.Remove(bannedWord);
        }

        public IList<BannedWord> GetAll(bool onlyStopWords = false)
        {
            var cacheKey = string.Concat(CacheKeys.BannedWord.StartsWith, "GetAll-", onlyStopWords);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                if (onlyStopWords)
                {
                    return _context.BannedWord.AsNoTracking().Where(x => x.IsStopWord == true).OrderByDescending(x => x.DateAdded).ToList();
                }
                return _context.BannedWord.AsNoTracking().Where(x => x.IsStopWord != true).OrderByDescending(x => x.DateAdded).ToList();
            });
        }

        public BannedWord Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.BannedWord.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.BannedWord.FirstOrDefault(x => x.Id == id));
        }

        public PagedList<BannedWord> GetAllPaged(int pageIndex, int pageSize)
        {
            var cacheKey = string.Concat(CacheKeys.BannedWord.StartsWith, "GetAllPaged-", pageIndex, "-", pageSize);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var total = _context.BannedWord.Count();
                var results = _context.BannedWord
                                    .AsNoTracking()
                                    .OrderBy(x => x.Word)
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

                return new PagedList<BannedWord>(results, pageIndex, pageSize, total);
            });
        }

        public PagedList<BannedWord> GetAllPaged(string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);

            var cacheKey = string.Concat(CacheKeys.BannedWord.StartsWith, "GetAllPaged-", search, "-", pageIndex, "-", pageSize);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var total = _context.BannedWord.Count(x => x.Word.ToLower().Contains(search.ToLower()));
                var results = _context.BannedWord
                                    .Where(x => x.Word.ToLower().Contains(search.ToLower()))
                                    .OrderBy(x => x.Word)
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();

                return new PagedList<BannedWord>(results, pageIndex, pageSize, total);
            });
        }

        public string SanitiseBannedWords(string content)
        {
            var bannedWords = GetAll();
            if (bannedWords.Any())
            {
                return SanitiseBannedWords(content, bannedWords.Select(x => x.Word).ToList());
            }
            return content;
        }

        public string SanitiseBannedWords(string content, IList<string> words)
        {
            if (words != null && words.Any())
            {
                var censor = new CensorUtils(words);
                return censor.CensorText(content);
            }
            return content;
        }
    }
}
