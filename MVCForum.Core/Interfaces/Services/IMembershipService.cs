namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using System.Web;
    using Models.Entities;
    using Models.Enums;
    using Models.General;
    using Pipeline;

    public partial interface IMembershipService : IContextService
    {
        LoginAttemptStatus LastLoginStatus { get; }
        MembershipUser Add(MembershipUser newUser);
        MembershipUser SanitizeUser(MembershipUser membershipUser);
        bool ValidateUser(string userName, string password, int maxInvalidPasswordAttempts);
        string[] GetRolesForUser(string username);
        MembershipUser Get(Guid id);
        MembershipUser GetUser(string username, bool removeTracking = false);
        MembershipUser GetUserByEmail(string email, bool removeTracking = false);
        MembershipUser GetUserBySlug(string slug);
        IList<MembershipUser> GetUserBySlugLike(string slug);
        IList<MembershipUser> GetUsersById(List<Guid> guids);
        IList<MembershipUser> GetUsersByDaysPostsPoints(int amoutOfDaysSinceRegistered, int amoutOfPosts);
        MembershipUser GetUser(Guid id);
        bool ChangePassword(MembershipUser user, string oldPassword, string newPassword);
        bool ResetPassword(MembershipUser user, string newPassword);
        void UnlockUser(string username, bool resetPasswordAttempts);
        MembershipUser CreateEmptyUser();
        Task<IPipelineProcess<MembershipUser>> CreateUser(MembershipUser newUser, LoginType loginType);
        Task<IPipelineProcess<MembershipUser>> EditUser(MembershipUser userToEdit, IPrincipal loggedInUser, HttpPostedFileBase image);
        string ErrorCodeToString(MembershipCreateStatus createStatus);
        IList<MembershipUser> GetAll();
        Task<PaginatedList<MembershipUser>> GetAll(int pageIndex, int pageSize);
        Task<PaginatedList<MembershipUser>> SearchMembers(string search, int pageIndex, int pageSize);
        IList<MembershipUser> SearchMembers(string username, int amount);
        IList<MembershipUser> GetActiveMembers();
        void ProfileUpdated(MembershipUser user);
        Task<IPipelineProcess<MembershipUser>> Delete(MembershipUser user);
        IList<MembershipUser> GetLatestUsers(int amountToTake);
        IList<MembershipUser> GetLowestPointUsers(int amountToTake);
        int MemberCount();
        string ToCsv();
        CsvReport FromCsv(List<string> allLines);
        /// <summary>
        ///     Completed scrubs a users account clean
        ///     Clears everything - Posts, polls, votes, favourites, profile etc...
        /// </summary>
        /// <param name="user"></param>
        Task<IPipelineProcess<MembershipUser>> ScrubUsers(MembershipUser user);
        bool UpdatePasswordResetToken(MembershipUser user);
        bool ClearPasswordResetToken(MembershipUser user);
        bool IsPasswordResetTokenValid(MembershipUser user, string token);
    }
}