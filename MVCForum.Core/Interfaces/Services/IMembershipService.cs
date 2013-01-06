using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public enum LoginAttemptStatus
    {
        LoginSuccessful,
        UserNotFound,
        PasswordIncorrect,
        PasswordAttemptsExceeded,
        UserLockedOut,
        UserNotApproved,
    }

    public interface IMembershipService
    {
        MembershipUser SanitizeUser(MembershipUser membershipUser);
        bool ValidateUser(string userName, string password, int maxInvalidPasswordAttempts);
        LoginAttemptStatus LastLoginStatus { get; }
        string[] GetRolesForUser(string username);
        MembershipUser GetUser(string username);
        MembershipUser GetUserByEmail(string email);
        MembershipUser GetUserBySlug(string slug);
        MembershipUser GetUserByFacebookId(long facebookId);
        IList<MembershipUser> GetUsersById(List<Guid> guids);
        MembershipUser GetUser(Guid id);
        bool ChangePassword(MembershipUser user, string oldPassword, string newPassword);
        bool ResetPassword(MembershipUser user, string newPassword);
        void UnlockUser(string username, bool resetPasswordAttempts);
        MembershipCreateStatus CreateUser(MembershipUser newUser);
        string ErrorCodeToString(MembershipCreateStatus createStatus);
        MembershipUser CreateEmptyUser();
        IList<MembershipUser> GetAll();
        PagedList<MembershipUser> GetAll(int pageIndex, int pageSize);
        PagedList<MembershipUser> SearchMembers(string search, int pageIndex, int pageSize);
        IList<MembershipUser> SearchMembers(string username, int amount);
        void Save(MembershipUser user);
        void ProfileUpdated(MembershipUser user);
        void Delete(MembershipUser user);
        IList<MembershipUser> GetLatestUsers(int amountToTake);
        IList<MembershipUser> GetLowestPointUsers(int amountToTake);
        int MemberCount();
        string ToCsv();
        CsvReport FromCsv(List<string> allLines);
    }
}
