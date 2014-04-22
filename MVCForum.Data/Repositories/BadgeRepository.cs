using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Data.Context;
using System.Data.Entity;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class BadgeRepository : IBadgeRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public BadgeRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Get a badge by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Badge GetBadge(string name)
        {
            return _context.Badge.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Get badges
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Badge> GetAll()
        {
            return _context.Badge.ToList();
        }

        /// <summary>
        /// Add a new badge (expected id already assigned)
        /// </summary>
        /// <param name="newBadge"></param>
        public Badge Add(Badge newBadge)
        {
            return _context.Badge.Add(newBadge);
        }

        /// <summary>
        /// Generic single entity delete
        /// </summary>
        /// <param name="badge"></param>
        public void Delete(Badge badge)
        {
            badge.Users.Clear();
            _context.Badge.Remove(badge);
        }

        public void Update(Badge item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.Badge.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;  
        }

        /// <summary>
        /// Pages list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<Badge> GetPagedGroupedBadges(int pageIndex, int pageSize)
        {
            var totalCount = _context.Badge.Count();
            // Get the topics using an efficient
            var results = _context.Badge
                                .OrderByDescending(x => x.Name)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();


            // Return a paged list
            return new PagedList<Badge>(results, pageIndex, pageSize, totalCount);
        }

        public PagedList<Badge> SearchPagedGroupedBadges(string search, int pageIndex, int pageSize)
        {
            var totalCount = _context.Badge.Count(x => x.Name.ToUpper().Contains(search.ToUpper()));
            // Get the topics using an efficient
            var results = _context.Badge
                                .Where(x => x.Name.ToUpper().Contains(search.ToUpper()))
                                .OrderByDescending(x => x.Name)
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();


            // Return a paged list
            return new PagedList<Badge>(results, pageIndex, pageSize, totalCount);
        }

        public Badge Get(Guid id)
        {
            return _context.Badge.FirstOrDefault(badge => badge.Id == id);
        }

        public void DeleteTimeLastChecked(BadgeTypeTimeLastChecked badgeTypeTimeLastChecked)
        {
            _context.BadgeTypeTimeLastChecked.Remove(badgeTypeTimeLastChecked);
        }
    }
}
