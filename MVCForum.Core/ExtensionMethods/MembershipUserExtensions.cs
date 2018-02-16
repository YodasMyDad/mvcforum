namespace MvcForum.Core.ExtensionMethods
{
    using System.Linq;
    using Models.Entities;

    public static class MembershipUserExtensions
    {
        /// <summary>
        /// Is this user an Admin
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsAdmin(this MembershipUser user)
        {
            return user.Roles.Any(x => x.RoleName.Contains(Constants.Constants.AdminRoleName));
        }
    }
}