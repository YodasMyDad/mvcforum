using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IBadgeRepository
    {
        /// <summary>
        /// Get a badge by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Badge GetBadge(string name);

        /// <summary>
        /// All badges
        /// </summary>
        /// <returns></returns>
        IEnumerable<Badge> GetAll();

        Badge Add(Badge newBadge);

        PagedList<Badge> GetPagedGroupedBadges(int pageIndex, int pageSize);
        PagedList<Badge> SearchPagedGroupedBadges(string search, int pageIndex, int pageSize);

        Badge Get(Guid id);

        /// <summary>
        /// Generic single entity delete
        /// </summary>
        /// <param name="badge"></param>
        void Delete(Badge badge);
        void Update(Badge item);

        void DeleteTimeLastChecked(BadgeTypeTimeLastChecked badgeTypeTimeLastChecked);
    }
}

