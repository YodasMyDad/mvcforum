namespace MVCForum.Services
{
    using Domain.Constants;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Domain.DomainModel;
    using Domain.DomainModel.Activity;
    using Domain.DomainModel.Attributes;
    using Domain.Events;
    using Domain.Interfaces;
    using Domain.Interfaces.Badges;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Utilities;

    public partial class BadgeService : IBadgeService
    {
        private readonly ILocalizationService _localizationService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ILoggingService _loggingService;
        private readonly IReflectionService _reflectionService;
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        public const int BadgeCheckIntervalMinutes = 10;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="localizationService"> </param>
        /// <param name="membershipUserPointsService"></param>
        /// <param name="reflectionService"></param>
        /// <param name="context"></param>
        /// <param name="cacheService"></param>
        public BadgeService(ILoggingService loggingService, ILocalizationService localizationService,
            IMembershipUserPointsService membershipUserPointsService, IReflectionService reflectionService, IMVCForumContext context, ICacheService cacheService)
        {
            _loggingService = loggingService;
            _localizationService = localizationService;
            _membershipUserPointsService = membershipUserPointsService;
            _reflectionService = reflectionService;
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        #region Private static methods

        /// <summary>
        /// The discovered badge class .Net types, indexed by MVCForum badge type
        /// </summary>
        private static Dictionary<BadgeType, List<BadgeMapping>> _badges;

        /// <summary>
        /// Utility class to correlate badged classes with badge database records
        /// </summary>
        private class BadgeMapping
        {
            /// <summary>
            /// The domain object representation of the badge
            /// </summary>
            public Badge DbBadge { get; set; }

            /// <summary>
            /// The class type that implements the badge rule
            /// </summary>
            public Type BadgeClass { get; set; }

            /// <summary>
            /// An instance of the badge class, lazy loaded
            /// </summary>
            public IBadge BadgeClassInstance { get; set; }
        }

        /// <summary>
        /// Create a database badge from a badge class
        /// </summary>
        /// <param name="badgeType"></param>
        /// <param name="classType"></param>
        /// <returns></returns>
        private static Badge CreateDbBadgeFromClass(BadgeType badgeType, Type classType)
        {
            var idAtt = GetAttribute<IdAttribute>(classType);
            var nameAtt = GetAttribute<NameAttribute>(classType);
            var descAtt = GetAttribute<DescriptionAttribute>(classType);
            var imageAtt = GetAttribute<ImageAttribute>(classType);
            var displayNameAtt = GetAttribute<DisplayNameAttribute>(classType);
            var awardsPointsAtt = GetAttribute<AwardsPointsAttribute>(classType);

            var badge = new Badge
            {
                Id = idAtt.Id,
                Name = nameAtt.Name,
                Description = descAtt.Description,
                Image = imageAtt.Image,
                DisplayName = displayNameAtt.DisplayName,
                Users = new List<MembershipUser>(),
                Type = badgeType.ToString().TrimEnd(),
                AwardsPoints = awardsPointsAtt?.Points ?? 0
            };
            return badge;
        }


        /// <summary>
        /// Get the specified attribute off a badge class
        /// </summary>
        /// <param name="type"></param>
        /// <returns>The attribute class instance</returns>
        /// <exception cref="BadgeAttributeNotFoundException">Class does not have the attribute</exception>
        private static T GetAttribute<T>(Type type) where T : class
        {
            foreach (var attribute in type.GetCustomAttributes(false))
            {
                if (attribute is T)
                {
                    return attribute as T;
                }
            }

            throw new BadgeAttributeNotFoundException();
        }

        /// <summary>
        /// Get an instance from a badge class (instantiate it)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="badgeMapping"></param>
        /// <returns></returns>
        private static IBadge GetInstance<T>(BadgeMapping badgeMapping)
        {
            // If we've previously instantiated this class then return the instance
            if (badgeMapping.BadgeClassInstance != null)
            {
                return badgeMapping.BadgeClassInstance;
            }

            var ctor = badgeMapping.BadgeClass.GetConstructors().First();
            var createdActivator = ReflectionUtilities.GetActivator<T>(ctor);

            // Create an instance:
            badgeMapping.BadgeClassInstance = createdActivator() as IBadge;

            return badgeMapping.BadgeClassInstance;
        }

        /// <summary>
        /// Check if a user badge can be awarded
        /// </summary>
        /// <param name="user"></param>
        /// <param name="badgeMapping"></param>
        /// <returns></returns>
        private bool BadgeCanBeAwarded(MembershipUser user, BadgeMapping badgeMapping)
        {
            var cacheKey = string.Concat(CacheKeys.Badge.StartsWith, "BadgeCanBeAwarded-", user.Id, "-", badgeMapping.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                if (user.Badges == null)
                {
                    _loggingService.Error(string.Format(_localizationService.GetResourceString("Badges.UnableToAward"), user.UserName));
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
            });
        }

        #endregion

        /// <summary>
        /// Check to see if the badge type has been recently processed for this user
        /// </summary>
        /// <param name="badgeType"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        private bool RecentlyProcessed(BadgeType badgeType, MembershipUser user)
        {
            var cacheKey = string.Concat(CacheKeys.Badge.StartsWith, "RecentlyProcessed-", user.Id, "-", badgeType);
            return _cacheService.CachePerRequest(cacheKey, () =>
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
            });
        }

        /// <summary>
        /// Convert a string to an enum
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
                _loggingService.Error(string.Format(_localizationService.GetResourceString("Badge.UnknownBadge"), badgeTypeStr));
            }

            return null;
        }

        /// <summary>
        /// Iterates over the runtime folder looking for classes that implement the badge interface
        /// </summary>
        private void GetBadgesByReflection(IEnumerable<Assembly> assemblies)
        {
            _badges = new Dictionary<BadgeType, List<BadgeMapping>>();

            var interfaceFilter = new TypeFilter(_reflectionService.InterfaceFilter);

            // Get all the dlls
            foreach (var nextAssembly in assemblies)
            {
                try
                {

                    foreach (var type in nextAssembly.GetTypes())
                    {
                        if (type.IsInterface)
                        {
                            continue;
                        }

                        // See if this type is one of the badge interfaces
                        foreach (BadgeType badgeType in Enum.GetValues(typeof(BadgeType)))
                        {
                            // Look for the target badge class type
                            if (!Badge.BadgeClassNames.ContainsKey(badgeType))
                            {
                                throw new ApplicationException(
                                    string.Format(_localizationService.GetResourceString("Badge.BadegEnumNoClass"), badgeType));
                            }

                            var interfaceType = Badge.BadgeClassNames[badgeType];

                            var myInterfaces = type.FindInterfaces(interfaceFilter, interfaceType);
                            if (myInterfaces.Length <= 0)
                            {
                                // Not a match
                                continue;
                            }

                            // This class implements the interface

                            // Create a domain model version
                            var dbBadge = CreateDbBadgeFromClass(badgeType, type);

                            if (!_badges.ContainsKey(badgeType))
                            {
                                _badges.Add(badgeType, new List<BadgeMapping>());
                            }
                            _badges[badgeType].Add(new BadgeMapping { BadgeClass = type, DbBadge = dbBadge });
                        }
                    }
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    var msg =
                        $"Unable to load assembly. Probably not an event assembly, loader exception was: '{rtle.LoaderExceptions[0].GetType()}':'{rtle.LoaderExceptions[0].Message}'.";
                    _loggingService.Error(msg);
                }
                catch (Exception ex)
                {
                    _loggingService.Error(ex);
                }
            }
        }


        /// <summary>
        /// Bring the database into line with the badge classes found at runtime
        /// </summary>
        /// <returns>Set of valid badge classes to use when assigning badges</returns>
        public void SyncBadges(List<Assembly> assemblies)
        {
            try
            {
                GetBadgesByReflection(assemblies);

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
        /// Processes the user for the specified badge type
        /// </summary>
        /// <param name="badgeType"></param>
        /// <param name="user"></param>
        /// <returns>True if badge was awarded</returns>
        public bool ProcessBadge(BadgeType badgeType, MembershipUser user)
        {
            var databaseUpdateNeeded = false;

            var e = new BadgeEventArgs { User = user, BadgeType = badgeType };
            EventManager.Instance.FireBeforeBadgeAwarded(this, e);

            if (!e.Cancel)
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
                            var badge = GetInstance<IBadge>(badgeMapping);

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
                                        _membershipUserPointsService.Add(points);
                                    }
                                    user.Badges.Add(dbBadge);
                                    //_activityService.BadgeAwarded(badgeMapping.DbBadge, user, DateTime.UtcNow);
                                    var badgeActivity = BadgeActivity.GenerateMappedRecord(badgeMapping.DbBadge, user, DateTime.UtcNow);
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
            return databaseUpdateNeeded;
        }

        /// <summary>
        /// Gets a paged list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<Badge> GetPagedGroupedBadges(int pageIndex, int pageSize)
        {

            var cacheKey = string.Concat(CacheKeys.Badge.StartsWith, "GetPagedGroupedBadges-", pageIndex, "-", pageSize);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var totalCount = _context.Badge.Count();
                // Get the topics using an efficient
                var results = _context.Badge
                                    .OrderByDescending(x => x.Name)
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();


                // Return a paged list
                return new PagedList<Badge>(results, pageIndex, pageSize, totalCount);
            });
        }

        public PagedList<Badge> SearchPagedGroupedTags(string search, int pageIndex, int pageSize)
        {
            var cacheKey = string.Concat(CacheKeys.Badge.StartsWith, "SearchPagedGroupedTags-", search, "-", pageIndex, "-", pageSize);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                search = StringUtils.SafePlainText(search);
                var totalCount = _context.Badge.Count(x => x.Name.ToUpper().Contains(search.ToUpper()));
                // Get the topics using an efficient
                var results = _context.Badge
                                    .Where(x => x.Name.ToUpper().Contains(search.ToUpper()))
                                    .OrderByDescending(x => x.Name)
                                    .Skip((pageIndex - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();


                // Return a paged list
                return new PagedList<Badge>(results, pageIndex, pageSize, totalCount);
            });
        }

        public IList<Badge> GetallBadges()
        {
            var cacheKey = string.Concat(CacheKeys.Badge.StartsWith, "GetallBadges");
            return _cacheService.CachePerRequest(cacheKey, () => GetAll().ToList());
        }

        public void DeleteTimeLastChecked(BadgeTypeTimeLastChecked badgeTypeTimeLastChecked)
        {
            _context.BadgeTypeTimeLastChecked.Remove(badgeTypeTimeLastChecked);
        }

        public Badge GetBadge(string name)
        {
            var cacheKey = string.Concat(CacheKeys.Badge.StartsWith, "GetBadge-", name);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Badge.FirstOrDefault(x => x.Name == name));
        }

        public Badge Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.Badge.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Badge.FirstOrDefault(badge => badge.Id == id));
        }

        public IEnumerable<Badge> GetAll()
        {
            var cacheKey = string.Concat(CacheKeys.Badge.StartsWith, "GetAll");
            return _cacheService.CachePerRequest(cacheKey, () => _context.Badge.ToList());
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
    }

    public class BadgeAttributeNotFoundException : Exception
    {
    }
}

