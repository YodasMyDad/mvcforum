namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data.Entity;
    using Domain.DomainModel;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Domain.Constants;

    public partial class GlobalPermissionForRoleService : IGlobalPermissionForRoleService
    {
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        public GlobalPermissionForRoleService(IMVCForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
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
            var cacheKey = string.Concat(CacheKeys.GlobalPermissionForRole.StartsWith, "GetAll-", role.Id);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var catRowList = _context.GlobalPermissionForRole.Include(x => x.MembershipRole).Where(x => x.MembershipRole.Id == role.Id).ToList();
                return catRowList.ToDictionary(catRow => catRow.Permission);
            });
        }

        public Dictionary<Permission, GlobalPermissionForRole> GetAll()
        {
            var cacheKey = string.Concat(CacheKeys.GlobalPermissionForRole.StartsWith, "GetAll");
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var catRowList = _context.GlobalPermissionForRole.Include(x => x.MembershipRole).ToList();
                return catRowList.ToDictionary(catRow => catRow.Permission);
            });
        }

        public GlobalPermissionForRole Get(Guid permId, Guid roleId)
        {
            var cacheKey = string.Concat(CacheKeys.GlobalPermissionForRole.StartsWith, "Get-", permId, "-", roleId);
            return _cacheService.CachePerRequest(cacheKey, () => _context.GlobalPermissionForRole
                                                                            .Include(x => x.MembershipRole)
                                                                            .FirstOrDefault(x => x.Permission.Id == permId && x.MembershipRole.Id == roleId));
        }

        public GlobalPermissionForRole Get(Guid permId)
        {
            var cacheKey = string.Concat(CacheKeys.GlobalPermissionForRole.StartsWith, "Get-", permId);
            return _cacheService.CachePerRequest(cacheKey, () => _context.GlobalPermissionForRole
                                                                             .Include(x => x.MembershipRole)
                                                                             .FirstOrDefault(x => x.Id == permId));
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
