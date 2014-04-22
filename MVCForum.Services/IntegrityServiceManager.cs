using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class IntegrityServiceManager : IIntegrityServiceManager
    {
        private readonly List<IIntegrityService> _integrityServices;

        public IntegrityServiceManager()
        {
            _integrityServices = new List<IIntegrityService>();
        }

        /// <summary>
        /// Registers the integrity service
        /// </summary>
        /// <param name="service"></param>
        public void Register(IIntegrityService service)
        {
            if (!_integrityServices.Contains(service))
            {
                _integrityServices.Add(service);
            }
        }

        /// <summary>
        /// Checks whether the entity being deleted is in use by any other entities
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="inUseBy"></param>
        /// <returns></returns>
        public bool CanDelete(Entity entity, out List<Entity> inUseBy)
        {
            inUseBy = new List<Entity>();
            
            // Loop through all services asking each one if this entity can be deleted,
            // accumulating all the entities that use the entity we want to delete
            foreach (var service in _integrityServices)
            {
                List<Entity> inUseByEntity;
                if (!service.CanDelete(entity, out inUseByEntity))
                {
                    inUseBy.AddRange(inUseByEntity);
                }
            }

            return inUseBy.Count == 0;
        }
    }
}
