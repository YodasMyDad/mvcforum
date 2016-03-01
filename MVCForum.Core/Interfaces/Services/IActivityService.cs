using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IActivityService
    {
        /// <summary>
        /// Gets a paged list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<ActivityBase> GetPagedGroupedActivities(int pageIndex, int pageSize);

        /// <summary>
        /// Gets all activities by search data field for a Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        IEnumerable<Activity> GetDataFieldByGuid(Guid guid);

        /// <summary>
        /// Get a paged list of activities by search string
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<ActivityBase> SearchPagedGroupedActivities(string search, int pageIndex, int pageSize);

        IEnumerable<ActivityBase> GetAll(int howMany);

        /// <summary>
        /// New badge has been awarded
        /// </summary>
        /// <param name="badge"></param>
        /// <param name="user"> </param>
        /// <param name="timestamp"> </param>
        void BadgeAwarded(Badge badge, MembershipUser user, DateTime timestamp);

        void MemberJoined(MembershipUser user);

        /// <summary>
        /// Profile has been updated
        /// </summary>
        /// <param name="user"></param>
        void ProfileUpdated(MembershipUser user);

        /// <summary>
        /// Delete a number of activities
        /// </summary>
        /// <param name="activities"></param>
        void Delete(IList<Activity> activities);

        Activity Add(Activity newActivity);
        Activity Get(Guid id);
        void Delete(Activity item);
    }
}
