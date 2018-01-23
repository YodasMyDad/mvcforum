namespace MvcForum.Core.ExtensionMethods
{
    using System.Linq;
    using System.Security.Principal;
    using Constants;
    using Interfaces.Services;
    using Models.Entities;

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
        /// <param name="removeTracking"></param>
        /// <returns></returns>
        public static MembershipRole GetRole(this MembershipUser user, IRoleService roleService, bool removeTracking = true)
        {
            return user == null ? roleService.GetRole(Constants.GuestRoleName, removeTracking) : user.Roles.FirstOrDefault();
        }
    }
}