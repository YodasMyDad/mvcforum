using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IMembershipUserPointsService
    {
        /// <summary>
        /// Delete a specific point
        /// </summary>
        /// <param name="points"></param>
        void Delete(MembershipUserPoints points);
        /// <summary>
        /// Delete a certain amount of points - Just finds closest one to the amount and then deletes
        /// </summary>
        /// <param name="amount"></param>
        /// <param name="user"></param>
        void Delete(int amount, MembershipUser user);
        /// <summary>
        /// Delete point for a specific type and for the associated object if (Post.Id, Vote.Id etc...)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="type"></param>
        /// <param name="referenceId"></param>
        void Delete(MembershipUser user, PointsFor type, Guid referenceId);

        /// <summary>
        /// Delete all points by action/type and the reference ID
        /// </summary>
        /// <param name="type"></param>
        /// <param name="referenceId"></param>
        void Delete(PointsFor type, Guid referenceId);

        /// <summary>
        /// Delete all points for a type - So delete all points a user has gained for voting, or for badges
        /// </summary>
        /// <param name="user"></param>
        /// <param name="type"></param>        
        void Delete(MembershipUser user, PointsFor type);
        IEnumerable<MembershipUserPoints> GetByUser(MembershipUser user, bool removeTracking = true);
        MembershipUserPoints Add(MembershipUserPoints points);
        Dictionary<MembershipUser, int> GetCurrentWeeksPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetThisYearsPoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePoints(int? amountToTake);
        Dictionary<MembershipUser, int> GetAllTimePointsNegative(int? amountToTake);
        /// <summary>
        /// Sync members points
        /// </summary>
        /// <param name="user">The user who's points need syncing</param>
        /// <returns>Whether or not the database needs updating</returns>
        bool SyncUserPoints(MembershipUser user);
        /// <summary>
        /// Find out a users 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="type">What points you are looking for, posts, votes, badges etc...</param>
        /// <returns></returns>
        int PointsByType(MembershipUser user, PointsFor type);

        MembershipUserPoints Get(Guid id);
        int UserPoints(MembershipUser user);
        void Delete(IEnumerable<MembershipUserPoints> points);
    }
}
