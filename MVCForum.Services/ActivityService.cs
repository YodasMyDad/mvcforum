using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class ActivityService : IActivityService
    {
        private readonly IActivityRepository _activityRepository;
        private readonly IMembershipRepository _membershipRepository;
        private readonly IBadgeRepository _badgeRepository;
        private readonly ILoggingService _loggingService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="activityRepository"> </param>
        /// <param name="badgeRepository"> </param>
        /// <param name="membershipRepository"> </param>
        /// <param name="loggingService"> </param>
        public ActivityService(IActivityRepository activityRepository, IBadgeRepository badgeRepository, IMembershipRepository membershipRepository, ILoggingService loggingService)
        {
            _activityRepository = activityRepository;
            _badgeRepository = badgeRepository;
            _loggingService = loggingService;
            _membershipRepository = membershipRepository;
        }

        /// <summary>
        /// Make a badge activity object from the more generic database activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private BadgeActivity GenerateBadgeActivity(Activity activity)
        {
            // Get the corresponding badge
            var dataPairs = ActivityBase.UnpackData(activity);

            if (!dataPairs.ContainsKey(BadgeActivity.KeyBadgeId))
            {
                // Log the problem then skip
                _loggingService.Error(string.Format("A badge activity record with id '{0}' has no badge id in its data.", activity.Id.ToString()));
                return null;
            }

            var badgeId = dataPairs[BadgeActivity.KeyBadgeId];
            var badge = _badgeRepository.Get(new Guid(badgeId));

            if (badge == null)
            {
                // Log the problem then skip
                _loggingService.Error(string.Format("A badge activity record with id '{0}' has a badge id '{1}' that is not found in the badge table.",
                    activity.Id.ToString(), badgeId));
                return null;
            }

            var userId = dataPairs[BadgeActivity.KeyUserId];
            var user = _membershipRepository.Get(new Guid(userId));

            if (user == null)
            {
                // Log the problem then skip
                _loggingService.Error(string.Format("A badge activity record with id '{0}' has a user id '{1}' that is not found in the user table.",
                    activity.Id, userId));
                return null;
            }

            return new BadgeActivity(activity, badge, user);
        }

        /// <summary>
        /// Make a profile updated joined activity object from the more generic database activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private ProfileUpdatedActivity GenerateProfileUpdatedActivity(Activity activity)
        {
            var dataPairs = ActivityBase.UnpackData(activity);

            if (!dataPairs.ContainsKey(ProfileUpdatedActivity.KeyUserId))
            {
                // Log the problem then skip
                _loggingService.Error(string.Format("A profile updated activity record with id '{0}' has no user id in its data.", activity.Id));
                return null;
            }

            var userId = dataPairs[ProfileUpdatedActivity.KeyUserId];
            var user = _membershipRepository.Get(new Guid(userId));

            if (user == null)
            {
                // Log the problem then skip
                _loggingService.Error(string.Format("A profile updated activity record with id '{0}' has a user id '{1}' that is not found in the user table.",
                    activity.Id, userId));
                return null;
            }

            return new ProfileUpdatedActivity(activity, user);
        }

        /// <summary>
        /// Make a member joined activity object from the more generic database activity object
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        private MemberJoinedActivity GenerateMemberJoinedActivity(Activity activity)
        {
            var dataPairs = ActivityBase.UnpackData(activity);

            if (!dataPairs.ContainsKey(MemberJoinedActivity.KeyUserId))
            {
                // Log the problem then skip
                _loggingService.Error(string.Format("A member joined activity record with id '{0}' has no user id in its data.", activity.Id));
                return null;
            }

            var userId = dataPairs[MemberJoinedActivity.KeyUserId];
            var user = _membershipRepository.Get(new Guid(userId));

            if (user == null)
            {
                // Log the problem then skip
                _loggingService.Error(string.Format("A member joined activity record with id '{0}' has a user id '{1}' that is not found in the user table.",
                    activity.Id, userId));
                return null;
            }

            return new MemberJoinedActivity(activity, user);
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
            var listedResults = ConvertToSpecificActivities(activities);

            return new PagedList<ActivityBase>(listedResults, pageIndex, pageSize, activities.Count);
        }

        /// <summary>
        /// Converts a paged list of generic activities into a list of more specific activity instances
        /// </summary>
        /// <param name="activities"></param>
        /// <returns></returns>
        private IEnumerable<ActivityBase> ConvertToSpecificActivities(IEnumerable<Activity> activities)
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
        }

        /// <summary>
        /// Gets a paged list of badges
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedList<ActivityBase> GetPagedGroupedActivities(int pageIndex, int pageSize)
        {
            // Read the database for all activities and convert each to a more specialised activity type

            var activities = _activityRepository.GetPagedGroupedActivities(pageIndex, pageSize);
            var specificActivities = ConvertToSpecificActivities(activities, pageIndex, pageSize);

            return specificActivities;
        }

        /// <summary>
        /// Gets all activities by search data field for a Guid
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public IEnumerable<Activity> GetDataFieldByGuid(Guid guid)
        {
            return _activityRepository.GetDataFieldByGuid(guid);
        }

        public PagedList<ActivityBase> SearchPagedGroupedActivities(string search, int pageIndex, int pageSize)
        {
            // Read the database for all activities and convert each to a more specialised activity type

            var activities = _activityRepository.SearchPagedGroupedActivities(StringUtils.SafePlainText(search), pageIndex, pageSize);
            var specificActivities = ConvertToSpecificActivities(activities, pageIndex, pageSize);

            return specificActivities;
        }

        public IEnumerable<ActivityBase> GetAll(int howMany)
        {
            var activities = _activityRepository.GetAll().Take(howMany);
            var specificActivities = ConvertToSpecificActivities(activities);
            return specificActivities;
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
            _activityRepository.Add(badgeActivity);
        }

        /// <summary>
        /// New member has joined
        /// </summary>
        /// <param name="user"></param>
        public void MemberJoined(MembershipUser user)
        {
            var memberJoinedActivity = MemberJoinedActivity.GenerateMappedRecord(user);
            _activityRepository.Add(memberJoinedActivity);
        }

        /// <summary>
        /// Profile has been updated
        /// </summary>
        /// <param name="user"></param>
        public void ProfileUpdated(MembershipUser user)
        {
            var profileUpdatedActivity = ProfileUpdatedActivity.GenerateMappedRecord(user, DateTime.UtcNow);
            _activityRepository.Add(profileUpdatedActivity);
        }

        /// <summary>
        /// Delete a list of activities
        /// </summary>
        /// <param name="activities"></param>
        public void Delete(IList<Activity> activities)
        {
            foreach (var activity in activities)
            {
                _activityRepository.Delete(activity);
            }
        }
    }
}

