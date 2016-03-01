using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{
    public partial class GlobalPermissionForRoleService : IGlobalPermissionForRoleService
    {
        private readonly MVCForumContext _context;
        public GlobalPermissionForRoleService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public GlobalPermissionForRole Add(GlobalPermissionForRole permissionForRole)
        {
            return _context.GlobalPermissionForRole.Add(permissionForRole);
        }

        public void Delete(GlobalPermissionForRole permissionForRole)
        {
            _context.GlobalPermissionForRole.Remove(permissionForRole);
        }

        public GlobalPermissionForRole CheckExists(GlobalPermissionForRole permissionForRole)
        {
            if (permissionForRole.Permission != null && permissionForRole.MembershipRole != null)
            {
                return Get(permissionForRole.Permission.Id, permissionForRole.MembershipRole.Id);
            }

            return null;
        }

        public Dictionary<Permission, GlobalPermissionForRole> GetAll(MembershipRole role)
        {
            var catRowList =  _context.GlobalPermissionForRole.Include(x => x.MembershipRole).Where(x => x.MembershipRole.Id == role.Id).ToList();
            return catRowList.ToDictionary(catRow => catRow.Permission);
        }

        public Dictionary<Permission, GlobalPermissionForRole> GetAll()
        {
            var catRowList = _context.GlobalPermissionForRole.Include(x => x.MembershipRole).ToList();
            return catRowList.ToDictionary(catRow => catRow.Permission);
        }

        public GlobalPermissionForRole Get(Guid permId, Guid roleId)
        {
            return _context.GlobalPermissionForRole.Include(x => x.MembershipRole).FirstOrDefault(x => x.Permission.Id == permId && x.MembershipRole.Id == roleId);
        }

        public GlobalPermissionForRole Get(Guid permId)
        {
            return _context.GlobalPermissionForRole.Include(x => x.MembershipRole).FirstOrDefault(x => x.Id == permId);
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
