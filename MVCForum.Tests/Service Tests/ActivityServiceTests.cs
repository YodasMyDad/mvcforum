using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    [TestFixture]
    public class ActivityServiceTests
    {
        private IBadgeRepository _badgeRepository;
        private ILoggingService _loggingService;
        private IMembershipRepository _membershipRepository;

        [SetUp]
        public void Init()
        {
            _loggingService = Substitute.For<ILoggingService>();
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _membershipRepository = Substitute.For<IMembershipRepository>();
        }

        [Test]
        public void BadgeAwardedActivityAdd()
        {
            const string guidStrBadge = "515b7240-3be1-43d4-8846-c0b589cd1cd2";
            const string guidStrUser = "0c462b02-a991-48d8-8435-92f04e666675";

            var activityRepository = Substitute.For<IActivityRepository>();
            IActivityService activityService = new ActivityService(activityRepository, _badgeRepository, _membershipRepository, _loggingService);
            var user = new MembershipUser { Id = new Guid(guidStrUser), UserName = "SpongeBob" };
            var badge = new Badge { Id = new Guid(guidStrBadge) };

            activityService.BadgeAwarded(badge, user, DateTime.UtcNow);

            activityRepository.Received().Add((Arg.Is<Activity>(x => x.Data == BadgeActivity.KeyBadgeId + "=" + guidStrBadge + "," + BadgeActivity.KeyUserId + "=" + guidStrUser)));
        }

        [Test]
        public void BadgeAwardedActivityGet()
        {
            const string guidStr = "515b7240-3be1-43d4-8846-c0b589cd1cd2";
            const string guidStrUser = "0c462b02-a991-48d8-8435-92f04e666675";

            var timestamp = DateTime.UtcNow;
            var activityRepository = Substitute.For<IActivityRepository>();
            var badgeActivitiesInDb = new List<Activity>
                                          {
                                              new Activity
                                                  {
                                                      Type = ActivityType.BadgeAwarded.ToString(),
                                                      Timestamp = timestamp,
                                                      Data = BadgeActivity.KeyBadgeId + "=" + guidStr + "," + BadgeActivity.KeyUserId + "=" + guidStrUser
                                                  }
                                          };
            var pagedBadgeActivities = new PagedList<Activity>(badgeActivitiesInDb, 1, 20, 1);
            activityRepository.GetPagedGroupedActivities(1, 20).Returns(pagedBadgeActivities);
            _badgeRepository.Get(new Guid(guidStr)).Returns(new Badge());
            _membershipRepository.Get(new Guid(guidStrUser)).Returns(new MembershipUser());

            IActivityService activityService = new ActivityService(activityRepository, _badgeRepository, _membershipRepository, _loggingService);

            var badgeActivities = activityService.GetPagedGroupedActivities(1, 20);

            Assert.IsTrue(badgeActivities[0] is BadgeActivity);
        }

        [Test]
        public void MemberJoinedActivityAdd()
        {
            const string guidStr = "515b7240-3be1-43d4-8846-c0b589cd1cd2";
            var activityRepository = Substitute.For<IActivityRepository>();
            IActivityService activityService = new ActivityService(activityRepository, _badgeRepository, _membershipRepository, _loggingService);
            var user = new MembershipUser { Id = new Guid(guidStr), UserName = "SpongeBob" };

            activityService.MemberJoined(user);

            activityRepository.Received().Add((Arg.Is<Activity>(x => x.Data == MemberJoinedActivity.KeyUserId + "=" + guidStr)));
        }

        [Test]
        public void MemberJoinedActivityGet()
        {
            const string guidStr = "515b7240-3be1-43d4-8846-c0b589cd1cd2";

            var timestamp = DateTime.UtcNow;
            var activityRepository = Substitute.For<IActivityRepository>();

            _membershipRepository.Get(new Guid(guidStr)).Returns(new MembershipUser());
            var memberJoinedActivitiesInDb = new List<Activity>
                                          {
                                              new Activity
                                                  {
                                                      Type = ActivityType.MemberJoined.ToString(),
                                                      Timestamp = timestamp,
                                                      Data = MemberJoinedActivity.KeyUserId + "=" + guidStr
                                                  }
                                          };

            var pagedJoinedActivities = new PagedList<Activity>(memberJoinedActivitiesInDb, 1, 20, 1);
            activityRepository.GetPagedGroupedActivities(1, 20).Returns(pagedJoinedActivities);

            IActivityService activityService = new ActivityService(activityRepository, _badgeRepository, _membershipRepository, _loggingService);
            var memberJoinedActivities = activityService.GetPagedGroupedActivities(1, 20);

            Assert.IsTrue(memberJoinedActivities[0] is MemberJoinedActivity);
        }

        [Test]
        public void ProfileUpdatedActivityAdd()
        {
            const string guidStr = "515b7240-3be1-43d4-8846-c0b589cd1cd2";
            var activityRepository = Substitute.For<IActivityRepository>();
            IActivityService activityService = new ActivityService(activityRepository, _badgeRepository, _membershipRepository, _loggingService);
            var user = new MembershipUser { Id = new Guid(guidStr), UserName = "SpongeBob" };

            activityService.ProfileUpdated(user);

            activityRepository.Received().Add((Arg.Is<Activity>(x => x.Data == ProfileUpdatedActivity.KeyUserId + "=" + guidStr)));
        }

        [Test]
        public void ProfileUpdatedActivityGet()
        {
            const string guidStr = "515b7240-3be1-43d4-8846-c0b589cd1cd2";
            var timestamp = DateTime.UtcNow;
            var activityRepository = Substitute.For<IActivityRepository>();
            var profileUpdatedActivitiesInDb = new List<Activity>
                                          {
                                              new Activity
                                                  {
                                                      Type = ActivityType.ProfileUpdated.ToString(),
                                                      Timestamp = timestamp,
                                                      Data = ProfileUpdatedActivity.KeyUserId + "=" + guidStr
                                                  }
                                          };
            var pagedActivities = new PagedList<Activity>(profileUpdatedActivitiesInDb, 1, 20, 1);

            _membershipRepository.Get(new Guid(guidStr)).Returns(new MembershipUser());
            activityRepository.GetPagedGroupedActivities(1, 20).Returns(pagedActivities);
            IActivityService activityService = new ActivityService(activityRepository, _badgeRepository, _membershipRepository, _loggingService);

            var memberJoinedActivities = activityService.GetPagedGroupedActivities(1, 20);

            Assert.IsTrue(memberJoinedActivities[0] is ProfileUpdatedActivity);
        }
    }
}
