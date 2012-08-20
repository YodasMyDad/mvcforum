using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IBadgeService
    {
        /// <summary>
        /// Synchronises badge classes with database badge records
        /// </summary>
        void SyncBadges();


        /// <summary>
        /// Processes the user for the specified badge type
        /// </summary>
        /// <param name="badgeType"></param>
        /// <param name="user"></param>
        /// <returns>True if badge was awarded</returns>
        bool ProcessBadge(BadgeType badgeType, MembershipUser user);

        PagedList<Badge> GetPagedGroupedBadges(int pageIndex, int pageSize);
        PagedList<Badge> SearchPagedGroupedTags(string search, int pageIndex, int pageSize);
        IList<Badge> GetallBadges();

        void Delete(Badge badge);
        void DeleteTimeLastChecked(BadgeTypeTimeLastChecked badgeTypeTimeLastChecked);
    }
}
