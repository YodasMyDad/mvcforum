namespace MvcForum.Core.Interfaces.Services
{
    using System.Threading.Tasks;

    public partial interface IContextService
    {
        /// <summary>
        ///     When using pipelines, we need to make sure we are using the same context throughout, or we'll end up with an error
        ///     like
        ///     'An entity object cannot be referenced by multiple instances of IEntityChangeTracker'. So we pass in the existing
        ///     context
        ///     and it replaces the one created by the IoC provider
        /// </summary>
        /// <param name="context"></param>
        void RefreshContext(IMvcForumContext context);
        Task<int> SaveChanges();
    }
}