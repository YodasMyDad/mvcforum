using System;
using System.Collections.Generic;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    [TestFixture]
    public class EventManagerTests
    {
        private IActivityService _activityService;
        private const string TestString = "testeventhandlerstring";
        private IMVCForumAPI _api;
        private IPrivateMessageService _privateMessageService;
        private IMembershipUserPointsService _membershipUserPointsService;
        private ITopicNotificationService _topicNotificationService;
        private IVoteService _voteService;
        private IBadgeService _badgeService;
        private ICategoryNotificationService _categoryNotificationService;

        [SetUp]
        public void Init()
        {
            _activityService = Substitute.For<IActivityService>();
            BadgeServiceTests.AppendBadgeClassPath();
            _api = Substitute.For<IMVCForumAPI>();
            _privateMessageService = Substitute.For<IPrivateMessageService>();
            _membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            _topicNotificationService = Substitute.For<ITopicNotificationService>();
            _voteService = Substitute.For<IVoteService>();
            _badgeService = Substitute.For<IBadgeService>();
            _categoryNotificationService = Substitute.For<ICategoryNotificationService>();
        }

        [Test]
        public void BeforeUserRegisteredAllow()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();

            var settingsRepository = Substitute.For<ISettingsRepository>();
            settingsRepository.GetSettings().Returns(new Settings { NewMemberStartingRole = new MembershipRole{RoleName = "Test"} });
           
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();

            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _privateMessageService,
                _membershipUserPointsService, _topicNotificationService, _voteService, _badgeService, _categoryNotificationService, _api);
            EventManager.Instance.BeforeRegisterUser += EventManagerInstance_BeforeRegisterUserAllow;

            var newUser = new MembershipUser { UserName = "SpongeBob", Password = "Test" };
            membershipRepository.GetUser(newUser.UserName).Returns(x => null);
            membershipService.CreateUser(newUser);

            membershipRepository.Received().Add(Arg.Is<MembershipUser>(x => x.UserName == "SpongeBob"));
            Assert.AreEqual(newUser.Email, TestString);
            EventManager.Instance.BeforeRegisterUser -= EventManagerInstance_BeforeRegisterUserAllow;
        }

        private void EventManagerInstance_BeforeRegisterUserAllow(object sender, RegisterUserEventArgs args)
        {
            args.User.Email = TestString;
            args.Cancel = false;
        }

        [Test]
        public void BeforeUserRegisteredCancel()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();

            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();

            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _privateMessageService,
                _membershipUserPointsService, _topicNotificationService, _voteService, _badgeService, _categoryNotificationService, _api);
            EventManager.Instance.BeforeRegisterUser += EventManagerInstance_BeforeRegisterUserCancel;

            var newUser = new MembershipUser { UserName = "SpongeBob", Password = "Test" };
            membershipRepository.GetUser(newUser.UserName).Returns(x => null);
            membershipService.CreateUser(newUser);

            membershipRepository.DidNotReceive().Add(Arg.Is<MembershipUser>(x => x.UserName == "SpongeBob"));
            EventManager.Instance.BeforeRegisterUser -= EventManagerInstance_BeforeRegisterUserCancel;
        }

        private void EventManagerInstance_BeforeRegisterUserCancel(object sender, RegisterUserEventArgs args)
        {
            args.Cancel = true;
        }

        [Test]
        public void AfterUserRegistered()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();

            var settingsRepository = Substitute.For<ISettingsRepository>();
            settingsRepository.GetSettings().Returns(new Settings { NewMemberStartingRole = new MembershipRole{RoleName = "Test"} });
            
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();

            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _privateMessageService,
                _membershipUserPointsService, _topicNotificationService, _voteService, _badgeService, _categoryNotificationService, _api);
            EventManager.Instance.AfterRegisterUser += EventManagerInstance_AfterRegisterUser;

            var newUser = new MembershipUser {UserName = "SpongeBob", Password = "Test"};
            membershipRepository.GetUser(newUser.UserName).Returns(x => null);
            membershipService.CreateUser(newUser);

            Assert.AreEqual(newUser.Email, TestString);
            EventManager.Instance.AfterRegisterUser -= EventManagerInstance_AfterRegisterUser;
        }

        private void EventManagerInstance_AfterRegisterUser(object sender, RegisterUserEventArgs args)
        {
            args.User.Email = TestString;
        }

        [Test]
        public void BeforeBadgeAwardedCancel()
        {
            EventManager.Instance.BeforeBadgeAwarded += _EventManagerInstance_BeforeBadgeAwardedCancel;
            var badgeRepository = Substitute.For<IBadgeRepository>();
            var loggingService = Substitute.For<ILoggingService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var badgeService = new BadgeService(badgeRepository,  _api, loggingService, localisationService, _activityService);
            badgeService.SyncBadges();

            // Create a user with one post with one vote
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Votes = new List<Vote> { new Vote { Id = Guid.NewGuid() } },
            };

            var user = new MembershipUser
            {
                Posts = new List<Post> { post },
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = BadgeServiceTests.GetTimeAllowsBadgeUpdate()} }
            };

            badgeService.ProcessBadge(BadgeType.VoteUp, user);

            // Event should have cancelled the update
            Assert.IsTrue(user.Badges.Count == 0);
            Assert.IsFalse(user.Email == TestString);
            EventManager.Instance.BeforeBadgeAwarded -= _EventManagerInstance_BeforeBadgeAwardedCancel;
        }

        private void _EventManagerInstance_BeforeBadgeAwardedCancel(object sender, BadgeEventArgs args)
        {
            args.Cancel = true;
        }

        [Test]
        public void BeforeBadgeAwardedAllow()
        {
            EventManager.Instance.BeforeBadgeAwarded += _EventManagerInstance_BeforeBadgeAwardedAllow;
            var badgeRepository = Substitute.For<IBadgeRepository>();
            var api = Substitute.For<IMVCForumAPI>();
            var loggingService = Substitute.For<ILoggingService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var badgeService = new BadgeService(badgeRepository, api, loggingService, localisationService, _activityService);
            badgeService.SyncBadges();

            // Create a user with one post with one vote
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Votes = new List<Vote> { new Vote { Id = Guid.NewGuid() } },
            };

            var user = new MembershipUser
            {
                Posts = new List<Post> { post },
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = BadgeServiceTests.GetTimeAllowsBadgeUpdate()} }
            };

            badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge{Name="testbadge"});
            badgeService.ProcessBadge(BadgeType.VoteUp, user);

            // Event should have cancelled the update
            Assert.IsTrue(user.Badges.Count == 2);

            EventManager.Instance.BeforeBadgeAwarded -= _EventManagerInstance_BeforeBadgeAwardedAllow;
        }

        private void _EventManagerInstance_BeforeBadgeAwardedAllow(object sender, BadgeEventArgs args)
        {
            args.Cancel = false;
        }


        [Test]
        public void AfterBadgeAwarded()
        {
            EventManager.Instance.AfterBadgeAwarded += _EventManagerInstance_AfterBadgeAwarded;
            var badgeRepository = Substitute.For<IBadgeRepository>();
            var api = Substitute.For<IMVCForumAPI>();
            var loggingService = Substitute.For<ILoggingService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var badgeService = new BadgeService(badgeRepository, api, loggingService, localisationService, _activityService);
            badgeService.SyncBadges();

            // Create a user with one post with one vote
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Votes = new List<Vote> { new Vote { Id = Guid.NewGuid() } },
            };

            var user = new MembershipUser
            {
                Posts = new List<Post> { post },
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = BadgeServiceTests.GetTimeAllowsBadgeUpdate()} }
            };

            badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "PosterVoteUp" });
            badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 1);
            Assert.IsTrue(user.Badges[0].Name == "PosterVoteUp" || user.Badges[0].Name == BadgeServiceTests.NameTestVoteUp);

            // Event has been called test
            Assert.IsTrue(user.Email == TestString);
            EventManager.Instance.AfterBadgeAwarded -= _EventManagerInstance_AfterBadgeAwarded;
        }

        private void _EventManagerInstance_AfterBadgeAwarded(object sender, BadgeEventArgs args)
        {
            args.User.Email = TestString;
        }

        private string _eventTestStr = string.Empty;

        [Test]
        public void BeforeVoteMadeAllow()
        {
            var voteRepository = Substitute.For<IVoteRepository>();
            var voteService = new VoteService(voteRepository, _api);

            EventManager.Instance.BeforeVoteMade += EventManagerInstanceBeforeVoteMadeAllow;

            var vote = new Vote { Amount = 1 };

            _eventTestStr = string.Empty;
            voteService.Add(vote);

            voteRepository.Received().Add((Arg.Is<Vote>(x => x.Amount == 1)));
            EventManager.Instance.BeforeVoteMade -= EventManagerInstanceBeforeVoteMadeAllow;
        }

        private void EventManagerInstanceBeforeVoteMadeAllow(object sender, VoteEventArgs args)
        {
            args.Cancel = false;
        }

        [Test]
        public void BeforeVoteMadeCancel()
        {
            var voteRepository = Substitute.For<IVoteRepository>();
            var voteService = new VoteService(voteRepository, _api);

            EventManager.Instance.BeforeVoteMade += EventManagerInstanceBeforeVoteMadeCancel;

            var vote = new Vote { Amount = 999 };

            _eventTestStr = string.Empty;
            voteService.Add(vote);

            voteRepository.DidNotReceive().Update((Arg.Is<Vote>(x => x.Amount == 999)));
            EventManager.Instance.BeforeVoteMade -= EventManagerInstanceBeforeVoteMadeCancel;
        }

        private void EventManagerInstanceBeforeVoteMadeCancel(object sender, VoteEventArgs args)
        {
            args.Cancel = true;
        }

        [Test]
        public void AfterVoteMade()
        {
            var voteRepository = Substitute.For<IVoteRepository>();
            var voteService = new VoteService(voteRepository, _api);

            EventManager.Instance.AfterVoteMade += EventManagerInstanceAfterVoteMade;

            var vote = new Vote { Amount = 1 };

            _eventTestStr = string.Empty;
            voteService.Add(vote);

            Assert.IsTrue(_eventTestStr == TestString);
            EventManager.Instance.AfterVoteMade -= EventManagerInstanceAfterVoteMade;
        }

        private void EventManagerInstanceAfterVoteMade(object sender, VoteEventArgs args)
        {
            _eventTestStr = TestString;
        }

        [Test]
        public void BeforeMarkedAsSolutionCancel()
        {
            var topicRepository = Substitute.For<ITopicRepository>();
            var postRepository = Substitute.For<IPostRepository>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();

            var topicService = new TopicService(membershipUserPointsService, settingsService, topicRepository, postRepository, _api, _topicNotificationService);
            var marker = new MembershipUser
            {
                UserName = "SpongeBob",
                Id = Guid.NewGuid()
            };

            var topic = new Topic
            {
                Name = "something",
                Tags = new List<TopicTag>
                                           {
                                               new TopicTag{Tag = "tagone"},
                                               new TopicTag{Tag = "tagtwo"}
                                           },
                User = marker
            };

            var post = new Post { PostContent = "Test content" };

            var solutionWriter = new MembershipUser { UserName = "Squidward" };

            EventManager.Instance.BeforeMarkedAsSolution += eventsService_BeforeMarkedAsSolutionCancel;
            topicService.SolveTopic(topic, post, marker, solutionWriter);

            postRepository.DidNotReceive().Update(Arg.Is<Post>(x => x.PostContent == "Test content"));
            Assert.IsFalse(topic.Solved);
            EventManager.Instance.BeforeMarkedAsSolution -= eventsService_BeforeMarkedAsSolutionCancel;

        }

        private void eventsService_BeforeMarkedAsSolutionCancel(object sender, MarkedAsSolutionEventArgs args)
        {
            args.Cancel = true;
        }

        [Test]
        public void BeforeMarkedAsSolutionAllow()
        {
            var topicRepository = Substitute.For<ITopicRepository>();
            var postRepository = Substitute.For<IPostRepository>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();
            settingsService.GetSettings().Returns(new Settings { PointsAddedForSolution = 20 });

            var topicService = new TopicService(membershipUserPointsService, settingsService, topicRepository, postRepository, _api, _topicNotificationService);
            var marker = new MembershipUser
            {
                UserName = "SpongeBob",
                Id = Guid.NewGuid()
            };

            var topic = new Topic
            {
                Name = "something",
                Tags = new List<TopicTag>
                                           {
                                               new TopicTag{Tag = "tagone"},
                                               new TopicTag{Tag = "tagtwo"}
                                           },
                User = marker
            };

            var post = new Post { PostContent = "Test content" };

            var solutionWriter = new MembershipUser { UserName = "Squidward" };

            EventManager.Instance.BeforeMarkedAsSolution += eventsService_BeforeMarkedAsSolutionAllow;
            topicService.SolveTopic(topic, post, marker, solutionWriter);

            Assert.IsTrue(topic.Solved);
            EventManager.Instance.BeforeMarkedAsSolution -= eventsService_BeforeMarkedAsSolutionAllow;

        }

        private void eventsService_BeforeMarkedAsSolutionAllow(object sender, MarkedAsSolutionEventArgs args)
        {
            args.Cancel = false;
        }

        [Test]
        public void AfterMarkedAsSolution()
        {
            var topicRepository = Substitute.For<ITopicRepository>();
            var postRepository = Substitute.For<IPostRepository>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();
            settingsService.GetSettings().Returns(new Settings{PointsAddedForSolution = 20});

            var topicService = new TopicService(membershipUserPointsService, settingsService, topicRepository, postRepository, _api, _topicNotificationService);
            var marker = new MembershipUser
            {
                UserName = "SpongeBob",
                Id = Guid.NewGuid()
            };

            var topic = new Topic
            {
                Name = "something",
                Tags = new List<TopicTag>
                                           {
                                               new TopicTag{Tag = "tagone"},
                                               new TopicTag{Tag = "tagtwo"}
                                           },
                User = marker
            };

            var post = new Post { PostContent = "Test content" };

            var solutionWriter = new MembershipUser { UserName = "Squidward" };

            EventManager.Instance.AfterMarkedAsSolution += eventsService_AfterMarkedAsSolution;
            topicService.SolveTopic(topic, post, marker, solutionWriter);

            Assert.IsTrue(topic.Solved);
            Assert.AreEqual(solutionWriter.Email, TestString);
            EventManager.Instance.AfterMarkedAsSolution -= eventsService_AfterMarkedAsSolution;
        }

        private void eventsService_AfterMarkedAsSolution(object sender, MarkedAsSolutionEventArgs args)
        {
            args.SolutionWriter.Email = TestString;
        }

        [Test]
        public void BeforePostMadeAllow()
        {
            var postRepository = Substitute.For<IPostRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var roleService = Substitute.For<IRoleService>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();
            settingsService.GetSettings().Returns(new Settings { PointsAddedPerPost = 20 });

            var localisationService = Substitute.For<ILocalizationService>();
            var postService = new PostService(membershipUserPointsService, settingsService, roleService, postRepository, topicRepository, localisationService, _api);

            var category = new Category();
            var role = new MembershipRole { RoleName = "TestRole" };

            var categoryPermissionForRoleSet = new List<CategoryPermissionForRole>
                                                   {
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionEditPosts }, IsTicked = true},
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionDenyAccess }, IsTicked = false},
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionReadOnly  }, IsTicked = false}
                                                   };

            var permissionSet = new PermissionSet(categoryPermissionForRoleSet);
            roleService.GetPermissions(category, role).Returns(permissionSet);

            var topic = new Topic { Name = "Captain America", Category = category };
            var user = new MembershipUser
            {
                UserName = "SpongeBob",
                Roles = new List<MembershipRole> { role }
            };

            EventManager.Instance.BeforePostMade += eventsService_BeforePostMadeAllow;
            postService.AddNewPost("A test post", topic, user, out permissionSet);

            membershipUserPointsService.Received().Add(Arg.Is<MembershipUserPoints>(x => x.User == user));

            EventManager.Instance.BeforePostMade -= eventsService_BeforePostMadeAllow;
        }

        private void eventsService_BeforePostMadeAllow(object sender, PostMadeEventArgs args)
        {
            args.Cancel = false;
        }

        [Test]
        public void BeforePostMadeCancel()
        {
            var postRepository = Substitute.For<IPostRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var roleService = Substitute.For<IRoleService>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();

            var localisationService = Substitute.For<ILocalizationService>();
            var postService = new PostService(membershipUserPointsService, settingsService, roleService, postRepository, topicRepository, localisationService, _api);

            var category = new Category();
            var role = new MembershipRole { RoleName = "TestRole" };

            var categoryPermissionForRoleSet = new List<CategoryPermissionForRole>
                                                   {
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionEditPosts }, IsTicked = true},
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionDenyAccess }, IsTicked = false},
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionReadOnly  }, IsTicked = false}
                                                   };

            var permissionSet = new PermissionSet(categoryPermissionForRoleSet);
            roleService.GetPermissions(category, role).Returns(permissionSet);

            var topic = new Topic { Name = "Captain America", Category = category };
            var user = new MembershipUser
            {
                UserName = "SpongeBob",
                Roles = new List<MembershipRole>() { role }
            };

            EventManager.Instance.BeforePostMade += eventsService_BeforePostMadeCancel;
            postService.AddNewPost("A test post", topic, user, out permissionSet);

            membershipUserPointsService.DidNotReceive().Add(Arg.Is<MembershipUserPoints>(x => x.User == user));
            EventManager.Instance.BeforePostMade -= eventsService_BeforePostMadeCancel;
        }

        private void eventsService_BeforePostMadeCancel(object sender, PostMadeEventArgs args)
        {
            args.Cancel = true;
        }



        [Test]
        public void AfterPostMade()
        {
            var postRepository = Substitute.For<IPostRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var roleService = Substitute.For<IRoleService>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();
            settingsService.GetSettings().Returns(new Settings { PointsAddedPerPost = 20 });

            var localisationService = Substitute.For<ILocalizationService>();
            var postService = new PostService(membershipUserPointsService, settingsService, roleService, postRepository, topicRepository, localisationService, _api);

            var category = new Category();
            var role = new MembershipRole { RoleName = "TestRole" };

            var categoryPermissionForRoleSet = new List<CategoryPermissionForRole>
                                                   {
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionEditPosts }, IsTicked = true},
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionDenyAccess }, IsTicked = false},
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionReadOnly  }, IsTicked = false}
                                                   };

            var permissionSet = new PermissionSet(categoryPermissionForRoleSet);
            roleService.GetPermissions(category, role).Returns(permissionSet);

            var topic = new Topic { Name = "Captain America", Category = category };
            var user = new MembershipUser
            {
                UserName = "SpongeBob",
                Roles = new List<MembershipRole> { role }
            };

            EventManager.Instance.AfterPostMade += eventsService_AfterPostMade;
            var newPost = postService.AddNewPost("A test post", topic, user, out permissionSet);

            Assert.AreEqual(newPost.User, user);
            Assert.AreEqual(newPost.Topic, topic);
            Assert.AreEqual(newPost.PostContent, TestString);
            EventManager.Instance.AfterPostMade -= eventsService_AfterPostMade;
        }

        private void eventsService_AfterPostMade(object sender, PostMadeEventArgs args)
        {
            args.Post.PostContent = TestString;
        }

        [Test]
        public void BeforeUpdateProfileAllow()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _privateMessageService,
                _membershipUserPointsService, _topicNotificationService, _voteService, _badgeService, _categoryNotificationService, _api);

            var user = new MembershipUser { UserName = "SpongeBob" };

            EventManager.Instance.BeforeUpdateProfile += eventsService_BeforeUpdateProfileAllow;
            membershipService.Save(user);

            membershipRepository.Received().Update(Arg.Is<MembershipUser>(x => x.UserName == "SpongeBob"));
            EventManager.Instance.BeforeUpdateProfile -= eventsService_BeforeUpdateProfileAllow;
        }

        private void eventsService_BeforeUpdateProfileAllow(object sender, UpdateProfileEventArgs args)
        {
            args.Cancel = false;
        }

        [Test]
        public void BeforeUpdateProfileCancel()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _privateMessageService,
                _membershipUserPointsService, _topicNotificationService, _voteService, _badgeService, _categoryNotificationService, _api);

            var user = new MembershipUser { UserName = "SpongeBob" };

            EventManager.Instance.BeforeUpdateProfile += eventsService_BeforeUpdateProfileCancel;
            membershipService.ProfileUpdated(user);

            membershipRepository.DidNotReceive().Update(Arg.Is<MembershipUser>(x => x.UserName == "SpongeBob"));
            EventManager.Instance.BeforeUpdateProfile -= eventsService_BeforeUpdateProfileCancel;
        }

        private void eventsService_BeforeUpdateProfileCancel(object sender, UpdateProfileEventArgs args)
        {
            args.Cancel = true;
        }

        [Test]
        public void AfterUpdateProfile()
        {
            var membershipRepository = Substitute.For<IMembershipRepository>();
            var settingsRepository = Substitute.For<ISettingsRepository>();
            var emailService = Substitute.For<IEmailService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var membershipService = new MembershipService(membershipRepository, settingsRepository, emailService, localisationService, _activityService, _privateMessageService,
                _membershipUserPointsService, _topicNotificationService, _voteService, _badgeService, _categoryNotificationService, _api);

            var user = new MembershipUser {UserName = "SpongeBob"};

            EventManager.Instance.AfterUpdateProfile += eventsService_AfterUpdateProfile;
            membershipService.ProfileUpdated(user);

            Assert.AreEqual(user.Email, TestString);
            EventManager.Instance.AfterUpdateProfile -= eventsService_AfterUpdateProfile;
        }

        private void eventsService_AfterUpdateProfile(object sender, UpdateProfileEventArgs args)
        {
            args.User.Email = TestString;
        }
    }
}
