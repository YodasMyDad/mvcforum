using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.API;

namespace MVCForum.API.Examples
{
    public class APIExamples
    {
        private IMVCForumAPI _api;

        public APIExamples(IMVCForumAPI api)
        {
            _api = api;
        }
        
        public IList<Topic> GetTopicsByCategory(Guid categoryId)
        {
            return _api.Topic.GetTopicsByCatId(categoryId);
        }

        public IList<Topic> GetSolvedTopics(MembershipUser user)
        {
            return _api.Topic.GetSolvedTopicsByMember(user.Id);
        }

        public IList<Topic> GetTopics(MembershipUser user)
        {
            return _api.Topic.GetTopicsByMember(user.Id);
        }

        public Topic GetTopic()
        {
            var slug = "yet-another-admin-discussion";
            return _api.Topic.GetTopicBySlug(slug);
        }

        public PagedList<Topic> GetPagedTopics()
        {
            // Start at page 2, 20 per page, get 100 topics
            return _api.Topic.GetAllTopicsPaged(2, 20, 100);
        }

        public void CreateTopic(MembershipUser user)
        {
            var category = _api.Category.GetBySlug("development");
            var newTopic = new Topic {User = user, Category = category, CreateDate = DateTime.Now};
            _api.Topic.Create(newTopic);
        }

        public Topic GetTopicById(Guid id)
        {
            return _api.Topic.Get(id);
        }

        public void DeleteTopic(Topic topic)
        {
            _api.Topic.Delete(topic);
        }

        public void DeletePost(Post post)
        {
            _api.Post.Delete(post);
        }

        public IList<Post> GetPosts(MembershipUser user)
        {
            return _api.Post.GetPostsByMember(user.Id);
        }

        public IList<Post> GetPostsForTopic(Topic topic)
        {
            return _api.Post.GetPostsByTopicId(topic.Id);
        }

        public MembershipUser Create(string name)
        {
            var newUser = new MembershipUser
                              {
                                  UserName = name
                              };

            // Id is created by API
            return _api.Member.Create(newUser);
        }

        public void DeleteMember(MembershipUser user)
        {
            _api.Member.Delete(user);
        }
    }
}
