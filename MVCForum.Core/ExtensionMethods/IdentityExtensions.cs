namespace MvcForum.Core.ExtensionMethods
{
    using System.Linq;
    using DomainModel.Entities;
    using System.Security.Principal;
    using Constants;
    using Interfaces.Services;

    public static class IdentityExtensions
    {
        /// <summary>
        /// Gets the membership user from the IPrincpal
        /// </summary>
        /// <param name="user"></param>
        /// <param name="membershipService"></param>
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        public static MembershipUser GetMembershipUser(this IPrincipal user, IMembershipService membershipService, bool removeTracking = true)
        {
            return user.Identity.IsAuthenticated ? membershipService.GetUser(user.Identity.Name, removeTracking) : null;
        }

        /// <summary>
        /// Gets the users roles
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleService"></param>
        /// <returns></returns>
        public static MembershipRole GetRole(this MembershipUser user, IRoleService roleService)
        {
            return user == null ? roleService.GetRole(AppConstants.GuestRoleName, true) : user.Roles.FirstOrDefault();
        }
    }
}