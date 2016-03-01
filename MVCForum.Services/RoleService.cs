﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Exceptions;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class RoleService : IRoleService
    {
        private readonly ICategoryPermissionForRoleService _categoryPermissionForRoleService;
        private readonly IGlobalPermissionForRoleService _globalPermissionForRoleService;
        private readonly IPermissionService _permissionService;
        private readonly MVCForumContext _context;
        private PermissionSet _permissions;

        public RoleService(IMVCForumContext context, ICategoryPermissionForRoleService categoryPermissionForRoleService, IPermissionService permissionService, IGlobalPermissionForRoleService globalPermissionForRoleService)
        {
            _categoryPermissionForRoleService = categoryPermissionForRoleService;
            _permissionService = permissionService;
            _globalPermissionForRoleService = globalPermissionForRoleService;
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Get all roles in the system
        /// </summary>
        /// <returns></returns>
        public IList<MembershipRole> AllRoles()
        {
            return _context.MembershipRole
                .OrderByDescending(x => x.RoleName)
                .ToList();
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        /// <param name="rolename"></param>
        /// <param name="removeTracking">If true, adds AsNoTracking()</param>
        /// <returns></returns>
        public MembershipRole GetRole(string rolename, bool removeTracking = false)
        {
            if (removeTracking)
            {
                return _context.MembershipRole
                    .Include(x => x.CategoryPermissionForRoles)
                    .Include(x => x.GlobalPermissionForRole)
                    .AsNoTracking()
                    .FirstOrDefault(y => y.RoleName.Contains(rolename));
            }
            return _context.MembershipRole.FirstOrDefault(y => y.RoleName.Contains(rolename));
        }

        /// <summary>
        /// Get role by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MembershipRole GetRole(Guid id)
        {
            return _context.MembershipRole.FirstOrDefault(x => x.Id == id);
        }

        /// <summary>
        /// Get all users for a specified role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public IList<MembershipUser> GetUsersForRole(string roleName)
        {
            return GetRole(roleName).Users;

        }
        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="role"></param>
        public MembershipRole CreateRole(MembershipRole role)
        {
            role.RoleName = StringUtils.SafePlainText(role.RoleName);
            var membershipRole = GetRole(role.RoleName);
            return membershipRole ?? _context.MembershipRole.Add(role);
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="role"></param>
        public void Delete(MembershipRole role)
        {
            // Check if anyone else if using this role
            var okToDelete = role.Users.Count == 0;

            if (okToDelete)
            {
                // Get any categorypermissionforoles and delete these first
                var rolesToDelete = _categoryPermissionForRoleService.GetByRole(role.Id);

                foreach (var categoryPermissionForRole in rolesToDelete)
                {
                    _categoryPermissionForRoleService.Delete(categoryPermissionForRole);
                }

                _context.MembershipRole.Remove(role);
            }
            else
            {
                var inUseBy = new List<Entity>();
                inUseBy.AddRange(role.Users);
                throw new InUseUnableToDeleteException(inUseBy);
            }
        }


        #region Methods

        /// <summary>
        /// Admin: so no need to check db, admin is all powerful
        /// </summary>
        private PermissionSet GetAdminPermissions(Category category, MembershipRole role)
        {
            // Get all permissions
            var permissionList = _permissionService.GetAll().ToList();

            // Make a new entry in the results against each permission. All true (this is admin) except "Deny Access" 
            // and "Read Only" which should be false

            // Category could be null if only requesting global permissions
            // Just return a new list
            var categoryPermissions = new List<CategoryPermissionForRole>();
            if (category != null)
            {
                foreach (var permission in permissionList.Where(x => !x.IsGlobal))
                {
                    categoryPermissions.Add(new CategoryPermissionForRole
                    {
                        Category = category,
                        IsTicked = (permission.Name != SiteConstants.Instance.PermissionDenyAccess && permission.Name != SiteConstants.Instance.PermissionReadOnly),
                        MembershipRole = role,
                        Permission = permission
                    });
                }

            }

            // Sort the global permissions out - As it's a admin we set everything to true!
            var globalPermissions = permissionList
                                        .Where(x => x.IsGlobal)
                                        .Select(permission => new GlobalPermissionForRole
                                                        {
                                                            IsTicked = true,
                                                            MembershipRole = role,
                                                            Permission = permission
                                                        });

            // Create the permission set
            return new PermissionSet(categoryPermissions, globalPermissions);
        }

        /// <summary>
        /// Guest = Not logged in, so only need to check the access permission
        /// </summary>
        /// <param name="category"></param>
        /// <param name="role"></param>
        private PermissionSet GetGuestPermissions(Category category, MembershipRole role)
        {
            // Get all the permissions 
            var permissionList = _permissionService.GetAll().ToList();

            // Make a CategoryPermissionForRole for each permission that exists,
            // but only set the read-only permission to true for this role / category. All others false

            // Category could be null if only requesting global permissions
            // Just return a new list
            var categoryPermissions = new List<CategoryPermissionForRole>();
            if (category != null)
            {
                foreach (var permission in permissionList.Where(x => !x.IsGlobal))
                {
                    categoryPermissions.Add(new CategoryPermissionForRole
                    {
                        Category = category,
                        IsTicked = permission.Name == SiteConstants.Instance.PermissionReadOnly,
                        MembershipRole = role,
                        Permission = permission
                    });
                }

                // Deny Access may have been set (or left null) for guest for the category, so need to read for it
                var denyAccessPermission = role.CategoryPermissionForRoles
                                   .FirstOrDefault(x => x.Category.Id == category.Id &&
                                                        x.Permission.Name == SiteConstants.Instance.PermissionDenyAccess &&
                                                        x.MembershipRole.Id == role.Id);

                // Set the Deny Access value in the results. If it's null for this role/category, record it as false in the results
                var categoryPermissionForRole = categoryPermissions.FirstOrDefault(x => x.Permission.Name == SiteConstants.Instance.PermissionDenyAccess);
                if (categoryPermissionForRole != null)
                {
                    categoryPermissionForRole.IsTicked = denyAccessPermission != null && denyAccessPermission.IsTicked;
                }
            }

            // Sort the global permissions out - As it's a guest we set everything to false
            var globalPermissions = new List<GlobalPermissionForRole>();
            foreach (var permission in permissionList.Where(x => x.IsGlobal))
            {
                globalPermissions.Add(new GlobalPermissionForRole
                {
                    IsTicked = false,
                    MembershipRole = role,
                    Permission = permission
                });
            }

            return new PermissionSet(categoryPermissions, globalPermissions);
        }

        /// <summary>
        /// Get permissions for roles other than those specially treated in this class
        /// </summary>
        /// <param name="category"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        private PermissionSet GetOtherPermissions(Category category, MembershipRole role)
        {
            // Get all permissions
            var permissionList = _permissionService.GetAll().ToList();

            var categoryPermissions = new List<CategoryPermissionForRole>();
            if (category != null)
            {
                // Get the known permissions for this role and category
                var categoryRow = _categoryPermissionForRoleService.GetCategoryRow(role, category);
                var categoryRowPermissions = categoryRow.ToDictionary(catRow => catRow.Key.Id);

                // Load up the results with the permisions for this role / cartegory. A null entry for a permissions results in a new
                // record with a false value
                foreach (var permission in permissionList.Where(x => !x.IsGlobal))
                {
                    categoryPermissions.Add(categoryRowPermissions.ContainsKey(permission.Id)
                                        ? categoryRowPermissions[permission.Id].Value
                                        : new CategoryPermissionForRole { Category = category, MembershipRole = role, IsTicked = false, Permission = permission });
                }
            }

            // Sort the global permissions out - As it's a guest we set everything to false
            var globalPermissions = new List<GlobalPermissionForRole>();

            // Get the known global permissions for this role
            var globalRow = _globalPermissionForRoleService.GetAll(role);
            var globalRowPermissions = globalRow.ToDictionary(row => row.Key.Id);

            // Load up the results with the permisions for this role. A null entry for a permissions results in a new
            // record with a false value
            foreach (var permission in permissionList.Where(x => x.IsGlobal))
            {
                globalPermissions.Add(globalRowPermissions.ContainsKey(permission.Id)
                                    ? globalRowPermissions[permission.Id].Value
                                    : new GlobalPermissionForRole { MembershipRole = role, IsTicked = false, Permission = permission });
            }

            return new PermissionSet(categoryPermissions, globalPermissions);
        }

        /// <summary>
        /// Returns permission set based on category and role
        /// </summary>
        /// <param name="category">Category could be null when requesting global permissions</param>
        /// <param name="role"></param>
        /// <returns></returns>
        public PermissionSet GetPermissions(Category category, MembershipRole role)
        {
            // Pass the role in to see select which permissions to apply
            // Going to cache this per request, just to help with performance

            // We pass in an empty guid if the category is null
            var categoryId = Guid.Empty;
            if (category != null)
            {
                categoryId = category.Id;
            }

            var objectContextKey = string.Concat(HttpContext.Current.GetHashCode().ToString("x"), "-", categoryId, "-", role.Id);
            if (!HttpContext.Current.Items.Contains(objectContextKey))
            {
                switch (role.RoleName)
                {
                    case AppConstants.AdminRoleName:
                        _permissions = GetAdminPermissions(category, role);
                        break;
                    case AppConstants.GuestRoleName:
                        _permissions = GetGuestPermissions(category, role);
                        break;
                    default:
                        _permissions = GetOtherPermissions(category, role);
                        break;
                }

                HttpContext.Current.Items.Add(objectContextKey, _permissions);
            }

            return HttpContext.Current.Items[objectContextKey] as PermissionSet;

        }

        #endregion



    }
}
