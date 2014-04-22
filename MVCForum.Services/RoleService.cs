using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Exceptions;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ICategoryPermissionForRoleRepository _categoryPermissionForRoleRepository;
        private readonly IPermissionRepository _permissionRepository;

        private PermissionSet _permissions;

        public RoleService(IRoleRepository roleRepository, ICategoryPermissionForRoleRepository categoryPermissionForRoleRepository, IPermissionRepository permissionRepository)
        {
            _roleRepository = roleRepository;
            _categoryPermissionForRoleRepository = categoryPermissionForRoleRepository;
            _permissionRepository = permissionRepository;            
        }

        /// <summary>
        /// Get all roles in the system
        /// </summary>
        /// <returns></returns>
        public IList<MembershipRole> AllRoles()
        {
                return _roleRepository.AllRoles();
        }

        /// <summary>
        /// Get role by name
        /// </summary>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public MembershipRole GetRole(string rolename)
        {
                return _roleRepository.GetRole(rolename);
        }

        /// <summary>
        /// Get role by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MembershipRole GetRole(Guid Id)
        {
                return _roleRepository.Get(Id);

        }

        /// <summary>
        /// Get all users for a specified role
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public IList<MembershipUser> GetUsersForRole(string roleName)
        {
                return _roleRepository.GetRole(roleName).Users;

        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="role"></param>
        public void CreateRole(MembershipRole role)
        {
            role.RoleName = StringUtils.SafePlainText(role.RoleName);
            _roleRepository.Add(role);
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
                        var rolesToDelete = _categoryPermissionForRoleRepository.GetByRole(role.Id);

                        foreach (var categoryPermissionForRole in rolesToDelete)
                        {
                            _categoryPermissionForRoleRepository.Delete(categoryPermissionForRole);
                        }

                        _roleRepository.Delete(role);
                }
                else
                {
                    var inUseBy = new List<Entity>();
                    inUseBy.AddRange(role.Users);
                    throw new InUseUnableToDeleteException(inUseBy);
                }
        }

        /// <summary>
        /// Save a role
        /// </summary>
        /// <param name="role"></param>
        public void Save(MembershipRole role)
        {
            role.RoleName = StringUtils.SafePlainText(role.RoleName);
            _roleRepository.Update(role);  
        }


        #region Methods

        /// <summary>
        /// Admin: so no need to check db, admin is all powerful
        /// </summary>
        private PermissionSet GetAdminPermissions(Category category, MembershipRole role)
        {
            // Get all permissions
                    var permissionList = _permissionRepository.GetAll();

                    // Make a new entry in the results against each permission. All true (this is admin) except "Deny Access" 
                    // and "Read Only" which should be false
                    var permissionSet = new PermissionSet(permissionList.Select(permission => new CategoryPermissionForRole
                                                                                                            {
                                                                                                                Category = category,
                                                                                                                IsTicked = (permission.Name != AppConstants.PermissionDenyAccess && permission.Name != AppConstants.PermissionReadOnly),
                                                                                                                MembershipRole = role,
                                                                                                                Permission = permission
                                                                                                            }).ToList());


            return permissionSet;

        }

        /// <summary>
        /// Guest = Not logged in, so only need to check the access permission
        /// </summary>
        /// <param name="category"></param>
        /// <param name="role"></param>
        private PermissionSet GetGuestPermissions(Category category, MembershipRole role)
        {
            // Get all the permissions 
                    var permissionList = _permissionRepository.GetAll();

                    // Make a CategoryPermissionForRole for each permission that exists,
                    // but only set the read-only permission to true for this role / category. All others false
                    var permissions = permissionList.Select(permission => new CategoryPermissionForRole
                    {
                        Category = category,
                        IsTicked = permission.Name == AppConstants.PermissionReadOnly,
                        MembershipRole = role,
                        Permission = permission
                    }).ToList();

                    // Deny Access may have been set (or left null) for guest for the category, so need to read for it
                    var denyAccessPermission = role.CategoryPermissionForRole
                                       .FirstOrDefault(x => x.Category == category &&
                                                            x.Permission.Name == AppConstants.PermissionDenyAccess &&
                                                            x.MembershipRole == role);

                    // Set the Deny Access value in the results. If it's null for this role/category, record it as false in the results
                    var categoryPermissionForRole = permissions.FirstOrDefault(x => x.Permission.Name == AppConstants.PermissionDenyAccess);
                    if (categoryPermissionForRole != null)
                    {
                        categoryPermissionForRole.IsTicked = denyAccessPermission != null && denyAccessPermission.IsTicked;
                    }

                    var permissionSet = new PermissionSet(permissions);


            return permissionSet;

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
                    var permissionList = _permissionRepository.GetAll();

                    // Get the known permissions for this role and category
                    var categoryRow = _categoryPermissionForRoleRepository.GetCategoryRow(role, category);
                    var categoryRowPermissions = categoryRow.ToDictionary(catRow => catRow.Permission);

                    // Load up the results with the permisions for this role / cartegory. A null entry for a permissions results in a new
                    // record with a false value
                    var permissions = new List<CategoryPermissionForRole>();
                    foreach (var permission in permissionList)
                    {
                        permissions.Add(categoryRowPermissions.ContainsKey(permission)
                                            ? categoryRowPermissions[permission]
                                            : new CategoryPermissionForRole { Category = category, MembershipRole = role, IsTicked = false, Permission = permission });
                    }

                    var permissionSet = new PermissionSet(permissions);

            return permissionSet;

        }

        /// <summary>
        /// Returns permission set based on category and role
        /// </summary>
        /// <param name="category"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public PermissionSet GetPermissions(Category category, MembershipRole role)
        {
            // Pass the role in to see select which permissions to apply
            // Going to cache this per request, just to help with performance
            var objectContextKey = string.Concat(HttpContext.Current.GetHashCode().ToString("x"), "-", category.Id, "-", role.Id);
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
