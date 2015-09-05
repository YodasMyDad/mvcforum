using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Data.UnitOfWork
{
    public class UnitOfWorkManager : IUnitOfWorkManager
    {
        private bool _isDisposed;
        private readonly MVCForumContext _context;

        public UnitOfWorkManager(IMVCForumContext context)
        {
            //http://www.entityframeworktutorial.net/code-first/automated-migration-in-code-first.aspx
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MVCForumContext, Migrations.Configuration>(AppConstants.MvcForumContext));
            //Database.SetInitializer<MVCForumContext>(null);
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Provides an instance of a unit of work. This wrapping in the manager
        /// class helps keep concerns separated
        /// </summary>
        /// <returns></returns>
        public IUnitOfWork NewUnitOfWork()
        {
            return new UnitOfWork(_context);
        }

        /// <summary>
        /// Make sure there are no open sessions.
        /// In the web app this will be called when the injected UnitOfWork manager
        /// is disposed at the end of a request.
        /// </summary>
        public void Dispose()
        {
            if (!_isDisposed)
            {
                _context.Dispose();
                _isDisposed = true;
            }
        }
    }
}
