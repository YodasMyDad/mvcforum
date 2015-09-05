using System;
using System.Linq;
using MVCForum.Data.Context;
using System.Data.Entity;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class SettingsRepository : ISettingsRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public SettingsRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public Settings GetSettings(bool addTracking)
        {
            var settings = _context.Setting
                .Include(x => x.DefaultLanguage)
                .Include(x => x.NewMemberStartingRole);
            return addTracking ? settings.FirstOrDefault() : settings.AsNoTracking().FirstOrDefault();
        }

        public Settings Add(Settings item)
        {
            return _context.Setting.Add(item);
        }

        public Settings Get(Guid id)
        {
            return _context.Setting.FirstOrDefault(x => x.Id == id);
        }

    }
}
