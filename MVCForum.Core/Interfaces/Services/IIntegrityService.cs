using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    /// <summary>
    /// Services can implement this service if they want integrity
    /// issues to be coordinated by the Integrity Manager instance
    /// </summary>
    public partial interface IIntegrityService
    {
        /// <summary>
        /// If the specified entity is clear for deletion, return true.
        /// For example, return false if this entity is in use (e.g.
        /// a Team that still has Members)
        /// </summary>
        /// <param name="entity">Entity to check, i.e. someone wants to delete this, ok?</param>
        /// <param name="inUseBy">List of entities that use the delete request entity</param>
        /// <returns>True if OK to delete the requested entity. If false, caller can check contents of inUseBy list</returns>
        bool CanDelete(Entity entity, out List<Entity> inUseBy);
    }
}
