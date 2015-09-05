using System;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public partial class PermissionsController : BaseAdminController
    {
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryPermissionForRoleService _categoryPermissionForRoleService;
        private readonly IGlobalPermissionForRoleService _globalPermissionForRoleService;

        public PermissionsController(ILoggingService loggingService, 
                                    IUnitOfWorkManager unitOfWorkManager, 
                                    IRoleService roleService,
                                    ILocalizationService localizationService,
                                    IPermissionService permissionService,
                                    ICategoryService categoryService,
                                    ICategoryPermissionForRoleService categoryPermissionForRoleService,
                                    IMembershipService membershipService,
                                    ISettingsService settingsService, 
                                    IGlobalPermissionForRoleService globalPermissionForRoleService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _roleService = roleService;
            _permissionService = permissionService;
            _categoryService = categoryService;
            _categoryPermissionForRoleService = categoryPermissionForRoleService;
            _globalPermissionForRoleService = globalPermissionForRoleService;
        }

        /// <summary>
        /// List of roles to apply permissions to
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var permViewModel = new ChoosePermissionsViewModel
                                        {
                                            MembershipRoles = _roleService.AllRoles().ToList(),
                                            Permissions = _permissionService.GetAll().ToList()
                                        };
                return View(permViewModel);
            }
        }

        /// <summary>
        /// Add / Remove permissions for a role on each category
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditPermissions(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var role = _roleService.GetRole(id);
                var permViewModel = new EditPermissionsViewModel
                                        {
                                            MembershipRole = role,
                                            Permissions = _permissionService.GetAll().ToList(),
                                            Categories = _categoryService.GetAll(),
                                            CurrentGlobalPermissions = _roleService.GetPermissions(null, role)
                                        };

                return View(permViewModel);
            }
        }

        public ActionResult PermissionTypes()
        {
            var permViewModel = new ChoosePermissionsViewModel{
                Permissions = _permissionService.GetAll().ToList()
            };
            return View(permViewModel);
        }

        /// <summary>
        /// Add a new permission type into the permission table
        /// </summary>
        /// <returns></returns>
        public ActionResult AddType()
        {
            return View(new AddTypeViewModel());
        }

        [HttpPost]
        public ActionResult AddType(AddTypeViewModel permissionViewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var permission = new Permission
                                         {
                                             Name = permissionViewModel.Name,
                                             IsGlobal = permissionViewModel.IsGlobal
                                         };
                        
                    _permissionService.Add(permission);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Permission Added",
                        MessageType = GenericMessages.success
                    };
                    unitOfWork.Commit();
                }
                catch(Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex); 
                    throw;
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public void UpdatePermission(AjaxEditPermissionViewModel ajaxEditPermissionViewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    if (Request.IsAjaxRequest())
                    {
                        if (ajaxEditPermissionViewModel.Category == Guid.Empty)
                        {
                            // If category is empty guid then this is a global permission

                            var gpr = new GlobalPermissionForRole
                            {
                                MembershipRole =
                                    _roleService.GetRole(ajaxEditPermissionViewModel.MembershipRole),
                                Permission =
                                    _permissionService.Get(ajaxEditPermissionViewModel.Permission),
                                IsTicked = ajaxEditPermissionViewModel.HasPermission
                            };
                            _globalPermissionForRoleService.UpdateOrCreateNew(gpr);
                        }
                        else
                        {
                            // We have a category so it's a category permission 

                            var mappedItem = new CategoryPermissionForRole
                            {
                                Category = _categoryService.Get(ajaxEditPermissionViewModel.Category),
                                MembershipRole =
                                    _roleService.GetRole(ajaxEditPermissionViewModel.MembershipRole),
                                Permission =
                                    _permissionService.Get(ajaxEditPermissionViewModel.Permission),
                                IsTicked = ajaxEditPermissionViewModel.HasPermission
                            };
                            _categoryPermissionForRoleService.UpdateOrCreateNew(mappedItem);   
                        }
                    }
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    throw;
                }
            }
        }

        public ActionResult DeletePermission(Guid id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var permission = _permissionService.Get(id);
                    _permissionService.Delete(permission);

                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                                                {
                                                                    Message = "Permission Deleted",
                                                                    MessageType = GenericMessages.success
                                                                };
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    throw;
                }
            }
            return RedirectToAction("Index");
        }

    }
}
