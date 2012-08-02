using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public class ActivityRepository : IActivityRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public ActivityRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Get activities
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Activity> GetAll()
        {
            return _context.Activity.ToList();
        }

        /// <summary>
        /// Add a new activity (expected id already assigned)
        /// </summary>
        /// <param name="newActivity"></param>
        public Activity Add(Activity newActivity)
        {
            newActivity.Id = Guid.NewGuid();
            return _context.Activity.Add(newActivity);
        }

        /// <summary>
        /// Paged list of activities
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<Activity> GetPagedGroupedActivities(int pageIndex, int pageSize)
        {
            var results = _context.Activity
                  .OrderByDescending(x => x.Timestamp)
                  .Skip((pageIndex - 1) * pageSize)
                  .Take(pageSize)
                  .ToList();

            // Return a paged list
            return new PagedList<Activity>(results, pageIndex, pageSize, results.Count);
        }

        public PagedList<Activity> SearchPagedGroupedActivities(string search, int pageIndex, int pageSize)
        {
            // Get the topics using an efficient
            var results = _context.Activity
                  .Where(x => x.Type.ToUpper().Contains(search.ToUpper()))
                  .OrderByDescending(x => x.Timestamp)
                  .Skip((pageIndex - 1) * pageSize)
                  .Take(pageSize)
                  .ToList();


            // Return a paged list
            return new PagedList<Activity>(results, pageIndex, pageSize, results.Count);
        }

        public Activity Get(Guid id)
        {
            return _context.Activity.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(Activity item)
        {
            _context.Activity.Remove(item);
        }

        public void Update(Activity item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.Activity.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;  
        }
    }
}
