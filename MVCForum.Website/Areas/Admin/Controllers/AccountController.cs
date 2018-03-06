namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.Enums;
    using Web.ViewModels;
    using Web.ViewModels.Admin;
    using Web.ViewModels.Mapping;

    public class AccountController : BaseAdminController
    {
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly IPollService _pollService;
        private readonly IPostService _postService;
        private readonly IRoleService _roleService;
        private readonly ITopicService _topicService;
        private readonly IUploadedFileService _uploadedFileService;
        public readonly IActivityService ActivityService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="roleService"> </param>
        /// <param name="settingsService"> </param>
        /// <param name="loggingService"> </param>
        /// <param name="postService"> </param>
        /// <param name="topicService"> </param>
        /// <param name="membershipUserPointsService"> </param>
        /// <param name="activityService"> </param>
        /// <param name="pollService"> </param>
        /// <param name="uploadedFileService"></param>
        public AccountController(ILoggingService loggingService, IMvcForumContext context,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ISettingsService settingsService, IPostService postService, ITopicService topicService,
            IMembershipUserPointsService membershipUserPointsService,
            IActivityService activityService, IPollService pollService, 
            IUploadedFileService uploadedFileService)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
            ActivityService = activityService;
            _roleService = roleService;
            _postService = postService;
            _topicService = topicService;
            _membershipUserPointsService = membershipUserPointsService;
            _pollService = pollService;
            _uploadedFileService = uploadedFileService;
        }

        #region Users

        /// <summary>
        ///     Take a set of role names and update a user's collection of roles accordingly
        /// </summary>
        /// <param name="user"></param>
        /// <param name="updatedRoles"></param>
        private void UpdateUserRoles(MembershipUser user, IEnumerable<string> updatedRoles)
        {
            // ---------------------------------------------------------------------
            // IMPORTANT - If you call this it MUST be within a unit of work
            // ---------------------------------------------------------------------

            // Not done in automapper to avoid handling services in the mapper
            var updatedRolesSet = new List<MembershipRole>();
            foreach (var roleStr in updatedRoles)
            {
                var alreadyIsRoleForUser = false;
                foreach (var role in user.Roles)
                {
                    if (roleStr == role.RoleName)
                    {
                        // This role for this user is UNchanged
                        updatedRolesSet.Add(role);
                        alreadyIsRoleForUser = true;
                        break;
                    }
                }

                if (!alreadyIsRoleForUser)
                {
                    // This is a new role for this user
                    updatedRolesSet.Add(_roleService.GetRole(roleStr));
                }
            }

            // Replace the roles in the user's collection
            user.Roles.Clear();
            foreach (var role in updatedRolesSet)
            {
                user.Roles.Add(role);
            }
        }

        /// <summary>
        ///     List out users and allow editing
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        private async Task<ActionResult> ListUsers(int? p, string search)
        {
            var pageIndex = p ?? 1;
            var allUsers = string.IsNullOrWhiteSpace(search)
                ? await MembershipService.GetAll(pageIndex, ForumConfiguration.Instance.AdminListPageSize)
                : await MembershipService.SearchMembers(search, pageIndex, ForumConfiguration.Instance.AdminListPageSize);

            // Redisplay list of users
            var allViewModelUsers = allUsers.Select(ViewModelMapping.UserToSingleMemberListViewModel).ToList();
            var memberListModel = new MemberListViewModel
            {
                Users = allViewModelUsers,
                AllRoles = _roleService.AllRoles(),
                Id = MembershipService.GetUser(User.Identity.Name).Id,
                PageIndex = pageIndex,
                TotalCount = allUsers.TotalCount,
                Search = search,
                TotalPages = allUsers.TotalPages
            };

            return View("List", memberListModel);
        }

        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult ManageUserPoints(Guid id)
        {
            var user = MembershipService.GetUser(id);
            var viewModel = new ManageUsersPointsViewModel
            {
                AllPoints = _membershipUserPointsService.GetByUser(user).OrderByDescending(x => x.DateAdded)
                    .ToList(),
                User = user
            };
            return View(viewModel);
        }

        [Authorize(Roles = Constants.AdminRoleName)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageUserPoints(ManageUsersPointsViewModel viewModel)
        {
            // Repopulate viewmodel
            var user = MembershipService.GetUser(viewModel.Id);
            viewModel.AllPoints = _membershipUserPointsService.GetByUser(user).OrderByDescending(x => x.DateAdded)
                .ToList();
            viewModel.User = user;

            if (viewModel.Amount > 0)
            {
                // Add the new points
                var newPoints = new MembershipUserPoints
                {
                    DateAdded = DateTime.UtcNow,
                    Notes = viewModel.Note,
                    Points = (int)viewModel.Amount,
                    PointsFor = PointsFor.Manual,
                    User = user
                };

                await _membershipUserPointsService.Add(newPoints);

                try
                {
                    Context.SaveChanges();

                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = "Points Added",
                        MessageType = GenericMessages.success
                    });
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = "There was an error adding the points",
                        MessageType = GenericMessages.danger
                    });
                }
            }

            return RedirectToAction("ManageUserPoints", new { id = user.Id });
        }

        [Authorize(Roles = Constants.AdminRoleName)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemovePoints(Guid pointToRemove)
        {
            var point = _membershipUserPointsService.Get(pointToRemove);
            var user = point.User;
            _membershipUserPointsService.Delete(point);

            try
            {
                Context.SaveChanges();

                ShowMessage(new GenericMessageViewModel
                {
                    Message = "Points Removed",
                    MessageType = GenericMessages.success
                });
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                ShowMessage(new GenericMessageViewModel
                {
                    Message = "There was an error",
                    MessageType = GenericMessages.danger
                });
            }

            return RedirectToAction("ManageUserPoints", new { id = user.Id });
        }

        /// <summary>
        ///     Manage users
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        public async Task<ActionResult> Manage(int? p, string search)
        {
            return await ListUsers(p, search);
        }


        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult Edit(Guid id)
        {
            var user = MembershipService.GetUser(id);

            var viewModel = ViewModelMapping.UserToMemberEditViewModel(user);
            viewModel.AllRoles = _roleService.AllRoles();

            return View(viewModel);
        }


        [HttpPost]
        [Authorize(Roles = Constants.AdminRoleName)]
        public async Task<ActionResult> Edit(MemberEditViewModel userModel)
        {
            var user = MembershipService.GetUser(userModel.Id);

            // Map everything in model except properties hidden on page
            user.Age = userModel.Age;
            user.Comment = userModel.Comment;
            user.Email = userModel.Email;
            user.Facebook = userModel.Facebook;
            user.IsApproved = userModel.IsApproved;
            user.IsLockedOut = userModel.IsLockedOut;
            user.IsBanned = userModel.IsBanned;
            user.Location = userModel.Location;
            user.PasswordAnswer = userModel.PasswordAnswer;
            user.PasswordQuestion = userModel.PasswordQuestion;
            user.Signature = userModel.Signature;
            user.Twitter = userModel.Twitter;
            user.UserName = userModel.UserName;
            user.Website = userModel.Website;
            user.DisableEmailNotifications = userModel.DisableEmailNotifications;
            user.DisablePosting = userModel.DisablePosting;
            user.DisablePrivateMessages = userModel.DisablePrivateMessages;
            user.IsTrustedUser = userModel.IsTrustedUser;

            try
            {
                Context.SaveChanges();

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "User saved",
                    MessageType = GenericMessages.success
                };
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                ModelState.AddModelError(string.Empty,
                    LocalizationService.GetResourceString("Errors.GenericMessage"));
            }

            return await ListUsers(null, null);
        }


        [Authorize(Roles = Constants.AdminRoleName)]
        public async Task<ActionResult> Delete(Guid id, int? p, string search)
        {

            var user = MembershipService.GetUser(id);
            if (user == null)
            {
                throw new ApplicationException("Cannot delete user - user does not exist");
            }

            var piplineReslt = await MembershipService.Delete(user);
            if (piplineReslt.Successful)
            {
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "User delete successfully",
                    MessageType = GenericMessages.success
                };
            }
            else
            {
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = piplineReslt.ProcessLog.FirstOrDefault(),
                    MessageType = GenericMessages.danger
                };
            }

            return RedirectToAction("Manage", new { p, search });
        }

        [HttpPost]
        [Authorize(Roles = Constants.AdminRoleName)]
        public void UpdateUserRoles(AjaxRoleUpdateViewModel ajaxRoleUpdateViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                var user = MembershipService.GetUser(ajaxRoleUpdateViewModel.Id);

                UpdateUserRoles(user, ajaxRoleUpdateViewModel.Roles);

                try
                {
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    throw new Exception("Error updating user roles");
                }
            }
        }

        #endregion

        #region Roles

        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult ListAllRoles()
        {
            var roles = new RoleListViewModel
            {
                MembershipRoles = _roleService.AllRoles()
            };
            return View(roles);
        }

        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult EditRole(Guid Id)
        {
            var role = _roleService.GetRole(Id);

            var viewModel = ViewModelMapping.RoleToRoleViewModel(role);

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult EditRole(RoleViewModel role)
        {
            var existingRole = _roleService.GetRole(role.Id);
            existingRole.RoleName = role.RoleName;

            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                throw new Exception("Error editing role");
            }


            // Use temp data as its a redirect
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Role saved",
                MessageType = GenericMessages.success
            };

            return RedirectToAction("ListAllRoles");
        }

        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult DeleteRole(Guid Id)
        {
            var roleToDelete = _roleService.GetRole(Id);
            _roleService.Delete(roleToDelete);

            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                throw new Exception("Error voting up post");
            }


            // Use temp data as its a redirect
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Role Deleted",
                MessageType = GenericMessages.success
            };
            return RedirectToAction("ListAllRoles");
        }

        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult AddRole()
        {
            var role = new RoleViewModel();

            return View(role);
        }

        [HttpPost]
        [Authorize(Roles = Constants.AdminRoleName)]
        public ActionResult AddRole(RoleViewModel role)
        {
            var newRole = ViewModelMapping.RoleViewModelToRole(role);
            _roleService.CreateRole(newRole);

            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                throw new Exception("Error adding a role");
            }


            // Use temp data as its a redirect
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Role Added",
                MessageType = GenericMessages.success
            };
            return RedirectToAction("ListAllRoles");
        }

        #endregion
    }
}