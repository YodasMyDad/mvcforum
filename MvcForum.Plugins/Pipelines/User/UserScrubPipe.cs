using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcForum.Plugins.Pipelines.User
{
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Models.Entities;

    public class UserScrubPipe : IPipe<IPipelineProcess<MembershipUser>>
    {
        public UserScrubPipe()
        {
            
        }

        /// <inheritdoc />
        public Task<IPipelineProcess<MembershipUser>> Process(IPipelineProcess<MembershipUser> input, IMvcForumContext context)
        {
            // PROFILE
            input.EntityToProcess.Website = string.Empty;
            input.EntityToProcess.Twitter = string.Empty;
            input.EntityToProcess.Facebook = string.Empty;
            input.EntityToProcess.Avatar = string.Empty;
            input.EntityToProcess.Signature = string.Empty;

            //// User Votes
            if (input.EntityToProcess.Votes != null)
            {
                var votesToDelete = new List<Vote>();
                votesToDelete.AddRange(input.EntityToProcess.Votes);
                votesToDelete.AddRange(input.EntityToProcess.VotesGiven);
                foreach (var d in votesToDelete)
                {
                    _voteService.Delete(d);
                }
                input.EntityToProcess.Votes.Clear();
                input.EntityToProcess.VotesGiven.Clear();
                _context.SaveChanges();
            }

            // User badge time checks
            if (input.EntityToProcess.BadgeTypesTimeLastChecked != null)
            {
                var toDelete = new List<BadgeTypeTimeLastChecked>();
                toDelete.AddRange(input.EntityToProcess.BadgeTypesTimeLastChecked);
                foreach (var obj in toDelete)
                {
                    _badgeService.DeleteTimeLastChecked(obj);
                }
                input.EntityToProcess.BadgeTypesTimeLastChecked.Clear();
                _context.SaveChanges();
            }

            // User Badges
            if (input.EntityToProcess.Badges != null)
            {
                var toDelete = new List<Badge>();
                toDelete.AddRange(input.EntityToProcess.Badges);
                foreach (var obj in toDelete)
                {
                    _badgeService.Delete(obj);
                }
                input.EntityToProcess.Badges.Clear();
                _context.SaveChanges();
            }

            // User category notifications
            if (input.EntityToProcess.CategoryNotifications != null)
            {
                var toDelete = new List<CategoryNotification>();
                toDelete.AddRange(input.EntityToProcess.CategoryNotifications);
                foreach (var obj in toDelete)
                {
                    _notificationService.Delete(obj);
                }
                input.EntityToProcess.CategoryNotifications.Clear();
                _context.SaveChanges();
            }

            // User PM Received
            if (input.EntityToProcess.PrivateMessagesReceived != null)
            {
                var toDelete = new List<PrivateMessage>();
                toDelete.AddRange(input.EntityToProcess.PrivateMessagesReceived);
                foreach (var obj in toDelete)
                {
                    _privateMessageService.DeleteMessage(obj);
                }
                input.EntityToProcess.PrivateMessagesReceived.Clear();
                _context.SaveChanges();
            }

            // User PM Sent
            if (input.EntityToProcess.PrivateMessagesSent != null)
            {
                var toDelete = new List<PrivateMessage>();
                toDelete.AddRange(input.EntityToProcess.PrivateMessagesSent);
                foreach (var obj in toDelete)
                {
                    _privateMessageService.DeleteMessage(obj);
                }
                input.EntityToProcess.PrivateMessagesSent.Clear();
                _context.SaveChanges();
            }

            // User Favourites
            if (input.EntityToProcess.Favourites != null)
            {
                var toDelete = new List<Favourite>();
                toDelete.AddRange(input.EntityToProcess.Favourites);
                foreach (var obj in toDelete)
                {
                    _favouriteService.Delete(obj);
                }
                input.EntityToProcess.Favourites.Clear();
                _context.SaveChanges();
            }

            if (input.EntityToProcess.TopicNotifications != null)
            {
                var notificationsToDelete = new List<TopicNotification>();
                notificationsToDelete.AddRange(input.EntityToProcess.TopicNotifications);
                foreach (var topicNotification in notificationsToDelete)
                {
                    _notificationService.Delete(topicNotification);
                }
                input.EntityToProcess.TopicNotifications.Clear();
            }

            // Also clear their points
            var userPoints = input.EntityToProcess.Points;
            if (userPoints.Any())
            {
                var pointsList = new List<MembershipUserPoints>();
                pointsList.AddRange(userPoints);
                foreach (var point in pointsList)
                {
                    point.User = null;
                    _membershipUserPointsService.Delete(point);
                }
                input.EntityToProcess.Points.Clear();
            }

            // Now clear all activities for this user
            var usersActivities = _activityService.GetDataFieldByGuid(input.EntityToProcess.Id);
            _activityService.Delete(usersActivities.ToList());
            _context.SaveChanges();

            // Also clear their poll votes
            var userPollVotes = input.EntityToProcess.PollVotes;
            if (userPollVotes.Any())
            {
                var pollList = new List<PollVote>();
                pollList.AddRange(userPollVotes);
                foreach (var vote in pollList)
                {
                    vote.User = null;
                    _pollService.Delete(vote);
                }
                input.EntityToProcess.PollVotes.Clear();
                _context.SaveChanges();
            }

            //// ######### POSTS TOPICS ########

            // Delete all topics first
            // This will get rid of everyone elses posts associated with this users topic too
            var topics = input.EntityToProcess.Topics;
            if (topics != null && topics.Any())
            {
                var topicList = new List<Topic>();
                topicList.AddRange(topics);
                foreach (var topic in topicList)
                {
                    // TODO - This is a pipeline!
                    _topicService.Delete(topic);
                }
                input.EntityToProcess.Topics.Clear();
                _context.SaveChanges();
            }

            // Now sorts Last Posts on topics and delete all the users posts
            var posts = input.EntityToProcess.Posts;
            if (posts != null && posts.Any())
            {
                var postIds = posts.Select(x => x.Id).ToList();

                // Get all categories
                var allCategories = _categoryService.GetAll();

                // Need to see if any of these are last posts on Topics
                // If so, need to swap out last post
                var lastPostTopics = _topicService.GetTopicsByLastPost(postIds, allCategories.ToList());
                foreach (var topic in lastPostTopics.Where(x => x.User.Id != input.EntityToProcess.Id))
                {
                    var lastPost = topic.Posts.Where(x => !postIds.Contains(x.Id)).OrderByDescending(x => x.DateCreated)
                        .FirstOrDefault();
                    topic.LastPost = lastPost;
                }

                _context.SaveChanges();

                // Delete all posts
                var postList = new List<Post>();
                postList.AddRange(posts);
                foreach (var post in postList)
                {
                    // TODO - This is a pipeline
                    _postService.Delete(post, true);
                }

                input.EntityToProcess.UploadedFiles.Clear();
                input.EntityToProcess.Posts.Clear();

                _context.SaveChanges();
            }
        }
    }
}
