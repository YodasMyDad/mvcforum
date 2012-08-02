using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    /// <summary>
    /// A manager to coordinate data integrity issues between other services.
    /// To use this those services must implement IIntegrityService, and accept
    /// an (injected) instance of this manager class. They can then register
    /// themselves with the manager.
    /// </summary>
    public interface IIntegrityServiceManager
    {
        /// <summary>
        /// Register a service - include in collection of services
        /// </summary>
        /// <param name="service"></param>
        void Register(IIntegrityService service);

        /// <summary>
        /// Check all registered services whether the entity can be deleted
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <param name="inUseBy">Set of entities that use this entity, and therefore block deletion </param>
        /// <returns>True if ok to delete, else false and caller can check inUseBy list</returns>
        bool CanDelete(Entity entity, out List<Entity> inUseBy);
    }
}
