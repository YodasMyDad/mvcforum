using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class BannedWordService : IBannedWordService
    {
        private readonly MVCForumContext _context;

        public BannedWordService(IMVCForumContext context)
        {
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
            if (onlyStopWords)
            {
                return _context.BannedWord.AsNoTracking().Where(x => x.IsStopWord == true).OrderByDescending(x => x.DateAdded).ToList();   
            }
            return _context.BannedWord.AsNoTracking().Where(x => x.IsStopWord != true).OrderByDescending(x => x.DateAdded).ToList();
        }

        public BannedWord Get(Guid id)
        {
            return _context.BannedWord.FirstOrDefault(x => x.Id == id);
        }

        public PagedList<BannedWord> GetAllPaged(int pageIndex, int pageSize)
        {
            var total = _context.BannedWord.Count();

            var results = _context.BannedWord
                                .AsNoTracking()
                                .OrderBy(x => x.Word)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<BannedWord>(results, pageIndex, pageSize, total);
        }

        public PagedList<BannedWord> GetAllPaged(string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);
            var total = _context.BannedWord.Count(x => x.Word.ToLower().Contains(search.ToLower()));

            var results = _context.BannedWord
                                .Where(x => x.Word.ToLower().Contains(search.ToLower()))
                                .OrderBy(x => x.Word)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<BannedWord>(results, pageIndex, pageSize, total);
        }

        public string SanitiseBannedWords(string content)
        {
            var bannedWords = GetAll().ToList();
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
