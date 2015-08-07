using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class BannedWordRepository : IBannedWordRepository
    {
        private readonly MVCForumContext _context;
        public BannedWordRepository(IMVCForumContext context)
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

        public IList<BannedWord> GetAll()
        {
            return _context.BannedWord.AsNoTracking().OrderByDescending(x => x.DateAdded).ToList();
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
            var total = _context.BannedWord.Count(x => x.Word.ToLower().Contains(search.ToLower()));

            var results = _context.BannedWord
                                .Where(x => x.Word.ToLower().Contains(search.ToLower()))
                                .OrderBy(x => x.Word)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedList<BannedWord>(results, pageIndex, pageSize, total);
        }
    }
}
