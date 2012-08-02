using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly ILocalizationService _localizationService;

        public const int BadgeCheckIntervalMinutes = 30;

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

        private readonly IBadgeRepository _badgeRepository;
        private readonly IMVCForumAPI _mvcForumAPI;
        private readonly ILoggingService _loggingService;
        private readonly IActivityService _activityService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="badgeRepository"> </param>
        /// <param name="api"></param>
        /// <param name="loggingService"> </param>
        /// <param name="localizationService"> </param>
        /// <param name="activityService"> </param>
        public BadgeService(IBadgeRepository badgeRepository, IMVCForumAPI api, 
            ILoggingService loggingService, ILocalizationService localizationService, IActivityService activityService)
        {
            _badgeRepository = badgeRepository;
            _mvcForumAPI = api;
            _loggingService = loggingService;
            _localizationService = localizationService;
            _activityService = activityService;
        }

        #region Private static methods

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
            
            return new Badge
            {
                Id = idAtt.Id,
                Name = nameAtt.Name,
                Description = descAtt.Description,
                Image = imageAtt.Image,
                DisplayName = displayNameAtt.DisplayName,
                Users = new List<MembershipUser>(),
                Type = badgeType.ToString().TrimEnd(),
            };
        }

        /// <summary>
        /// Callback used when comparing objects to see if they implement an interface
        /// </summary>
        /// <param name="typeObj"></param>
        /// <param name="criteriaObj"></param>
        /// <returns></returns>
        private static bool InterfaceFilter(Type typeObj, Object criteriaObj)
        {
            return typeObj.ToString() == criteriaObj.ToString();
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
            var recentlyProcessed = false;
            var now = DateTime.Now;

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
        private void GetBadgesByReflection()
        {
            _badges = new Dictionary<BadgeType, List<BadgeMapping>>();

            var interfaceFilter = new TypeFilter(InterfaceFilter);

            var path = AppDomain.CurrentDomain.RelativeSearchPath;

            // Get all the dlls
            var di = new DirectoryInfo(path);
            foreach (var file in di.GetFiles("*.dll"))
            {
                try
                {
                    if (file.Name == "EcmaScript.NET.dll")
                    {
                        continue;
                    }

                    Assembly nextAssembly;
                    try
                    {
                        nextAssembly = Assembly.LoadFrom(file.FullName);
                    }
                    catch (BadImageFormatException)
                    {
                        // Not an assembly ignore
                        continue;
                    }

                    if (nextAssembly.FullName.StartsWith("System") || nextAssembly.FullName.StartsWith("Microsoft"))
                    {
                        // Skip microsoft assemblies
                        continue;
                    }

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
                                    string.Format(_localizationService.GetResourceString("Badge.BadegEnumNoClass"), badgeType.ToString()));
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
                            _badges[badgeType].Add(new BadgeMapping {BadgeClass = type, DbBadge = dbBadge});
                        }
                    }
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    var msg =
                        string.Format(
                            "Unable to load assembly. Probably not a badge assembly. In file named '{0}', loader exception was: '{1}':'{2}'.",
                            file.Name, rtle.LoaderExceptions[0].GetType(), rtle.LoaderExceptions[0].Message);
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
        public void SyncBadges()
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

                var dbBadges = _badgeRepository.GetAll().ToList();

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

                            // Remove it from insert collection, it's not new
                            badgesToInsert.Remove(classBadge);
                        }
                    }
                }

                foreach (var badge in badgesToDelete)
                {
                    _badgeRepository.Delete(badge);
                }

                foreach (var badge in badgesToInsert)
                {
                    _badgeRepository.Add(badge);
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
            var badgeAwarded = false;

            var e = new BadgeEventArgs {User = user, BadgeType = badgeType, Api = _mvcForumAPI};
            EventManager.Instance.FireBeforeBadgeAwarded(this, e);

            if (!e.Cancel)
            {
                if (_badges.ContainsKey(badgeType))
                {
                    if (!RecentlyProcessed(badgeType, user))
                    {
                        var badgeSet = _badges[badgeType];

                        foreach (var badgeMapping in badgeSet)
                        {
                            if (!BadgeCanBeAwarded(user, badgeMapping))
                            {
                                continue;
                            }

                            // Instantiate the badge and execute the rule                
                            var badge = GetInstance<IBadge>(badgeMapping);

                            // Award badge?
                            if (badge != null && badge.Rule(user, _mvcForumAPI))
                            {
                                // Re-fetch the badge otherwise system will try and create new badges!
                                var dbBadge = _badgeRepository.Get(badgeMapping.DbBadge.Id);
                                user.Badges.Add(dbBadge);
                                badgeAwarded = true;
                                _activityService.BadgeAwarded(badgeMapping.DbBadge, user, DateTime.Now);

                                EventManager.Instance.FireAfterBadgeAwarded(this,
                                                                            new BadgeEventArgs
                                                                                {
                                                                                    User = user,
                                                                                    BadgeType = badgeType,
                                                                                    Api = _mvcForumAPI
                                                                                });
                            }
                        }
                    }
                }
            }
            return badgeAwarded;
        }

        /// <summary>
        /// Gets a paged list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<Badge> GetPagedGroupedBadges(int pageIndex, int pageSize)
        {
            return _badgeRepository.GetPagedGroupedBadges(pageIndex, pageSize);
        }

        public PagedList<Badge> SearchPagedGroupedTags(string search, int pageIndex, int pageSize)
        {
            return _badgeRepository.SearchPagedGroupedBadges(StringUtils.SafePlainText(search), pageIndex, pageSize);
        }

        public void DeleteTimeLastChecked(BadgeTypeTimeLastChecked badgeTypeTimeLastChecked)
        {
            _badgeRepository.DeleteTimeLastChecked(badgeTypeTimeLastChecked);
        }

        public void Delete(Badge badge)
        {
            _badgeRepository.Delete(badge);
        }
    }

    public class BadgeAttributeNotFoundException : Exception
    {
    }
}

