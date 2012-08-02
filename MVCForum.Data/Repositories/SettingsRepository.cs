using System;
using System.Data;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;


namespace MVCForum.Data.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        /// <param name="cacheHelper"> </param>
        public SettingsRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public Settings GetSettings()
        {
            var settings = _context.Setting.FirstOrDefault();
            return settings;
        }

        public Settings Add(Settings item)
        {
            return _context.Setting.Add(item);
        }

        public Settings Get(Guid id)
        {
            return _context.Setting.FirstOrDefault(x => x.Id == id);
        }

        public void Update(Settings item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.Setting.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;  
        }
    }
}
