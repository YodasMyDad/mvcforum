using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels.Mapping;
using MembershipUser = MVCForum.Domain.DomainModel.MembershipUser;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    public partial class AccountController : BaseAdminController
    {
        public readonly IActivityService _activityService;    
        private readonly IRoleService _roleService;
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly IPollService _pollService;
        private readonly IPollVoteService _pollVoteService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly IUploadedFileService _uploadedFileService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWorkManager"> </param>
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
        /// <param name="pollVoteService"> </param>
        /// <param name="pollAnswerService"> </param>
        /// <param name="uploadedFileService"></param>
        public AccountController(ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ISettingsService settingsService, IPostService postService, ITopicService topicService, IMembershipUserPointsService membershipUserPointsService, 
            IActivityService activityService, IPollService pollService, IPollVoteService pollVoteService, IPollAnswerService pollAnswerService, IUploadedFileService uploadedFileService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _activityService = activityService;
            _roleService = roleService;
            _postService = postService;
            _topicService = topicService;
            _membershipUserPointsService = membershipUserPointsService;
            _pollService = pollService;
            _pollVoteService = pollVoteService;
            _pollAnswerService = pollAnswerService;
            _uploadedFileService = uploadedFileService;
        }

        #region Users

        /// <summary>
        /// Take a set of role names and update a user's collection of roles accordingly
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
            foreach(var role in updatedRolesSet)
            {
                user.Roles.Add(role);
            }

        }

        /// <summary>
        /// List out users and allow editing
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = AppConstants.AdminRoleName)]
        private ActionResult ListUsers(int? p, string search)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var pageIndex = p ?? 1;
                var allUsers = string.IsNullOrEmpty(search) ? MembershipService.GetAll(pageIndex, SiteConstants.Instance.AdminListPageSize) :
                                    MembershipService.SearchMembers(search, pageIndex, SiteConstants.Instance.AdminListPageSize);

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
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult ManageUserPoints(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(id);
                var viewModel = new ManageUsersPointsViewModel
                {
                    AllPoints = _membershipUserPointsService.GetByUser(user).OrderByDescending(x => x.DateAdded).ToList(),
                    User = user
                };
                return View(viewModel);
            }
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageUserPoints(ManageUsersPointsViewModel viewModel)
        {
            using (var uow = UnitOfWorkManager.NewUnitOfWork())
            {
                // Repopulate viewmodel
                var user = MembershipService.GetUser(viewModel.Id);
                viewModel.AllPoints = _membershipUserPointsService.GetByUser(user).OrderByDescending(x => x.DateAdded).ToList();
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

                    _membershipUserPointsService.Add(newPoints);

                    try
                    {
                        uow.Commit();

                        ShowMessage(new GenericMessageViewModel
                        {
                            Message = "Points Added",
                            MessageType = GenericMessages.success
                        });
                    }
                    catch (Exception ex)
                    {
                        uow.Rollback();
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
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemovePoints(Guid pointToRemove)
        {
            using (var uow = UnitOfWorkManager.NewUnitOfWork())
            {
                var point = _membershipUserPointsService.Get(pointToRemove);
                var user = point.User;        
                _membershipUserPointsService.Delete(point);

                try
                {
                    uow.Commit();

                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = "Points Removed",
                        MessageType = GenericMessages.success
                    });
                }
                catch (Exception ex)
                {
                    uow.Rollback();
                    LoggingService.Error(ex);
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = "There was an error",
                        MessageType = GenericMessages.danger
                    });
                }

                return RedirectToAction("ManageUserPoints", new {id = user.Id });

            }            
        }

        /// <summary>
        /// Manage users
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult Manage(int? p, string search)
        {
            return ListUsers(p, search);
        }


        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult Edit(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(id);

                var viewModel = ViewModelMapping.UserToMemberEditViewModel(user);
                viewModel.AllRoles = _roleService.AllRoles();

                return View(viewModel);
            }
        }


        [HttpPost]
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult Edit(MemberEditViewModel userModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
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

                try
                {
                    unitOfWork.Commit();

                    ViewBag.Message = new GenericMessageViewModel
                    {
                        Message = "User saved",
                        MessageType = GenericMessages.success
                    };
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                }

                return ListUsers(null, null);
            }
        }


        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult Delete(Guid Id, int? p, string search)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var user = MembershipService.GetUser(Id);
                    if (user == null)
                    {
                        throw new ApplicationException("Cannot delete user - user does not exist");
                    }

                    MembershipService.Delete(user, unitOfWork);

                    ViewBag.Message = new GenericMessageViewModel
                    {
                        Message = "User delete successfully",
                        MessageType = GenericMessages.success
                    };
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ViewBag.Message = new GenericMessageViewModel
                    {
                        Message = string.Format("Delete failed: {0}", ex.Message),
                        MessageType = GenericMessages.danger
                    };
                }
                return RedirectToAction("Manage", new {p, search});
            }
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public void UpdateUserRoles(AjaxRoleUpdateViewModel ajaxRoleUpdateViewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (Request.IsAjaxRequest())
                {
                    var user = MembershipService.GetUser(ajaxRoleUpdateViewModel.Id);

                    UpdateUserRoles(user, ajaxRoleUpdateViewModel.Roles);

                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception("Error updating user roles");
                    }
                }
            }
        }

        #endregion

        #region Roles
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult ListAllRoles()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var roles = new RoleListViewModel
                        {
                            MembershipRoles = _roleService.AllRoles()
                        };
                return View(roles);
            }
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult EditRole(Guid Id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var role = _roleService.GetRole(Id);

                var viewModel = ViewModelMapping.RoleToRoleViewModel(role);

                return View(viewModel);
            }
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult EditRole(RoleViewModel role)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var existingRole = _roleService.GetRole(role.Id);
                existingRole.RoleName = role.RoleName;
               
                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    throw new Exception("Error editing role");
                }
            }

            // Use temp data as its a redirect
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Role saved",
                MessageType = GenericMessages.success
            };

            return RedirectToAction("ListAllRoles");
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult DeleteRole(Guid Id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var roleToDelete = _roleService.GetRole(Id);
                _roleService.Delete(roleToDelete);

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    throw new Exception("Error voting up post");
                }
            }

            // Use temp data as its a redirect
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Role Deleted",
                MessageType = GenericMessages.success
            };
            return RedirectToAction("ListAllRoles");
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult AddRole()
        {
            var role = new RoleViewModel();

            return View(role);
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult DeleteUsersPosts(Guid id, bool profileClick = false)
        {
            var user = MembershipService.GetUser(id);

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
                {
                    MembershipService.ScrubUsers(user, unitOfWork);

                    try
                    {
                        unitOfWork.Commit();
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "All posts and topics deleted",
                            MessageType = GenericMessages.success
                        };
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = "Error trying to delete posts and topics",
                            MessageType = GenericMessages.danger
                        };
                    }
                }
            }

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                if (profileClick)
                {
                    return Redirect(user.NiceUrl);
                }
                var viewModel = ViewModelMapping.UserToMemberEditViewModel(user);
                viewModel.AllRoles = _roleService.AllRoles();
                return View("Edit", viewModel);
            }

        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult AddRole(RoleViewModel role)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {

                var newRole = ViewModelMapping.RoleViewModelToRole(role);
                _roleService.CreateRole(newRole);

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    throw new Exception("Error adding a role");
                }
            }

            // Use temp data as its a redirect
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Role Added",
                MessageType = GenericMessages.success
            };
            return RedirectToAction("ListAllRoles");
        }

        #endregion

    }
}
