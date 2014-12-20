using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Attributes;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Badges;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    #region Badge Classes for testing. NOTE there are TWO poster vote up badges

    [Id(BadgeServiceTests.GuidTestVoteUp)]
    [Name(BadgeServiceTests.NameTestVoteUp)]
    [DisplayName(BadgeServiceTests.DisplayNameTestVoteUp)]
    [Domain.DomainModel.Attributes.Description(BadgeServiceTests.DescriptionTestVoteUp)]
    [Image(BadgeServiceTests.ImageTestVoteUp)]

    public class PosterVoteUpBadge2 : IVoteUpBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            return user.Posts != null && user.Posts.Any(post => post.Votes.Count > 0);
        }
    }

    [Id("2ac1fc11-2f9e-4d5a-9df4-29715f10b6d1")]
    [Name("PosterVoteUp")]
    [DisplayName("First Vote Up Received")]
    [Domain.DomainModel.Attributes.Description("This badge is awarded to users after they receive their first vote up from another user.")]
    [Image("PosterVoteUpBadge.png")]
    public class PosterVoteUpBadge : IVoteUpBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            return user.Posts != null && user.Posts.Any(post => post.Votes.Count > 0);
        }
    }


    [Id("c9913ee2-b8e0-4543-8930-c723497ee65c")]
    [Name("UserVoteUp")]
    [DisplayName("You've Given Your First Vote Up")]
    [Domain.DomainModel.Attributes.Description("This badge is awarded to users after they make their first vote up.")]
    [Image("UserVoteUpBadge.png")]
    public class UserVoteUpBadge : IVoteUpBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            return user.Votes != null && user.Votes.Count >= 1;
        }
    }

    [Id("52284d2b-7ed6-4154-9ccc-3a7d99b18cca")]
    [Name("MemberForAYear")]
    [DisplayName("Member For A Year")]
    [Domain.DomainModel.Attributes.Description("This badge is awarded to a user after their first year anniversary.")]
    [Image("MemberForAYearBadge.png")]
    public class MemberForAYearBadge : ITimeBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            var anniversary = new DateTime(user.CreateDate.Year + 1, user.CreateDate.Month, user.CreateDate.Day);
            return DateTime.UtcNow >= anniversary;
        }
    }

    [Id("8250f9f0-84d2-4dff-b651-c3df9e12bf2a")]
    [Name("PosterMarkAsSolution")]
    [DisplayName("Post Selected As Answer")]
    [Domain.DomainModel.Attributes.Description("This badge is awarded to the poster of a post marked as the topic answer, the first time they author an answer.")]
    [Image("PosterMarkAsSolutionBadge.png")]
    public class PosterMarkAsSolutionBadge : IMarkAsSolutionBadge
    {
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            //Post is marked as the answer to a topic - give the post author a badge
            var usersSolutions = api.Post.GetSolutionsWrittenByMember(user.Id);

            return (usersSolutions.Count >= 1);
        }
    }

    [Id("d68c289a-e3f7-4f55-ae4f-fc7ac2147781")]
    [Name("AuthorMarkAsSolution")]
    [DisplayName("Your Question Solved")]
    [Domain.DomainModel.Attributes.Description("This badge is awarded to topic authors the first time they have a post marked as the answer.")]
    [Image("UserMarkAsSolutionBadge.png")]
    public class AuthorMarkAsSolutionBadge : IMarkAsSolutionBadge
    {
        /// <summary>
        /// Post is marked as the answer to a topic - give the topic author a badge
        /// </summary>
        /// <returns></returns>
        public bool Rule(MembershipUser user, IMVCForumAPI api)
        {
            return api.Topic.GetSolvedTopicsByMember(user.Id).Count >= 1;
        }
    }
    #endregion

    [TestFixture]
    public class BadgeServiceTests
    {
        
        public const string GuidTestVoteUp = "dcabc59b-0e56-4644-be10-48bc46ba0fd2";
        public const string NameTestVoteUp = "TestVoteUpName";
        public const string DisplayNameTestVoteUp = "TestVoteUpDisplayName";
        public const string DescriptionTestVoteUp = "TestVoteUpDesc";
        public const string ImageTestVoteUp = "TestVoteUpImage";

        private static bool _pathAppended;

        private IBadgeService _badgeService;
        private IBadgeRepository _badgeRepository;
        private IMVCForumAPI _api;
        private ILoggingService _loggingService;
        private ILocalizationService _localizationService;
        private IActivityService _activityService;

        [SetUp]
        public void Init()
        {
            _api = Substitute.For<IMVCForumAPI>();
            _loggingService = Substitute.For<ILoggingService>();
            _localizationService = Substitute.For<ILocalizationService>();
            _activityService = Substitute.For<IActivityService>();

            AppendBadgeClassPath();

            // Ensure a database sync
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository,  _api, _loggingService, _localizationService, _activityService);
            _badgeService.SyncBadges();
        }

        [Test]
        public void SyncBadgesCreateNewBadgeRecords()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);
            _badgeService.SyncBadges();
            _badgeRepository.Received().Add(Arg.Is<Badge>(x => x.Name == "PosterVoteUp"
                && x.Id.ToString() == "2ac1fc11-2f9e-4d5a-9df4-29715f10b6d1"
                && x.Image == "PosterVoteUpBadge.png"
                && x.Description == "This badge is awarded to users after they receive their first vote up from another user."));
        }

        [Test]
        public void SyncBadgesDeleteOldBadgeRecords()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            var badgesInDbAlready = new List<Badge>
                                        {
                                            new Badge
                                                {
                                                    Id = new Guid("2ac1fc11-2f9e-4d5a-9df4-29715f10b6d1"),
                                                    Name = "PosterVoteUp"
                                                },
                                            new Badge
                                                {
                                                    Id = new Guid("2ac1fc11-2f9e-4d5a-9df4-29715f10b6d2"),
                                                    Name = "BadgeWithNoMatchingClass"
                                                }
                                        };
            _badgeRepository.GetAll().Returns(badgesInDbAlready);

            _badgeService.SyncBadges();

            _badgeRepository.Received().Delete(Arg.Is<Badge>(x => x.Name == "BadgeWithNoMatchingClass"));
        }

        [Test]
        public void SyncBadgesUpdateBadgeRecords()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            var badgeInDb = new Badge
                                {
                                    Id = new Guid(GuidTestVoteUp),
                                    Name = "XXX",
                                    DisplayName = "XXX",
                                    Description = "XXX",
                                    Image = "XXX"
                                };
   
            var badgesInDbAlready = new List<Badge>
                                        {
                                           badgeInDb                                          
                                        };

            _badgeRepository.GetAll().Returns(badgesInDbAlready);

            _badgeService.SyncBadges();

            // The test badge class has been identified as the same badge as found in the "database", so that database
            // badge's fields have been updated with the test badge class's attributes
            Assert.IsTrue(badgeInDb.Name == NameTestVoteUp);
            Assert.IsTrue(badgeInDb.Description == DescriptionTestVoteUp);
            Assert.IsTrue(badgeInDb.DisplayName == DisplayNameTestVoteUp);
            Assert.IsTrue(badgeInDb.Image == ImageTestVoteUp);
        }

        #region Vote Up Badges

        [Test]
        public void UserVoteUpAwardBadge()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            // Create a user with one vote no badges, set time checking offset to be safe so badge will be processed
            var user = new MembershipUser
            {
                Votes = new List<Vote>
                            {
                                new Vote { Id = Guid.NewGuid() }
                            },
                Badges = new List<Badge>(),

                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "UserVoteUp" });
            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 1);
            Assert.IsTrue(user.Badges[0].Name == "UserVoteUp");
        }

        [Test]
        public void UserVoteUpAwardBadgeTwoUsers()
        {
            // Useful for testing badge service internal badge class instance cache is called

            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            // Create a user with one vote no badges, set time checking offset to be safe so badge will be processed
            var user1 = new MembershipUser
            {
                Votes = new List<Vote>
                            {
                                new Vote { Id = Guid.NewGuid() }
                            },
                Badges = new List<Badge>(),

                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            // Create a user with one vote no badges, set time checking offset to be safe so badge will be processed
            var user2 = new MembershipUser
            {
                Votes = new List<Vote>
                            {
                                new Vote { Id = Guid.NewGuid() }
                            },
                Badges = new List<Badge>(),

                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "UserVoteUp" });
            _badgeService.ProcessBadge(BadgeType.VoteUp, user1);
            _badgeService.ProcessBadge(BadgeType.VoteUp, user2);

            Assert.IsTrue(user1.Badges.Count == 1);
            Assert.IsTrue(user1.Badges[0].Name == "UserVoteUp");
            Assert.IsTrue(user2.Badges.Count == 1);
            Assert.IsTrue(user2.Badges[0].Name == "UserVoteUp");
        }

        [Test]
        public void UserVoteUpAwardBadge2()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            // Create a user with two votes no badges - will create a badge because no previous badge
            var user = new MembershipUser
            {
                Votes = new List<Vote>
                            {
                                new Vote { Id = Guid.NewGuid() },
                                new Vote { Id = Guid.NewGuid() }
                            },
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "UserVoteUp" });
            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 1);
            Assert.IsTrue(user.Badges[0].Name == "UserVoteUp");

        }

        [Test]
        public void PosterVoteUpAwardBadge()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

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
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "testbadge" });
            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 2);
        }

 
        [Test]
        public void PosterVoteUpRefuseBadge_TooSoon()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository,  _api, _loggingService, _localizationService, _activityService);

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
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeDisAllowsBadgeUpdate()} }
            };

            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 0);
        }

        [Test]
        public void PosterVoteUpRefuseBadge_TooSoon_ThenAward()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

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
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeDisAllowsBadgeUpdate()} }
            };

            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 0);

            user.BadgeTypesTimeLastChecked[0].TimeLastChecked = GetTimeAllowsBadgeUpdate();
            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "testbadge" });

            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 2);
        }

        [Test]
        public void PosterVoteUpAwardBadge2()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository,  _api, _loggingService, _localizationService, _activityService);

            // Create a user with one post with two votes BUT user has no badge yet so will be awarded
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Votes = new List<Vote>
                            {
                                new Vote { Id = Guid.NewGuid() },
                                new Vote { Id = Guid.NewGuid() }
                            },
            };

            var user = new MembershipUser
            {
                Posts = new List<Post> { post },
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "testbadge" });
            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 2);

        }

        [Test]
        public void VoteUpAwardTwoBadges()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository,  _api, _loggingService, _localizationService, _activityService);

            // Create a user with one post with one vote, and no badge, and one post with one vote
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Votes = new List<Vote> { new Vote { Id = Guid.NewGuid() } },
            };

            var user = new MembershipUser
            {
                Posts = new List<Post> { post },
                Badges = new List<Badge>(),
                Votes = new List<Vote>
                            {
                                new Vote { Id = Guid.NewGuid() }
                            },
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "testbadge" });

            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 3);

            // These tests can no longer work as we now fetch back the db badge records using a substitute
            //var foundPosterVoteUp = false;
            //var foundUserVoteUp = false;

            //foreach (var badge in user.Badges)
            //{
            //    if (badge.Name == "PosterVoteUp" || badge.Name == NameTestVoteUp)
            //    {
            //        foundPosterVoteUp = true;
            //    }
            //    else if (badge.Name == "UserVoteUp")
            //    {
            //        foundUserVoteUp = true;
            //    }
            //}
            //Assert.IsTrue(foundPosterVoteUp);
            //Assert.IsTrue(foundUserVoteUp);
        }

        [Test]
        public void VoteUpAwardOneBadgeOutOfTwo()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            // Create a user with one post with one vote, and no badge, and one post with no votes
            var user = new MembershipUser
            {
                Posts = new List<Post> { 
                    new Post{ 
                            Id = Guid.NewGuid(),
                            Votes = new List<Vote> { new Vote { Id = Guid.NewGuid() } },
                            }    
                },
                Badges = new List<Badge>(),
                Votes = new List<Vote>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                        { new BadgeTypeTimeLastChecked 
                                { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "testbadge" });
            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 2);
        }

        #endregion

        #region Mark As Solution Badges

        [Test]
        public void TopicAuthorMarkAsSolutionAwardBadge()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _api.Topic = Substitute.For<ITopicAPI>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            // Create a user with one topic marked as solved 
            var id = Guid.NewGuid();
            var user = new MembershipUser
                           {
                               Id = id,
                               Badges = new List<Badge>(),
                               BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                                    { new BadgeTypeTimeLastChecked 
                                        { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
                                    };

                                var listTopicsForMember = new List<Topic> {
                                    new Topic
                                    {
                                        Solved = true,
                                        User = new MembershipUser
                                        {
                                            Id = user.Id,
                                            Badges = new List<Badge>()
                                        },  
                                    },
               };

            _api.Topic.GetSolvedTopicsByMember(id).Returns(listTopicsForMember);
            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "AuthorMarkAsSolution" });

            _badgeService.ProcessBadge(BadgeType.MarkAsSolution, user);

            Assert.IsTrue(user.Badges.Count == 1);
            Assert.IsTrue(user.Badges[0].Name == "AuthorMarkAsSolution");
        }

        [Test]
        public void TopicAuthorMarkAsSolutionAwardBadge2()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _api.Topic = Substitute.For<ITopicAPI>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            // Create a user with two topics marked as solved BUT no badge yet so will be awarded
            var id = Guid.NewGuid();
            var user = new MembershipUser
            {
                Id = id,
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            var listTopicsForMember = new List<Topic> 
            {
                new Topic
                            {
                                Solved = true,
                                User = new MembershipUser
                                {
                                    Id = user.Id,
                                    Badges = new List<Badge>()
                                },
                            },
                 new Topic
                            {
                                Solved = true,
                                User = new MembershipUser
                                {
                                    Id = user.Id,
                                    Badges = new List<Badge>()
                                },
                            },
               };

            _api.Topic.GetSolvedTopicsByMember(id).Returns(listTopicsForMember);
            _badgeService.ProcessBadge(BadgeType.MarkAsSolution, user);

            Assert.IsTrue(user.Badges.Count == 1);
        }

        [Test]
        public void PostAuthorMarkAsSolutionAwardBadge()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _api.Topic = Substitute.For<ITopicAPI>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            // Create a user with one topic marked as solved 
            var id = Guid.NewGuid();
            var user = new MembershipUser
            {
                Id = id,
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            var listPostsForMember = new List<Post> {
                new Post
                            {
                                User = new MembershipUser
                                {
                                    Id = user.Id,
                                    Badges = new List<Badge>()
                                },
                            },
               };

            _api.Post.GetSolutionsWrittenByMember(id).Returns(listPostsForMember);
            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "PosterMarkAsSolution" });
            _badgeService.ProcessBadge(BadgeType.MarkAsSolution, user);

            Assert.IsTrue(user.Badges.Count == 1);
            Assert.IsTrue(user.Badges[0].Name == "PosterMarkAsSolution");
        }

        [Test]
        public void PostAuthorMarkAsSolutionAwardBadge2()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _api.Topic = Substitute.For<ITopicAPI>();
            _badgeService = new BadgeService(_badgeRepository,  _api, _loggingService, _localizationService, _activityService);

            // Create a user with two posts marked as solved BUT user has NO badge to date so still awarded
            var id = Guid.NewGuid();
            var user = new MembershipUser
            {
                Id = id,
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            var listPostsForMember = new List<Post> {
                new Post
                            {
                                User = new MembershipUser
                                {
                                    Id = user.Id,
                                    Badges = new List<Badge>()
                                },
                            },
                new Post
                            {
                                User = new MembershipUser
                                {
                                    Id = user.Id,
                                    Badges = new List<Badge>()
                                },
                            },
               };

            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "testbadge" });
            _api.Post.GetSolutionsWrittenByMember(id).Returns(listPostsForMember);
            _badgeService.ProcessBadge(BadgeType.MarkAsSolution, user);

            Assert.IsTrue(user.Badges.Count == 1);
        }

        [Test]
        public void MarkAsSolutionAwardTwoBadges()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _api.Topic = Substitute.For<ITopicAPI>();
            _badgeService = new BadgeService(_badgeRepository,  _api, _loggingService, _localizationService, _activityService);

            // Create a user with one topic marked as solved 
            var id = Guid.NewGuid();
            var user = new MembershipUser
            {
                Id = id,
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            var listPostsForMember = new List<Post> {
                new Post
                            {
                                User = new MembershipUser
                                {
                                    Id = user.Id,
                                    Badges = new List<Badge>()
                                },
                            },
               };

            var listTopicsForMember = new List<Topic> 
            {
                 new Topic
                            {
                                Solved = true,
                                User = new MembershipUser
                                {
                                    Id = user.Id,
                                    Badges = new List<Badge>()
                                },
                            },
               };


            _api.Topic.GetSolvedTopicsByMember(id).Returns(listTopicsForMember);
            _api.Post.GetSolutionsWrittenByMember(id).Returns(listPostsForMember);
            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "testbadge" });
            _badgeService.ProcessBadge(BadgeType.MarkAsSolution, user);

            Assert.IsTrue(user.Badges.Count == 2);
        }        

        #endregion

        #region Time Badges

        [Test]
        public void AnniversaryTimeAwardBadge()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _api.Topic = Substitute.For<ITopicAPI>();
            _badgeService = new BadgeService(_badgeRepository,  _api, _loggingService, _localizationService, _activityService);

            var now = DateTime.UtcNow;
            var user = new MembershipUser
                           {
                               CreateDate = new DateTime(now.Year - 1, now.Month, now.Day),
                               Badges = new List<Badge>(),
                               BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                                    { new BadgeTypeTimeLastChecked 
                                        { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
                                               };

            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "MemberForAYear" });

            _badgeService.ProcessBadge(BadgeType.Time, user);

            Assert.IsTrue(user.Badges.Count == 1);
            Assert.IsTrue(user.Badges[0].Name == "MemberForAYear");
        }

        [Test]
        public void AnniversaryTimeBadgeRefuseBadge()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _api.Topic = Substitute.For<ITopicAPI>();
            _badgeService = new BadgeService(_badgeRepository,  _api, _loggingService, _localizationService, _activityService);

            var now = DateTime.UtcNow;
            var user = new MembershipUser
            {
                CreateDate = new DateTime(now.Year, now.Month, now.Day),
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            _badgeService.ProcessBadge(BadgeType.MarkAsSolution, user);

            Assert.IsTrue(user.Badges.Count == 0);
        }

        #endregion

        [Test]
        public void UserAlreadyHasBadge()
        {
            _badgeRepository = Substitute.For<IBadgeRepository>();
            _badgeService = new BadgeService(_badgeRepository, _api, _loggingService, _localizationService, _activityService);

            // Create a user with one vote no badges
            var user = new MembershipUser
            {
                Votes = new List<Vote>
                            {
                                new Vote { Id = Guid.NewGuid() }
                            },
                Badges = new List<Badge>(),
                BadgeTypesTimeLastChecked = new List<BadgeTypeTimeLastChecked> 
                { new BadgeTypeTimeLastChecked 
                    { BadgeType = BadgeType.VoteUp.ToString() , TimeLastChecked = GetTimeAllowsBadgeUpdate()} }
            };

            
            // Call twice but only one badge awarded
            _badgeRepository.Get(Arg.Any<Guid>()).Returns(new Badge { Name = "testbadge" });
            _badgeService.ProcessBadge(BadgeType.VoteUp, user);
            _badgeService.ProcessBadge(BadgeType.VoteUp, user);

            Assert.IsTrue(user.Badges.Count == 1);
        }

        #region Helper methods

        public static DateTime GetTimeAllowsBadgeUpdate()
        {
            var timeOffset = new TimeSpan(0, BadgeService.BadgeCheckIntervalMinutes + 1, 0);
            return DateTime.UtcNow.Subtract(timeOffset);            
        }

        public static DateTime GetTimeDisAllowsBadgeUpdate()
        {
            var timeOffset = new TimeSpan(0, BadgeService.BadgeCheckIntervalMinutes - 10, 0);
            return DateTime.UtcNow.Subtract(timeOffset);
        }

        public static void AppendBadgeClassPath()
        {
            // The badge service loads classes from the path found in the current domain - won't work in NUnit
            // so add a private path. There are badge clases in this test project which will be built into this path:
            if (!_pathAppended)
            {
                AppDomain.CurrentDomain.AppendPrivatePath(AppDomain.CurrentDomain.BaseDirectory);
                _pathAppended = true;
            }
        }
        #endregion
    }
}
