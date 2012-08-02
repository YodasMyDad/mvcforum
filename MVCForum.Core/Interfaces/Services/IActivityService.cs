using System;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IActivityService
    {
        /// <summary>
        /// Gets a paged list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<ActivityBase> GetPagedGroupedActivities(int pageIndex, int pageSize);

        PagedList<ActivityBase> SearchPagedGroupedActivities(string search, int pageIndex, int pageSize);

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
    }
}
