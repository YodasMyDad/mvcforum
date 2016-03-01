using System;
using System.Collections.Generic;
using System.Reflection;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IBadgeService
    {

        /// <summary>
        /// Bring the database into line with the badge classes found at runtime
        /// </summary>
        /// <returns>Set of valid badge classes to use when assigning badges</returns>
        void SyncBadges(List<Assembly> assemblies);

        /// <summary>
        /// Processes the user for the specified badge type
        /// </summary>
        /// <param name="badgeType"></param>
        /// <param name="user"></param>
        /// <returns>True if badge was awarded</returns>
        bool ProcessBadge(BadgeType badgeType, MembershipUser user);

        /// <summary>
        /// Gets a paged list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<Badge> GetPagedGroupedBadges(int pageIndex, int pageSize);

        /// <summary>
        /// Search for pages in a paged list
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<Badge> SearchPagedGroupedTags(string search, int pageIndex, int pageSize);

        /// <summary>
        /// Get all Badges enabled in the applications
        /// </summary>
        /// <returns></returns>
        IList<Badge> GetallBadges();

        /// <summary>
        /// Delete a badge
        /// </summary>
        /// <param name="badge"></param>
        void Delete(Badge badge);

        /// <summary>
        /// Deletes the last checked time
        /// </summary>
        /// <param name="badgeTypeTimeLastChecked"></param>
        void DeleteTimeLastChecked(BadgeTypeTimeLastChecked badgeTypeTimeLastChecked);

        /// <summary>
        /// Get a badge by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Badge GetBadge(string name);

        Badge Get(Guid id);

        /// <summary>
        /// All badges
        /// </summary>
        /// <returns></returns>
        IEnumerable<Badge> GetAll();

        Badge Add(Badge newBadge);
    }
}
