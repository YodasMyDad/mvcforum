using System;
using System.Linq;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class PermissionService : IPermissionService
    {
        private readonly MVCForumContext _context;
        private readonly ICategoryPermissionForRoleService _categoryPermissionForRoleService;

        public PermissionService(ICategoryPermissionForRoleService categoryPermissionForRoleService, IMVCForumContext context)
        {
            _categoryPermissionForRoleService = categoryPermissionForRoleService;
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Get all permissions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Permission> GetAll()
        {
            return _context.Permission
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToList();
        }

        /// <summary>
        /// Add a new permission
        /// </summary>
        /// <param name="permission"></param>
        public Permission Add(Permission permission)
        {
            permission.Name = StringUtils.SafePlainText(permission.Name);
            return _context.Permission.Add(permission);
        }

        /// <summary>
        ///  Delete permission and associated category permission for roles
        /// </summary>
        /// <param name="permission"></param>
        public void Delete(Permission permission)
        {
            var catPermForRoles = _categoryPermissionForRoleService.GetByPermission(permission.Id);
            foreach (var categoryPermissionForRole in catPermForRoles)
            {
                _categoryPermissionForRoleService.Delete(categoryPermissionForRole);
            }

            _context.Permission.Remove(permission);
        }

        /// <summary>
        /// Get a permision by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Permission Get(Guid id)
        {
            return _context.Permission.FirstOrDefault(x => x.Id == id);
        }
    }
}
