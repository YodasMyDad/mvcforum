using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace MVCForum.SSOLogin.Providers
{
    public class SSOTestMembershipUser : MembershipUser
    {
        public SSOTestMembershipUser(Models.User user) : base(null, user.UserName, user.UserName, user.Email, null, user.Comments, true, false, DateTime.MinValue, DateTime.MinValue, DateTime.Now, DateTime.MinValue, DateTime.MinValue)
        {
        }

        public static implicit operator SSOTestMembershipUser(Models.User user)
        {
            if (user == null)
                return null;
            return new SSOTestMembershipUser(user);
        }
    }
}