namespace MvcForum.Core.Data.UnitOfWork
{
    using System.Data.Entity;
    using Constants;
    using Context;
    using Interfaces.UnitOfWork;
    using Interfaces;
    using Services.Migrations;

    public class UnitOfWorkManager : IUnitOfWorkManager
    {
        private bool _isDisposed;
        private readonly MvcForumContext _context;

        public UnitOfWorkManager(IMvcForumContext context)
        {
            //http://www.entityframeworktutorial.net/code-first/automated-migration-in-code-first.aspx
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MvcForumContext, Configuration>(SiteConstants.Instance.MvcForumContext));
            _context = context as MvcForumContext;
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
