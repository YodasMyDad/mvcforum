using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.General;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class TopicService : ITopicService
    {
        private readonly ITopicRepository _topicRepository;
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly IPostService _postService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ISettingsService _settingsService;

        public TopicService(IMembershipUserPointsService membershipUserPointsService,
            ISettingsService settingsService, ITopicRepository topicRepository, ITopicNotificationService topicNotificationService, IPostService postService)
        {
            _membershipUserPointsService = membershipUserPointsService;
            _settingsService = settingsService;
            _topicRepository = topicRepository;
            _topicNotificationService = topicNotificationService;
            _postService = postService;
        }

        public Topic SanitizeTopic(Topic topic)
        {
            topic.Name = StringUtils.SafePlainText(topic.Name);
            return topic;
        }

        /// <summary>
        /// Get all topics
        /// </summary>
        /// <returns></returns>
        public IList<Topic> GetAll(List<Category> allowedCategories)
        {
            return _topicRepository.GetAll(allowedCategories);
        }

        public IList<Topic> GetHighestViewedTopics(int amountToTake, List<Category> allowedCategories)
        {
            return _topicRepository.GetHighestViewedTopics(amountToTake, allowedCategories);
        }

        public IList<Topic> GetPopularTopics(DateTime? from, DateTime? to, List<Category> allowedCategories,int amountToShow = 20)
        {
            if (from == null)
            {
                from = DateTime.UtcNow.AddDays(-14);
            }

            if (to == null)
            {
                to = DateTime.UtcNow;
            }

            return _topicRepository.GetPopularTopics((DateTime)from, (DateTime)to, amountToShow, allowedCategories);
        }

        /// <summary>
        /// Create a new topic and also the topic starter post
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public Topic Add(Topic topic)
        {
            topic = SanitizeTopic(topic);

            topic.CreateDate = DateTime.UtcNow;

            // url slug generator
            topic.Slug = ServiceHelpers.GenerateSlug(topic.Name, _topicRepository.GetTopicBySlugLike(ServiceHelpers.CreateUrl(topic.Name)), null);
            
            return _topicRepository.Add(topic);
        }

        /// <summary>
        /// Get todays topics
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Topic> GetTodaysTopics(int amountToTake, List<Category> allowedCategories)
        {
            return _topicRepository.GetTodaysTopics(amountToTake, allowedCategories);
        }

        /// <summary>
        /// Add a last post to a topic. Must be part of a separate database update
        /// in EF because of circular dependencies. So save the topic before calling this.
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="postContent"></param>
        /// <returns></returns>
        public Post AddLastPost(Topic topic, string postContent)
        {

            topic = SanitizeTopic(topic);

            // Create the post
            var post = new Post
            {
                DateCreated = DateTime.UtcNow,
                IsTopicStarter = true,
                DateEdited = DateTime.UtcNow,
                PostContent = postContent,
                User = topic.User,
                Topic = topic
            };

            // Add the post
            _postService.Add(post);

            topic.LastPost = post;

            return post;
        }

        public List<MarkAsSolutionReminder> GetMarkAsSolutionReminderList(int days)
        {
            return _topicRepository.GetMarkAsSolutionReminderList(days);
        }

        /// <summary>
        /// Returns a paged list of topics, ordered by most recent
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public PagedList<Topic> GetRecentTopics(int pageIndex, int pageSize, int amountToTake, List<Category> allowedCategories)
        {
            return _topicRepository.GetRecentTopics(pageIndex, pageSize, amountToTake, allowedCategories);
        }

        /// <summary>
        /// Returns a specified amount of most recent topics in a list used for RSS feeds
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Topic> GetRecentRssTopics(int amountToTake, List<Category> allowedCategories)
        {
            return _topicRepository.GetRecentRssTopics(amountToTake, allowedCategories);
        }

        /// <summary>
        /// Returns all topics by a specified user
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Topic> GetTopicsByUser(Guid memberId, List<Category> allowedCategories)
        {
            return _topicRepository.GetTopicsByUser(memberId, allowedCategories);
        }

        public IList<Topic> GetTopicsByLastPost(List<Guid> postIds, List<Category> allowedCategories)
        {
            return _topicRepository.GetTopicsByLastPost(postIds, allowedCategories);
        }

        /// <summary>
        /// Returns a paged list of topics from a specified category
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public PagedList<Topic> GetPagedTopicsByCategory(int pageIndex, int pageSize, int amountToTake, Guid categoryId)
        {
            return _topicRepository.GetPagedTopicsByCategory(pageIndex, pageSize, amountToTake, categoryId);
        }

        /// <summary>
        /// Gets all the pending topics in a paged list
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public PagedList<Topic> GetPagedPendingTopics(int pageIndex, int pageSize, List<Category> allowedCategories)
        {
            return _topicRepository.GetPagedPendingTopics(pageIndex, pageSize, allowedCategories);
        }

        public int GetPendingTopicsCount(List<Category> allowedCategories)
        {
            return _topicRepository.GetPendingTopicsCount(allowedCategories);
        }

        /// <summary>
        /// Returns a specified amount of most recent topics in a category used for RSS feeds
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IList<Topic> GetRssTopicsByCategory(int amountToTake, Guid categoryId)
        {
            return _topicRepository.GetRssTopicsByCategory(amountToTake, categoryId);
        }

        /// <summary>
        /// Returns a paged amount of topics in a list filtered via tag
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="amountToTake"></param>
        /// <param name="tag"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public PagedList<Topic> GetPagedTopicsByTag(int pageIndex, int pageSize, int amountToTake, string tag, List<Category> allowedCategories)
        {
            return _topicRepository.GetPagedTopicsByTag(pageIndex, pageSize, amountToTake, StringUtils.GetSafeHtml(tag), allowedCategories);
        }

        /// <summary>
        /// Returns a paged amount of searched topics by a string search value
        /// </summary>
        /// <param name="amountToTake"></param>
        /// <param name="searchTerm"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Topic> SearchTopics(int amountToTake, string searchTerm, List<Category> allowedCategories)
        {
            // Create search term
            var search = StringUtils.ReturnSearchString(searchTerm);

            // Now split the words
            var splitSearch = search.Split(' ').ToList();

            // Pass the sanitised split words to the repo
            return _topicRepository.SearchTopics(amountToTake, splitSearch, allowedCategories);
        }

        public PagedList<Topic> GetTopicsByCsv(int pageIndex, int pageSize, int amountToTake, List<Guid> topicIds, List<Category> allowedCategories)
        {
            return _topicRepository.GetTopicsByCsv(pageIndex, pageSize, amountToTake, topicIds, allowedCategories);
        }

        public PagedList<Topic> GetMembersActivity(int pageIndex, int pageSize, int amountToTake, Guid memberGuid, List<Category> allowedCategories)
        {
            return _topicRepository.GetMembersActivity(pageIndex, pageSize, amountToTake, memberGuid, allowedCategories);
        }

        public IList<Topic> GetTopicsByCsv(int amountToTake, List<Guid> topicIds, List<Category> allowedCategories)
        {
            return _topicRepository.GetTopicsByCsv(amountToTake, topicIds, allowedCategories);
        }

        /// <summary>
        /// Return a topic by url slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public Topic GetTopicBySlug(string slug)
        {
            return _topicRepository.GetTopicBySlug(StringUtils.GetSafeHtml(slug));
        }

        /// <summary>
        /// Return a topic by Id
        /// </summary>
        /// <param name="topicId"></param>
        /// <returns></returns>
        public Topic Get(Guid topicId)
        {
            return _topicRepository.Get(topicId);
        }

        public List<Topic> Get(List<Guid> topicIds, List<Category> allowedCategories)
        {
            return _topicRepository.Get(topicIds, allowedCategories);
        }

        /// <summary>
        /// Delete a topic
        /// </summary>
        /// <param name="topic"></param>
        public void Delete(Topic topic)
        {
            topic.LastPost = null;
            topic.Tags.Clear();

            // Delete all posts
            if (topic.Posts != null)
            {
                var postsToDelete = new List<Post>();
                postsToDelete.AddRange(topic.Posts);
                foreach (var post in postsToDelete)
                {                    
                    // Delete the post
                    _postService.Delete(post, true);
                } 
                topic.Posts.Clear();
            }

            if (topic.TopicNotifications != null)
            {
                var notificationsToDelete = new List<TopicNotification>();
                notificationsToDelete.AddRange(topic.TopicNotifications);
                foreach (var topicNotification in notificationsToDelete)
                {
                    _topicNotificationService.Delete(topicNotification);
                } 
            }

            _topicRepository.Delete(topic);
        }

        public int TopicCount(List<Category> allowedCategories)
        {
            return _topicRepository.TopicCount(allowedCategories);
        }

        /// <summary>
        /// Return topics by a specified user that are marked as solved
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public IList<Topic> GetSolvedTopicsByMember(Guid memberId, List<Category> allowedCategories)
        {
            return _topicRepository.GetSolvedTopicsByMember(memberId, allowedCategories);
        }

        /// <summary>
        /// Mark a topic as solved
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="post"></param>
        /// <param name="marker"></param>
        /// <param name="solutionWriter"></param>
        /// <returns>True if topic has been marked as solved</returns>
        public bool SolveTopic(Topic topic, Post post, MembershipUser marker, MembershipUser solutionWriter)
        {
            var solved = false;

            var e = new MarkedAsSolutionEventArgs
                        {
                            Topic = topic,
                            Post = post,
                            Marker = marker,
                            SolutionWriter = solutionWriter
                        };
            EventManager.Instance.FireBeforeMarkedAsSolution(this, e);

            if (!e.Cancel)
            {
                // Make sure this user owns the topic, if not do nothing
                if (topic.User.Id == marker.Id)
                {
                    // Update the post
                    post.IsSolution = true;
                    //_postRepository.Update(post);

                    // Update the topic
                    topic.Solved = true;
                    //SaveOrUpdate(topic);

                    // Assign points
                    // Do not give points to the user if they are marking their own post as the solution
                    if (marker.Id != solutionWriter.Id)
                    {
                        _membershipUserPointsService.Add(new MembershipUserPoints
                                                             {
                                                                 Points = _settingsService.GetSettings().PointsAddedForSolution,
                                                                 User = solutionWriter,
                                                                 PointsFor = PointsFor.Solution,
                                                                 PointsForId = post.Id
                                                             });
                    }

                    EventManager.Instance.FireAfterMarkedAsSolution(this, new MarkedAsSolutionEventArgs
                                                                              {
                                                                                  Topic = topic,
                                                                                  Post = post,
                                                                                  Marker = marker,
                                                                                  SolutionWriter = solutionWriter
                                                                              });
                    solved = true;
                }
            }

            return solved;
        }
    }
}
