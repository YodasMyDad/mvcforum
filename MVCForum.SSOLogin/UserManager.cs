using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Services;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;

namespace MVCForum.SSOLogin
{
    public class UserManager
    {
        private static string userJsonPath;
        private static AuthenticationSection authenticationSection;
        private static AuthenticationMode authenticationMode { get; set; }
        private static List<Models.User> AllUsers { get; set; }
        static UserManager()
        {
            //Get authentication section and authentication mode
            authenticationSection = (AuthenticationSection)ConfigurationManager.GetSection("system.web/authentication");
            if (authenticationSection == null)
                throw new ConfigurationErrorsException("missing system.web/authentication section");
            authenticationMode = authenticationSection.Mode;
            if (authenticationMode != AuthenticationMode.None && authenticationMode != AuthenticationMode.Forms)
                throw new InvalidOperationException("The SSOLogin project does not currently support an Authentication Mode other than None or Forms.");

            //Load simple users info from users.Json file in App_Data folder "this was the simplest way to back some users for the SSO Test I could think of, didn't want to implement EF or PetaPoco and a database for a test."
            userJsonPath = HttpContext.Current.Server.MapPath("~/App_Data/users.Json");
            var stream = new FileInfo(userJsonPath).OpenText();
            var usersJson = stream.ReadToEnd();
            stream.Dispose();
            AllUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.User>>(usersJson);
        }

        /// <summary>
        /// Validates user credentials against a user in the users.Json file.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool ValidateUser(string userName, string password)
        {
            var user = AllUsers.Where(p => p.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (user != null && user.Password.Equals(password, StringComparison.InvariantCultureIgnoreCase))
                return true;
            return false;
        }

        /// <summary>
        /// Get's a user from the users.Json file
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static Models.User GetUser(string userName)
        {
            var user = AllUsers.Where(p => p.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            return user;
        }

        /// <summary>
        /// Get's all the users from the users.Json file
        /// </summary>
        /// <returns></returns>
        public static List<Models.User> GetAllUsers()
        {
            return AllUsers.ToList();
        }

        /// <summary>
        /// Get's a user by email address from the users.Json file
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static Models.User GetUserByEmail(string email)
        {
            var user = AllUsers.Where(p => p.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            return user;
        }

        public static List<Models.User> FindUsersByUserName(string userName)
        {
            var users = AllUsers.Where(p => p.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase) || p.UserName.StartsWith(userName.ToLower()) || p.UserName.ToLower().EndsWith(userName.ToLower()) || p.UserName.ToLower().Contains(userName.ToLower()));
            return users.ToList();
        }

        public static List<Models.User> FindUsersByEmail(string email)
        {
            var users = AllUsers.Where(p => p.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase) || p.Email.StartsWith(email.ToLower()) || p.Email.ToLower().EndsWith(email.ToLower()) || p.Email.ToLower().Contains(email.ToLower()));
            return users.ToList();
        }


        /// <summary>
        /// Validates a user and creates a Claims Principal for them and set's it as the authenticated principal
        /// </summary>
        /// <param name="userName">The user being logged in</param>
        /// <param name="password">The password of the user being logged in</param>
        /// <param name="isPersistent">True if the session is persisted through the user Agent.</param>
        /// <returns>True if logged in, false if unable to validate credentials for user.</returns>
        public static bool Login(string userName, string password, bool isPersistent)        
        { 
            if (ValidateUser(userName, password))
            {
                try
                {                    
                    //GenericIdentity identity = new GenericIdentity(userName);                    
                    //ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    //var sessionToken = FederatedAuthentication.SessionAuthenticationModule.CreateSessionSecurityToken(principal, HttpContext.Current.Request.Url.ToString(), DateTime.Now, DateTime.Now.AddDays(1), isPersistent);
                    //if (sessionToken != null)
                    //{
                    //    FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionToken);
                    //    FederatedPassiveSecurityTokenServiceOperations.ProcessRequest(HttpContext.Current.Request, sessionToken.ClaimsPrincipal, WIF.MVCForumSecurityTokenServiceConfiguration.Current.CreateSecurityTokenService(), HttpContext.Current.Response);
                    //}
                    if (Membership.ValidateUser(userName, password))
                    {
                        FormsAuthentication.SetAuthCookie(userName, isPersistent);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return false;
        }
    }
}