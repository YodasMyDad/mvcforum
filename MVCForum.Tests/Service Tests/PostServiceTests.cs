using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.Constants;
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
    public class PostServiceTests
    {
        private IMVCForumAPI _api;

        [SetUp]
        public void Init()
        {
            _api = Substitute.For<IMVCForumAPI>();
        }

        [Test]
        public void Delete_Topic_Deleted_If_Topic_Starter()
        {
            var postRepository = Substitute.For<IPostRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var roleService = Substitute.For<IRoleService>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var postService = new PostService(membershipUserPointsService, settingsService, roleService, postRepository, topicRepository, localisationService, _api);

            var tags = new List<TopicTag>
                           {
                               new TopicTag{Tag = "Thor"},
                               new TopicTag{Tag = "Gambit"}
                           };
            var topic = new Topic { Name = "Captain America", Tags = tags };
            var post = new Post { Id = Guid.NewGuid(), Topic = topic, IsTopicStarter = true};
            topic.LastPost = post;
            topic.Posts = new List<Post>();
            post.IsTopicStarter = true;
            topic.Posts.Add(post);

            // Save
            var deleteTopic = postService.Delete(post);

            Assert.IsTrue(deleteTopic);
        }

        [Test]
        public void Delete_Post_Removed_From_TopicList_And_Last_Post()
        {
            var postRepository = Substitute.For<IPostRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var roleService = Substitute.For<IRoleService>();
            var membershipUserPointsService = Substitute.For<IMembershipUserPointsService>();
            var settingsService = Substitute.For<ISettingsService>();
            var localisationService = Substitute.For<ILocalizationService>();
            var postService = new PostService(membershipUserPointsService, settingsService, roleService, postRepository, topicRepository, localisationService, _api);
            
            // Create topic
            var topic = new Topic { Name = "Captain America"};

            // Create some posts with one to delete
            var postToDelete = new Post { Id = Guid.NewGuid(), Topic = topic, IsTopicStarter = false, DateCreated = DateTime.Now};
            var postToStay = new Post { Id = Guid.NewGuid(), Topic = topic, IsTopicStarter = false, DateCreated = DateTime.Now.Subtract(TimeSpan.FromDays(2)) };
            var postToStayTwo = new Post { Id = Guid.NewGuid(), Topic = topic, IsTopicStarter = false, DateCreated = DateTime.Now.Subtract(TimeSpan.FromDays(3)) };

            // set last post
            topic.LastPost = postToDelete;

            // Set the post list to the topic
            var posts = new List<Post> {postToDelete, postToStay, postToStayTwo};
            topic.Posts = posts;

            // Save
            postService.Delete(postToDelete);

            // Test that settings is no longer in cache
            Assert.IsTrue(topic.LastPost == postToStay);
            Assert.IsTrue(topic.Posts.Count == 2);         
        }

        [Test]
        public void AddPost()
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
            var role = new MembershipRole{RoleName = "TestRole"};
        
            var categoryPermissionForRoleSet = new List<CategoryPermissionForRole>
                                                   {
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionEditPosts }, IsTicked = true},
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionDenyAccess }, IsTicked = false},
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionReadOnly  }, IsTicked = false}
                                                   };
            
            var permissionSet = new PermissionSet(categoryPermissionForRoleSet);
            roleService.GetPermissions(category, role).Returns(permissionSet);

            var topic = new Topic { Name = "Captain America", Category = category};
            var user = new MembershipUser {
                UserName = "SpongeBob", 
                Roles = new List<MembershipRole>{role}
            };

            var newPost = postService.AddNewPost("A test post", topic, user, out permissionSet);

            Assert.AreEqual(newPost.User, user);
            Assert.AreEqual(newPost.Topic, topic);
        }

        [Test]
        public void AddPostNoPermission()
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
                                                       new CategoryPermissionForRole { Permission = new Permission { Name = AppConstants.PermissionDenyAccess }, IsTicked = true},
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

            try
            {
                var newPost = postService.AddNewPost("A test post", topic, user, out permissionSet);
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, "No permission");
            }
          
        }
    }
}
