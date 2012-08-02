using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Services;
using NSubstitute;
using NUnit.Framework;

namespace MVCForum.Tests.Service_Tests
{
    [TestFixture]
    public class TopicTagServiceTests
    {

        [Test]
        public void Add_Test()
        {
            const string tag = "testtag";
            var tagRepository = Substitute.For<ITopicTagRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var topicTagService = new TopicTagService(tagRepository, topicRepository);

            tagRepository.GetTagName(tag).Returns(x => null);

            var topic = new Topic();

            topicTagService.Add(tag, topic);

            Assert.IsTrue(topic.Tags.Count() == 1);
            Assert.IsTrue(topic.Tags[0].Tag == tag);
            tagRepository.Received().Add(Arg.Is<TopicTag>(x => x.Tag == tag));
        }

        [Test]
        public void Add_Test_With_Existing_Tags()
        {
            const string testtag = "testtag";
            const string testtagtwo = "testtagtwo";
            const string andthree = "andthree";
            var tagRepository = Substitute.For<ITopicTagRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var topicTagService = new TopicTagService(tagRepository, topicRepository);

            tagRepository.GetTagName(testtag).Returns(x => new TopicTag { Tag = testtag });
            tagRepository.GetTagName(testtagtwo).Returns(x => null);
            tagRepository.GetTagName(andthree).Returns(x => null);

            var topic = new Topic();

            topicTagService.Add(string.Concat(testtag, " , ", testtagtwo, " , ", andthree), topic);

            Assert.IsTrue(topic.Tags.Count() == 3);
            //Assert.IsTrue(topic.Tags[0].Tag == tag);
            tagRepository.DidNotReceive().Add(Arg.Is<TopicTag>(x => x.Tag == testtag));
            tagRepository.Received().Add(Arg.Is<TopicTag>(x => x.Tag == testtagtwo));
            tagRepository.Received().Add(Arg.Is<TopicTag>(x => x.Tag == andthree));
        }

        [Test]
        public void Add_Test_With_No_Existing_Tags()
        {
            const string testtag = "testtag";
            const string testtagtwo = "testtagtwo";
            const string andthree = "andthree";
            var tagRepository = Substitute.For<ITopicTagRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var topicTagService = new TopicTagService(tagRepository, topicRepository);

            tagRepository.GetTagName(testtag).Returns(x => new TopicTag { Tag = testtag });
            tagRepository.GetTagName(testtagtwo).Returns(x => new TopicTag { Tag = testtagtwo });
            tagRepository.GetTagName(andthree).Returns(x => new TopicTag { Tag = andthree });

            var topic = new Topic();

            topicTagService.Add(string.Concat(testtag, " , ", testtagtwo, " , ", andthree), topic);

            Assert.IsTrue(topic.Tags.Count() == 3);
            tagRepository.DidNotReceive().Add(Arg.Is<TopicTag>(x => x.Tag == testtag));
            tagRepository.DidNotReceive().Add(Arg.Is<TopicTag>(x => x.Tag == testtagtwo));
            tagRepository.DidNotReceive().Add(Arg.Is<TopicTag>(x => x.Tag == andthree));
        }

        [Test]
        public void Add_Test_With_Null_Tags()
        {
            var tagRepository = Substitute.For<ITopicTagRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var topicTagService = new TopicTagService(tagRepository, topicRepository);

            var topic = new Topic{Tags = new List<TopicTag>()};
            var topicTag = new TopicTag();

            topicTagService.Add(string.Empty, topic);

            Assert.IsTrue(!topic.Tags.Any());
            tagRepository.DidNotReceive().Update(Arg.Is(topicTag));
        }

        [Test]
        public void Delete_By_Topic_With_Multiple_Topics_Per_Tag()
        {
            var tagRepository = Substitute.For<ITopicTagRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var topicTagService = new TopicTagService(tagRepository, topicRepository);

            var topicTag = new TopicTag
                               {
                                   Tag = "tag-one",
                                   Topics = new List<Topic> {new Topic {Name = "Tony"},new Topic {Name = "Stark"}}
                               };
            var topic = new Topic
                            {
                                Tags = new List<TopicTag>
                                           {
                                               topicTag
                                           }
                            };

            topicTagService.DeleteByTopic(topic);

            Assert.IsFalse(topicTag.Topics.Count() <= 1);
            tagRepository.DidNotReceive().Delete(Arg.Is(topicTag));
        }

        [Test]
        public void Delete_By_Topic_With_One_Topics_Per_Tag()
        {
            var tagRepository = Substitute.For<ITopicTagRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var topicTagService = new TopicTagService(tagRepository, topicRepository);

            var topicTag = new TopicTag
            {
                Tag = "tag-one",
                Topics = new List<Topic> { new Topic { Name = "HulkRules" } }
            };
            var topic = new Topic
            {
                Tags = new List<TopicTag>
                                           {
                                               topicTag
                                           }
            };

            topicTagService.DeleteByTopic(topic);

            Assert.IsTrue(topicTag.Topics.Count() <= 1);
            tagRepository.Received().Delete(Arg.Is(topicTag));
        }

        [Test]
        public void DeleteTags_With_EmptyTags()
        {
            var tagRepository = Substitute.For<ITopicTagRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var topicTagService = new TopicTagService(tagRepository, topicRepository);

            var topicTags = new List<TopicTag>();

            topicTagService.DeleteTags(topicTags);

            Assert.IsTrue(!topicTags.Any());
            tagRepository.DidNotReceiveWithAnyArgs().Delete(Arg.Is<TopicTag>(x => x.Tag == null));
        }

        [Test]
        public void UpdateTagNames_With_Non_Existing_Tag()
        {
            var tagRepository = Substitute.For<ITopicTagRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var topicTagService = new TopicTagService(tagRepository, topicRepository);
            var oldName = "bilbo";
            var newName = "baggins";
            tagRepository.GetTagName(oldName).Returns(x => null);

            topicTagService.UpdateTagNames(newName, oldName);

            tagRepository.DidNotReceiveWithAnyArgs().Update(Arg.Is<TopicTag>(x => x.Tag == null));
        }


        [Test]
        //[ExpectedException]
        public void UpdateTagNames_With_Empty_Tags()
        {
            var tagRepository = Substitute.For<ITopicTagRepository>();
            var topicRepository = Substitute.For<ITopicRepository>();
            var topicTagService = new TopicTagService(tagRepository, topicRepository);
            const string oldName = "";
            const string newName = "";
            tagRepository.GetTagName(oldName).Returns(x => null);

            topicTagService.UpdateTagNames(newName, oldName);

            tagRepository.DidNotReceiveWithAnyArgs().Update(Arg.Is<TopicTag>(x => x.Tag == null));
        }
    }
}
