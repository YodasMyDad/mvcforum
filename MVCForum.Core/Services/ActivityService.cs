namespace MVCForum.Services
{
    using Domain.Constants;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.DomainModel;
    using Domain.DomainModel.Activity;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Utilities;

    public partial class ActivityService : IActivityService
    {
        private readonly MVCForumContext _context;
        private readonly IBadgeService _badgeService;
        private readonly ILoggingService _loggingService;
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Constructor
        /// </summary>
        public ActivityService(IBadgeService badgeService, ILoggingService loggingService, IMVCForumContext context, ICacheService cacheService)
        {
            _badgeService = badgeService;
            _loggingService = loggingService;
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        #region Private Methods
        /// <summary>
        /// Make a badge activity object from the more generic database activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private BadgeActivity GenerateBadgeActivity(Activity activity)
        {
            //System.Reflection.MethodBase.GetCurrentMethod().Name
            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "GenerateBadgeActivity-", activity.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                // Get the corresponding badge
                var dataPairs = ActivityBase.UnpackData(activity);

                if (!dataPairs.ContainsKey(BadgeActivity.KeyBadgeId))
                {
                    // Log the problem then skip
                    _loggingService.Error($"A badge activity record with id '{activity.Id}' has no badge id in its data.");
                    return null;
                }

                var badgeId = dataPairs[BadgeActivity.KeyBadgeId];
                var badge = _badgeService.Get(new Guid(badgeId));

                if (badge == null)
                {
                    // Log the problem then skip
                    _loggingService.Error($"A badge activity record with id '{activity.Id}' has a badge id '{badgeId}' that is not found in the badge table.");
                    return null;
                }

                var userId = dataPairs[BadgeActivity.KeyUserId];
                var user = _context.MembershipUser.FirstOrDefault(x => x.Id == new Guid(userId));

                if (user == null)
                {
                    // Log the problem then skip
                    _loggingService.Error($"A badge activity record with id '{activity.Id}' has a user id '{userId}' that is not found in the user table.");
                    return null;
                }

                return new BadgeActivity(activity, badge, user);
            });
        }

        /// <summary>
        /// Make a profile updated joined activity object from the more generic database activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private ProfileUpdatedActivity GenerateProfileUpdatedActivity(Activity activity)
        {

            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "GenerateProfileUpdatedActivity-", activity.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var dataPairs = ActivityBase.UnpackData(activity);

                if (!dataPairs.ContainsKey(ProfileUpdatedActivity.KeyUserId))
                {
                    // Log the problem then skip
                    _loggingService.Error($"A profile updated activity record with id '{activity.Id}' has no user id in its data.");
                    return null;
                }

                var userId = dataPairs[ProfileUpdatedActivity.KeyUserId];
                var user = _context.MembershipUser.FirstOrDefault(x => x.Id == new Guid(userId));

                if (user == null)
                {
                    // Log the problem then skip
                    _loggingService.Error($"A profile updated activity record with id '{activity.Id}' has a user id '{userId}' that is not found in the user table.");
                    return null;
                }

                return new ProfileUpdatedActivity(activity, user);
            });

        }

        /// <summary>
        /// Make a member joined activity object from the more generic database activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private MemberJoinedActivity GenerateMemberJoinedActivity(Activity activity)
        {
            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "GenerateMemberJoinedActivity-", activity.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var dataPairs = ActivityBase.UnpackData(activity);

                if (!dataPairs.ContainsKey(MemberJoinedActivity.KeyUserId))
                {
                    // Log the problem then skip
                    _loggingService.Error($"A member joined activity record with id '{activity.Id}' has no user id in its data.");
                    return null;
                }

                var userId = dataPairs[MemberJoinedActivity.KeyUserId];
                var user = _context.MembershipUser.FirstOrDefault(x => x.Id == new Guid(userId));

                if (user == null)
                {
                    // Log the problem then skip
                    _loggingService.Error($"A member joined activity record with id '{activity.Id}' has a user id '{userId}' that is not found in the user table.");
                    return null;
                }

                return new MemberJoinedActivity(activity, user);
            });
        }

        /// <summary>
        /// Converts a paged list of generic activities into a paged list of more specific activity instances
        /// </summary>
        /// <param name="activities">Paged list of activities where each member may be a specific activity instance e.g. a profile updated activity</param>
        /// <param name="pageIndex"> </param>
        /// <param name="pageSize"> </param>
        /// <returns></returns>
        private PagedList<ActivityBase> ConvertToSpecificActivities(PagedList<Activity> activities, int pageIndex, int pageSize)
        {
            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "ConvertToSpecificActivities-", activities.GetHashCode(), "-", pageIndex, "-", pageSize);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var listedResults = ConvertToSpecificActivities(activities);
                return new PagedList<ActivityBase>(listedResults, pageIndex, pageSize, activities.Count);
            });
        }

        /// <summary>
        /// Converts a paged list of generic activities into a list of more specific activity instances
        /// </summary>
        /// <param name="activities"></param>
        /// <returns></returns>
        private IEnumerable<ActivityBase> ConvertToSpecificActivities(IEnumerable<Activity> activities)
        {
            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "ConvertToSpecificActivities-", activities.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var listedResults = new List<ActivityBase>();
                foreach (var activity in activities)
                {
                    if (activity.Type == ActivityType.BadgeAwarded.ToString())
                    {
                        var badgeActivity = GenerateBadgeActivity(activity);

                        if (badgeActivity != null)
                        {
                            listedResults.Add(badgeActivity);
                        }

                    }
                    else if (activity.Type == ActivityType.MemberJoined.ToString())
                    {
                        var memberJoinedActivity = GenerateMemberJoinedActivity(activity);

                        if (memberJoinedActivity != null)
                        {
                            listedResults.Add(memberJoinedActivity);
                        }
                    }
                    else if (activity.Type == ActivityType.ProfileUpdated.ToString())
                    {

                        var profileUpdatedActivity = GenerateProfileUpdatedActivity(activity);

                        if (profileUpdatedActivity != null)
                        {
                            listedResults.Add(profileUpdatedActivity);
                        }
                    }
                }
                return listedResults;
            });
        } 
        #endregion

        /// <summary>
        /// Gets a paged list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<ActivityBase> GetPagedGroupedActivities(int pageIndex, int pageSize)
        {
            // Read the database for all activities and convert each to a more specialised activity type

            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "GetPagedGroupedActivities-", pageIndex, "-", pageSize);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var totalCount = _context.Activity.Count();
                var results = _context.Activity
                      .OrderByDescending(x => x.Timestamp)
                      .Skip((pageIndex - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();

                // Return a paged list
                var activities = new PagedList<Activity>(results, pageIndex, pageSize, totalCount);

                // Convert
                var specificActivities = ConvertToSpecificActivities(activities, pageIndex, pageSize);

                return specificActivities;
            });

        }

        /// <summary>
        /// Gets all activities by search data field for a Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public IEnumerable<Activity> GetDataFieldByGuid(Guid guid)
        {
            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "GetDataFieldByGuid-", guid);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Activity.Where(x => x.Data.Contains(guid.ToString())));
        }

        public PagedList<ActivityBase> SearchPagedGroupedActivities(string search, int pageIndex, int pageSize)
        {
            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "SearchPagedGroupedActivities-", search, "-", pageIndex, "-", pageSize);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                // Read the database for all activities and convert each to a more specialised activity type
                search = StringUtils.SafePlainText(search);
                var totalCount = _context.Activity.Count(x => x.Type.ToUpper().Contains(search.ToUpper()));

                // Get the topics using an efficient
                var results = _context.Activity
                      .Where(x => x.Type.ToUpper().Contains(search.ToUpper()))
                      .OrderByDescending(x => x.Timestamp)
                      .Skip((pageIndex - 1) * pageSize)
                      .Take(pageSize)
                      .ToList();

                // Return a paged list
                var activities = new PagedList<Activity>(results, pageIndex, pageSize, totalCount);

                // Convert
                var specificActivities = ConvertToSpecificActivities(activities, pageIndex, pageSize);

                return specificActivities;
            });
        }

        public IEnumerable<ActivityBase> GetAll(int howMany)
        {
            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "GetAll-", howMany);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var activities = _context.Activity.Take(howMany);
                var specificActivities = ConvertToSpecificActivities(activities);
                return specificActivities;
            });
        }

        /// <summary>
        /// New badge has been awarded
        /// </summary>
        /// <param name="badge"></param>
        /// <param name="user"> </param>
        /// <param name="timestamp"> </param>
        public void BadgeAwarded(Badge badge, MembershipUser user, DateTime timestamp)
        {
            var badgeActivity = BadgeActivity.GenerateMappedRecord(badge, user, timestamp);
            Add(badgeActivity);
        }

        /// <summary>
        /// New member has joined
        /// </summary>
        /// <param name="user"></param>
        public void MemberJoined(MembershipUser user)
        {
            var memberJoinedActivity = MemberJoinedActivity.GenerateMappedRecord(user);
            Add(memberJoinedActivity);
        }

        /// <summary>
        /// Profile has been updated
        /// </summary>
        /// <param name="user"></param>
        public void ProfileUpdated(MembershipUser user)
        {
            var profileUpdatedActivity = ProfileUpdatedActivity.GenerateMappedRecord(user, DateTime.UtcNow);
            Add(profileUpdatedActivity);
        }

        /// <summary>
        /// Delete a list of activities
        /// </summary>
        /// <param name="activities"></param>
        public void Delete(IList<Activity> activities)
        {
            foreach (var activity in activities)
            {
                Delete(activity);
            }
        }

        public Activity Add(Activity newActivity)
        {
            return _context.Activity.Add(newActivity);
        }

        public Activity Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.Activity.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Activity.FirstOrDefault(x => x.Id == id));
        }

        public void Delete(Activity item)
        {
            _context.Activity.Remove(item);
        }
    }
}

