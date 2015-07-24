using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace MVCForum.SSOLogin.Providers
{
    public class SSOTestMembershipProvider : MembershipProvider
    {
        private string applicationName = "SSOTestMembershipProvider";

        #region Properties
        public override string ApplicationName
        {
            get
            {
                return applicationName;
            }
            set
            {
                applicationName = value;
            }
        }
        public override bool EnablePasswordReset
        {
            get { return false; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }
        public override int MaxInvalidPasswordAttempts
        {
            get { return -1; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 0; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 0; }
        }

        public override int PasswordAttemptWindow
        {
            get { return -1; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Clear; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return null; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return true; }
        }
        #endregion

        #region Not Implemented
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }
        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }
        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }
        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }
        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }
        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }
        #endregion
        

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var users = UserManager.FindUsersByEmail(emailToMatch);
            MembershipUserCollection muc = new MembershipUserCollection();
            users.ForEach(user =>
            {
                muc.Add(user);
            });
            totalRecords = muc.Count;
            return muc;
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            var users = UserManager.FindUsersByUserName(usernameToMatch);
            MembershipUserCollection muc = new MembershipUserCollection();
            users.ForEach(user =>
            {
                muc.Add(user);
            });
            totalRecords = muc.Count;
            return muc;            
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var users = UserManager.GetAllUsers();
            MembershipUserCollection muc = new MembershipUserCollection();
            users.ForEach(user =>
            {
                muc.Add(user);
            });
            totalRecords = muc.Count;
            return muc;
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var user = UserManager.GetUser(username);
            return user;
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            var user = UserManager.GetUser(providerUserKey as string);
            return user;
        }

        public override string GetUserNameByEmail(string email)
        {
            var user = UserManager.GetUserByEmail(email);
            if (user != null)
                return user.UserName;
            return null;
        }
        public override bool ValidateUser(string username, string password)
        {
            return UserManager.ValidateUser(username, password);
        }
    }
}