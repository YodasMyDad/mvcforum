namespace MvcForum.Web.Application.Providers
{
    using System;
    using System.Collections.Specialized;
    using System.Web.Hosting;
    using System.Web.Security;
    using Core.Interfaces.Services;
    using Core.Ioc;
    using Unity;

    public class MvcForumMembershipProvider : MembershipProvider
    {
        // MSDN how to implement a custom provider: http://msdn.microsoft.com/en-us/library/6tc47t75.aspx

        // ALL THE METHODS MUST BE USED WITHIN A UNIT OF WORK IN THE CONTROLLERS

        // TODO - Need to implement this properly

        private string _applicationName;
        private bool _enablePasswordReset;
        private int _maxInvalidPasswordAttempts;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private int _passwordAttemptWindow;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;

        // Use Dependency Resolver
        //public IUnitOfWorkManager UnitOfWorkManager
        //{
        //    get { return UnityHelper.Container.Resolve<IUnitOfWorkManager>(); }
        //}
        public IMembershipService MembershipService => UnityHelper.Container.Resolve<IMembershipService>();

        public override int MinRequiredPasswordLength => _minRequiredPasswordLength;

        public override int MaxInvalidPasswordAttempts => _maxInvalidPasswordAttempts;


        public override string ApplicationName
        {
            get { return _applicationName; }
            set { _applicationName = value; }
        }

        public override bool EnablePasswordReset => _enablePasswordReset;

        public override bool EnablePasswordRetrieval { get; } = false;

        public override int MinRequiredNonAlphanumericCharacters => _minRequiredNonAlphanumericCharacters;

        public override int PasswordAttemptWindow => _passwordAttemptWindow;

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Read a config file value
        /// </summary>
        /// <param name="configValue"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string GetConfigValue(string configValue, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(configValue) ? defaultValue : configValue;
        }

        /// <summary>
        ///     Initialise membership properties from config file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "n3oSqlMembershipProvider";
            }

            if (string.IsNullOrWhiteSpace(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "MvcForum Standard Membership Provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            _applicationName = GetConfigValue(config["applicationName"], HostingEnvironment.ApplicationVirtualPath);
            _maxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            _passwordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            _minRequiredNonAlphanumericCharacters =
                Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            _minRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            _requiresQuestionAndAnswer =
                Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            _requiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            _enablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
        }

        //var user = MembershipService.GetUser(username);
        //if (user != null)
        //{
        //    return 
        //}
        //return false;

        /// <summary>
        ///     Validate the user (required for membership in MVC)
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public override bool ValidateUser(string username, string password)
        {
            // use the data in the graph object to authorise the user
            return MembershipService.ValidateUser(username, password, MaxInvalidPasswordAttempts);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var user = MembershipService.GetUser(username);
            if (user != null)
            {
                return MembershipService.ChangePassword(user, oldPassword, newPassword);
            }
            return false;
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password,
            string newPasswordQuestion, string newPasswordAnswer)
        {
            if (ValidateUser(username, password))
            {
                var user = MembershipService.GetUser(username);
                user.PasswordQuestion = newPasswordQuestion;
                user.PasswordAnswer = newPasswordAnswer;
                return true;
            }
            return false;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize,
            out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize,
            out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email,
            string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey,
            out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }
    }
}