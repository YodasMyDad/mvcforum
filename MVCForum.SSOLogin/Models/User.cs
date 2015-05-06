using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCForum.SSOLogin.Models
{
    public class User
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Comments { get; set; }

        public static implicit operator Providers.SSOTestMembershipUser(User user)
        {
            if (user == null)
                return null;
            return new Providers.SSOTestMembershipUser(user);
        }
    }
}