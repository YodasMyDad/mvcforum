using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.API;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    [TestFixture]
    public class TopicServiceTests
    {
        private IMVCForumAPI _api;
        private ITopicNotificationService _topicNotificationService;
 
        [SetUp]
        public void Init()
        {
            _api = Substitute.For<IMVCForumAPI>();
            _topicNotificationService = Substitute.For<ITopicNotificationService>();
        }

        [Test]
        public void Add_Check_Duplicate_Slugs_Count_Appended()
        {
            var topicRepository = Substitute.For<ITopicRepository>();
            var postRepository = Substitute.For<IPostRepository>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();
            var topicService = new TopicService(membershipUserPointsService, settingsService, topicRepository, postRepository, _api, _topicNotificationService);
            const string newSlug = "topic-name-here";
            const string postContent = "Who would you be if you had a choice? Tony stark or Bruce Banner?";
            var multipleSlugs = new List<Topic>
                                    {
                                        new Topic {Slug = "topic-name-here-2"},
                                        new Topic {Slug = "topic-name-here-1"},
                                        new Topic {Slug = newSlug}
                                    };
            topicRepository.GetTopicBySlugLike("something").Returns(multipleSlugs);
            var topic = new Topic{Name = "something"};

            topicService.Add(topic);
            topicService.AddLastPost(topic, postContent);

            Assert.IsTrue(topic.Slug.EndsWith("3"));
            //Assert.IsTrue(topic.Tags[0].Tag == tag);
            //topicRepository.Received().Add(Arg.Is<TopicTag>(x => x.Tag == tag));
        }

        [Test]
        public void Delete_Check_Tags_Are_Cleared()
        {
            var topicRepository = Substitute.For<ITopicRepository>();
            var postRepository = Substitute.For<IPostRepository>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();

            var topicService = new TopicService(membershipUserPointsService, settingsService, topicRepository, postRepository, _api, _topicNotificationService);

            var topic = new Topic 
                { 
                    Name = "something",
                    Tags = new List<TopicTag>
                               {
                                   new TopicTag{Tag = "tagone"},
                                   new TopicTag{Tag = "tagtwo"}
                               }
                };          

            topicService.Delete(topic);

            Assert.IsTrue(!topic.Tags.Any());
        }

        [Test]
        public void SolveTopic()
        {
            var topicRepository = Substitute.For<ITopicRepository>();
            var postRepository = Substitute.For<IPostRepository>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();
            settingsService.GetSettings().Returns(new Settings { PointsAddedForSolution = 20 });
            var topicService = new TopicService(membershipUserPointsService, settingsService, topicRepository, postRepository, _api, _topicNotificationService);
            var marker = new MembershipUser
                             {
                                 UserName = "SpongeBob", Id = Guid.NewGuid()
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

            var post = new Post() {PostContent = "Test content"};
            
            var solutionWriter = new MembershipUser {UserName = "Squidward"};

            topicService.SolveTopic(topic, post, marker, solutionWriter);

            Assert.IsTrue(topic.Solved);
        }

        [Test]
        public void SolveTopicMarkerNotOwner()
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

            var owner = new MembershipUser
            {
                UserName = "Patrick",
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
                User = owner
            };

            var post = new Post() { PostContent = "Test content" };

            var solutionWriter = new MembershipUser { UserName = "Squidward" };

            topicService.SolveTopic(topic, post, marker, solutionWriter);

            Assert.IsFalse(topic.Solved);
        }
    }
}
