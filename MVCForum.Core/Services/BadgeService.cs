namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Constants;
    using Events;
    using Interfaces;
    using Interfaces.Badges;
    using Interfaces.Services;
    using Ioc;
    using Models.Activity;
    using Models.Attributes;
    using Models.Entities;
    using Models.Enums;
    using Models.General;
    using Reflection;
    using Unity;
    using Utilities;

    public partial class BadgeService : IBadgeService
    {
        public const int BadgeCheckIntervalMinutes = 10;
        private readonly ICacheService _cacheService;
        private IMvcForumContext _context;
        private readonly ILocalizationService _localizationService;
        private readonly ILoggingService _loggingService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="localizationService"> </param>
        /// <param name="membershipUserPointsService"></param>
        /// <param name="context"></param>
        /// <param name="cacheService"></param>
        public BadgeService(ILoggingService loggingService, ILocalizationService localizationService,
            IMembershipUserPointsService membershipUserPointsService,
            IMvcForumContext context, ICacheService cacheService)
        {
            _loggingService = loggingService;
            _localizationService = localizationService;
            _membershipUserPointsService = membershipUserPointsService;
            _cacheService = cacheService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
            _localizationService.RefreshContext(context);
            _membershipUserPointsService.RefreshContext(context);
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        ///     Bring the database into line with the badge classes found at runtime
        /// </summary>
        /// <returns>Set of valid badge classes to use when assigning badges</returns>
        public void SyncBadges(IEnumerable<Assembly> assemblies)
        {
            try
            {
                GetBadgesByReflection();

                // Turn the badge classes into a set of domain objects
                var badgesFromClasses = new Dictionary<Guid, Badge>();
                foreach (var badgeType in _badges.Keys)
                {
                    foreach (var badgeClassMapping in _badges[badgeType])
                    {
                        if (!badgesFromClasses.ContainsKey(badgeClassMapping.DbBadge.Id))
                        {
                            badgesFromClasses.Add(badgeClassMapping.DbBadge.Id, badgeClassMapping.DbBadge);
                        }
                    }
                }

                var badgesToDelete = new List<Badge>();
                var badgesToInsert = new List<Badge>();

                var dbBadges = GetAll().ToList();

                // Find badges to delete - ie in database not in classes     
                badgesToDelete.AddRange(dbBadges);
                foreach (var dbBadge in dbBadges)
                {
                    if (badgesFromClasses.Values.Any(classBadge => classBadge.Id == dbBadge.Id))
                    {
                        badgesToDelete.Remove(dbBadge);
                    }
                }

                // Find badges to insert or update
                badgesToInsert.AddRange(badgesFromClasses.Values);
                foreach (var classBadge in badgesFromClasses.Values)
                {
                    foreach (var dbBadge in dbBadges)
                    {
                        if (dbBadge.Id == classBadge.Id)
                        {
                            // This class is found in the database so it's not new, but might be an update
                            if (dbBadge.Name != classBadge.Name)
                            {
                                dbBadge.Name = classBadge.Name;
                            }

                            if (dbBadge.Description != classBadge.Description)
                            {
                                dbBadge.Description = classBadge.Description;
                            }

                            if (dbBadge.DisplayName != classBadge.DisplayName)
                            {
                                dbBadge.DisplayName = classBadge.DisplayName;
                            }

                            if (dbBadge.Image != classBadge.Image)
                            {
                                dbBadge.Image = classBadge.Image;
                            }

                            if (dbBadge.AwardsPoints != classBadge.AwardsPoints)
                            {
                                dbBadge.AwardsPoints = classBadge.AwardsPoints;
                            }

                            // Remove it from insert collection, it's not new
                            badgesToInsert.Remove(classBadge);
                        }
                    }
                }

                foreach (var badge in badgesToDelete)
                {
                    //TODO - Remove points associated with a deleted badge?
                    Delete(badge);
                }

                foreach (var badge in badgesToInsert)
                {
                    Add(badge);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
        }

        /// <summary>
        ///     Processes the user for the specified badge type
        /// </summary>
        /// <param name="badgeType"></param>
        /// <param name="user"></param>
        /// <returns>True if badge was awarded</returns>
        public async Task<bool> ProcessBadge(BadgeType badgeType, MembershipUser user)
        {
            var databaseUpdateNeeded = false;

            var e = new BadgeEventArgs { User = user, BadgeType = badgeType };
            EventManager.Instance.FireBeforeBadgeAwarded(this, e);

            if (!e.Cancel)
            {
                try
                {
                    if (_badges.ContainsKey(badgeType))
                    {
                        if (!RecentlyProcessed(badgeType, user))
                        {
                            databaseUpdateNeeded = true;

                            var badgeSet = _badges[badgeType];

                            foreach (var badgeMapping in badgeSet)
                            {
                                if (!BadgeCanBeAwarded(user, badgeMapping))
                                {
                                    continue;
                                }

                                // Instantiate the badge and execute the rule
                                IBadge badge;
                                if (badgeMapping.BadgeClassInstance != null)
                                {
                                    badge = badgeMapping.BadgeClassInstance;
                                }
                                else
                                {
                                    badgeMapping.BadgeClassInstance = UnityHelper.Container.Resolve(badgeMapping.BadgeClass) as IBadge;
                                    badge = badgeMapping.BadgeClassInstance;
                                }

                                if (badge != null)
                                {
                                    var dbBadge = Get(badgeMapping.DbBadge.Id);

                                    // Award badge?
                                    if (badge.Rule(user))
                                    {
                                        // Re-fetch the badge otherwise system will try and create new badges!                                
                                        if (dbBadge.AwardsPoints != null && dbBadge.AwardsPoints > 0)
                                        {
                                            var points = new MembershipUserPoints
                                            {
                                                Points = (int)dbBadge.AwardsPoints,
                                                PointsFor = PointsFor.Badge,
                                                PointsForId = dbBadge.Id,
                                                User = user
                                            };
                                            var pointsAddResult = await _membershipUserPointsService.Add(points);
                                            if (!pointsAddResult.Successful)
                                            {
                                                _loggingService.Error(pointsAddResult.ProcessLog.FirstOrDefault());
                                                return false;
                                            }

                                        }
                                        user.Badges.Add(dbBadge);
                                        //_activityService.BadgeAwarded(badgeMapping.DbBadge, user, DateTime.UtcNow);
                                        var badgeActivity =
                                            BadgeActivity.GenerateMappedRecord(badgeMapping.DbBadge, user, DateTime.UtcNow);
                                        _context.Activity.Add(badgeActivity);
                                        EventManager.Instance.FireAfterBadgeAwarded(this,
                                            new BadgeEventArgs
                                            {
                                                User = user,
                                                BadgeType = badgeType
                                            });
                                    }
                                    //else
                                    //{
                                    //    // If we get here the user should not have the badge
                                    //    // Remove the badge if the user no longer has the criteria to be awarded it
                                    //    // and also remove any points associated with it.
                                    //    user.Badges.Remove(dbBadge);
                                    //    _membershipUserPointsService.Delete(user, PointsFor.Badge, dbBadge.Id);
                                    //}
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.Error(ex);
                }
            }
            return databaseUpdateNeeded;
        }

        /// <summary>
        ///     Gets a paged list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PaginatedList<Badge>> GetPagedGroupedBadges(int pageIndex, int pageSize)
        {
            var query = _context.Badge.OrderByDescending(x => x.Name);
            return await PaginatedList<Badge>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        public async Task<PaginatedList<Badge>> SearchPagedGroupedTags(string search, int pageIndex, int pageSize)
        {
            search = StringUtils.SafePlainText(search);

            // Return a paged list
            var query = _context.Badge
                .Where(x => x.Name.ToUpper().Contains(search.ToUpper()))
                .OrderByDescending(x => x.Name);
            return await PaginatedList<Badge>.CreateAsync(query.AsNoTracking(), pageIndex, pageSize);
        }

        public void DeleteTimeLastChecked(BadgeTypeTimeLastChecked badgeTypeTimeLastChecked)
        {
            _context.BadgeTypeTimeLastChecked.Remove(badgeTypeTimeLastChecked);
        }

        public Badge GetBadge(string name)
        {
            return _context.Badge.FirstOrDefault(x => x.Name == name);
        }

        public Badge Get(Guid id)
        {
            return _context.Badge.FirstOrDefault(badge => badge.Id == id);
        }

        public IEnumerable<Badge> GetAll()
        {
            return _context.Badge.ToList();
        }

        public Badge Add(Badge newBadge)
        {
            return _context.Badge.Add(newBadge);
        }

        public void Delete(Badge badge)
        {
            badge.Users.Clear();
            _context.Badge.Remove(badge);
        }

        /// <summary>
        ///     Check to see if the badge type has been recently processed for this user
        /// </summary>
        /// <param name="badgeType"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool RecentlyProcessed(BadgeType badgeType, MembershipUser user)
        {
            var recentlyProcessed = false;
            var now = DateTime.UtcNow;

            BadgeTypeTimeLastChecked timeBadgeLastChecked = null;

            // Go through all the badge-check time records for this user
            foreach (var nextBadgeTypeCheckedForUser in user.BadgeTypesTimeLastChecked)
            {
                var previouslyCheckedBadgeType = FromString(nextBadgeTypeCheckedForUser.BadgeType);

                if (previouslyCheckedBadgeType == null || previouslyCheckedBadgeType != badgeType)
                {
                    continue;
                }

                // Block the badge check if not enough time has elapsed since last check
                if ((now - nextBadgeTypeCheckedForUser.TimeLastChecked).TotalMinutes < BadgeCheckIntervalMinutes)
                {
                    recentlyProcessed = true;
                }

                timeBadgeLastChecked = nextBadgeTypeCheckedForUser;
                timeBadgeLastChecked.TimeLastChecked = now;

                break;
            }

            // If this badge type never checked for this user, add it
            if (timeBadgeLastChecked == null)
            {
                timeBadgeLastChecked = new BadgeTypeTimeLastChecked
                {
                    BadgeType = badgeType.ToString(),
                    TimeLastChecked = now,
                    User = user
                };

                user.BadgeTypesTimeLastChecked.Add(timeBadgeLastChecked);
            }

            return recentlyProcessed;

        }

        /// <summary>
        ///     Convert a string to an enum
        /// </summary>
        /// <param name="badgeTypeStr"></param>
        /// <returns></returns>
        private BadgeType? FromString(string badgeTypeStr)
        {
            try
            {
                return (BadgeType)Enum.Parse(typeof(BadgeType), badgeTypeStr);
            }
            catch (ArgumentException)
            {
                _loggingService.Error(string.Format(_localizationService.GetResourceString("Badge.UnknownBadge"),
                    badgeTypeStr));
            }

            return null;
        }

        private void AddBadgeRelection(BadgeType badeType, Type classType)
        {
            var dbBadge = CreateDbBadgeFromClass(badeType, classType);

            if (!_badges.ContainsKey(badeType))
            {
                _badges.Add(badeType, new List<BadgeMapping>());
            }
            _badges[badeType].Add(new BadgeMapping { BadgeClass = classType, DbBadge = dbBadge });
        }

        /// <summary>
        ///     Iterates over the runtime folder looking for classes that implement the badge interface
        /// </summary>
        private void GetBadgesByReflection()
        {
            try
            {
                _badges = new Dictionary<BadgeType, List<BadgeMapping>>();

                // All the allowed badges
                var allowedBadges = ForumConfiguration.Instance.Badges;

                // Get all the badges
                var badges = ImplementationManager.GetInstances<IBadge>();

                foreach (var allowedBadge in allowedBadges)
                {
                    if (badges.ContainsKey(allowedBadge))
                    {
                        var badgeClass = badges[allowedBadge];
                        var classType = badgeClass.GetType();

                        if (badgeClass is IVoteUpBadge)
                        {
                            // Create a domain model version
                            AddBadgeRelection(BadgeType.VoteUp, classType);
                        }
                        if (badgeClass is IMarkAsSolutionBadge)
                        {
                            // Create a domain model version
                            AddBadgeRelection(BadgeType.MarkAsSolution, classType);
                        }
                        if (badgeClass is IPostBadge)
                        {
                            // Create a domain model version
                            AddBadgeRelection(BadgeType.Post, classType);
                        }
                        if (badgeClass is IVoteDownBadge)
                        {
                            // Create a domain model version
                            AddBadgeRelection(BadgeType.VoteDown, classType);
                        }
                        if (badgeClass is IProfileBadge)
                        {
                            // Create a domain model version
                            AddBadgeRelection(BadgeType.Profile, classType);
                        }
                        if (badgeClass is IFavouriteBadge)
                        {
                            // Create a domain model version
                            AddBadgeRelection(BadgeType.Favourite, classType);
                        }
                        if (badgeClass is ITagBadge)
                        {
                            // Create a domain model version
                            AddBadgeRelection(BadgeType.Tag, classType);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException rtle)
            {
                var msg = $"Unable to load assembly. Probably not a a badge assembly, loader exception was: '{rtle.LoaderExceptions[0].GetType()}':'{rtle.LoaderExceptions[0].Message}'.";
                _loggingService.Error(msg);
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
        }

        #region Private static methods

        /// <summary>
        ///     The discovered badge class .Net types, indexed by MvcForum badge type
        /// </summary>
        private static Dictionary<BadgeType, List<BadgeMapping>> _badges;

        /// <summary>
        ///     Utility class to correlate badged classes with badge database records
        /// </summary>
        private class BadgeMapping
        {
            /// <summary>
            ///     The domain object representation of the badge
            /// </summary>
            public Badge DbBadge { get; set; }

            /// <summary>
            ///     The class type that implements the badge rule
            /// </summary>
            public Type BadgeClass { get; set; }

            /// <summary>
            ///     An instance of the badge class, lazy loaded
            /// </summary>
            public IBadge BadgeClassInstance { get; set; }
        }

        /// <summary>
        ///     Create a database badge from a badge class
        /// </summary>
        /// <param name="badgeType"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        private static Badge CreateDbBadgeFromClass(BadgeType badgeType, Type classType)
        {
            var badge = new Badge
            {
                Users = new List<MembershipUser>(),
                Type = badgeType.ToString().TrimEnd()
            };

            foreach (var attribute in classType.GetCustomAttributes(false))
            {
                if (attribute is IdAttribute)
                {
                    badge.Id = (attribute as IdAttribute).Id;
                }
                if (attribute is NameAttribute)
                {
                    badge.Name = (attribute as NameAttribute).Name;
                }
                if (attribute is DescriptionAttribute)
                {
                    badge.Description = (attribute as DescriptionAttribute).Description;
                }
                if (attribute is ImageAttribute)
                {
                    badge.Image = (attribute as ImageAttribute).Image;
                }
                if (attribute is DisplayNameAttribute)
                {
                    badge.DisplayName = (attribute as DisplayNameAttribute).DisplayName;
                }
                if (attribute is AwardsPointsAttribute)
                {
                    badge.AwardsPoints = (attribute as AwardsPointsAttribute).Points;
                }
            }

            return badge;
        }

        /// <summary>
        ///     Check if a user badge can be awarded
        /// </summary>
        /// <param name="user"></param>
        /// <param name="badgeMapping"></param>
        /// <returns></returns>
        private bool BadgeCanBeAwarded(MembershipUser user, BadgeMapping badgeMapping)
        {
            if (user.Badges == null)
            {
                _loggingService.Error(string.Format(_localizationService.GetResourceString("Badges.UnableToAward"),
                    user.UserName));
                return false;
            }

            var badgeCanBeAwarded = true;

            if (badgeMapping.BadgeClass == null || badgeMapping.DbBadge == null)
            {
                badgeCanBeAwarded = false;
            }
            else
            {
                var userHasBadge = user.Badges.Any(userBadge => userBadge.Name == badgeMapping.DbBadge.Name);

                if (userHasBadge)
                {
                    badgeCanBeAwarded = false;
                }
            }

            return badgeCanBeAwarded;
        }

        #endregion
    }

    public class BadgeAttributeNotFoundException : Exception
    {
    }
}