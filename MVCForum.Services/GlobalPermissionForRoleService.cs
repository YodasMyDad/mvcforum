using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class GlobalPermissionForRoleService : IGlobalPermissionForRoleService
    {
        private readonly IGlobalPermissionForRoleRepository _globalPermissionForRoleRepository;

        public GlobalPermissionForRoleService(IGlobalPermissionForRoleRepository globalPermissionForRoleRepository)
        {
            _globalPermissionForRoleRepository = globalPermissionForRoleRepository;
        }


        public void Add(GlobalPermissionForRole permissionForRole)
        {
            _globalPermissionForRoleRepository.Add(permissionForRole);
        }

        public void Delete(GlobalPermissionForRole permissionForRole)
        {
            _globalPermissionForRoleRepository.Delete(permissionForRole);
        }

        public GlobalPermissionForRole CheckExists(GlobalPermissionForRole permissionForRole)
        {
            if (permissionForRole.Permission != null && permissionForRole.MembershipRole != null)
            {
                return _globalPermissionForRoleRepository.Get(permissionForRole.Permission.Id, permissionForRole.MembershipRole.Id);
            }

            return null;
        }

        public Dictionary<Permission, GlobalPermissionForRole> GetAll(MembershipRole role)
        {
            var catRowList = _globalPermissionForRoleRepository.GetAll(role);
            return catRowList.ToDictionary(catRow => catRow.Permission);
        }

        public Dictionary<Permission, GlobalPermissionForRole> GetAll()
        {
            var catRowList = _globalPermissionForRoleRepository.GetAll();
            return catRowList.ToDictionary(catRow => catRow.Permission);
        }

        public GlobalPermissionForRole Get(Guid permId, Guid roleId)
        {
            return _globalPermissionForRoleRepository.Get(permId, roleId);
        }

        public GlobalPermissionForRole Get(Guid permId)
        {
            return _globalPermissionForRoleRepository.Get(permId);
        }

        public void UpdateOrCreateNew(GlobalPermissionForRole globalPermissionForRole)
        {
            // Firstly see if this exists already
            var permission = CheckExists(globalPermissionForRole);

            // if it exists then just update it
            if (permission != null)
            {
                permission.IsTicked = globalPermissionForRole.IsTicked;
            }
            else
            {
                Add(globalPermissionForRole);
            }
        }
    }
}
