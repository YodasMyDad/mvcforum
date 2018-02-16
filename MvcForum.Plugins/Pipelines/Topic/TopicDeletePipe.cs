namespace MvcForum.Plugins.Pipelines.Topic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class TopicDeletePipe : IPipe<IPipelineProcess<Topic>>
    {
        private readonly IFavouriteService _favouriteService;
        private readonly IPollService _pollService;
        private readonly IPostService _postService;
        private readonly INotificationService _notificationService;
        private readonly ILoggingService _loggingService;
        private readonly ICacheService _cacheService;

        public TopicDeletePipe(IFavouriteService favouriteService, IPollService pollService, IPostService postService, INotificationService notificationService, ILoggingService loggingService, ICacheService cacheService)
        {
            _favouriteService = favouriteService;
            _pollService = pollService;
            _postService = postService;
            _notificationService = notificationService;
            _loggingService = loggingService;
            _cacheService = cacheService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            _favouriteService.RefreshContext(context);
            _pollService.RefreshContext(context);
            _postService.RefreshContext(context);
            _notificationService.RefreshContext(context);

            try
            {
                // Remove all notifications on this topic too
                if (input.EntityToProcess.TopicNotifications != null)
                {
                    var notificationsToDelete = new List<TopicNotification>();
                    notificationsToDelete.AddRange(input.EntityToProcess.TopicNotifications);
                    foreach (var topicNotification in notificationsToDelete)
                    {
                        input.EntityToProcess.TopicNotifications.Remove(topicNotification);
                        _notificationService.Delete(topicNotification);
                    }

                    // Final Clear
                    input.EntityToProcess.TopicNotifications.Clear();
                }

                // Remove all favourites on this topic too
                if (input.EntityToProcess.Favourites != null)
                {
                    var toDelete = new List<Favourite>();
                    toDelete.AddRange(input.EntityToProcess.Favourites);
                    foreach (var entity in toDelete)
                    {
                        input.EntityToProcess.Favourites.Remove(entity);
                        _favouriteService.Delete(entity);
                    }

                    // Final Clear
                    input.EntityToProcess.Favourites.Clear();
                }

                // Poll
                if (input.EntityToProcess.Poll != null)
                {
                    var pollToDelete = input.EntityToProcess.Poll;

                    // Final Clear
                    input.EntityToProcess.Poll = null;

                    // Delete the poll 
                    _pollService.Delete(pollToDelete);
                }

                // First thing - Set the last post as null and clear tags
                input.EntityToProcess.Tags.Clear();

                // Save here to clear the last post
                await context.SaveChangesAsync();

                // Loop through all the posts and clear the associated entities
                // then delete the posts
                if (input.EntityToProcess.Posts != null)
                {
                    var postsToDelete = new List<Post>();
                    postsToDelete.AddRange(input.EntityToProcess.Posts);

                    foreach (var post in postsToDelete)
                    {
                        // Posts should only be deleted from this method as it clears
                        // associated data
                        var postDeleteResult = await _postService.Delete(post, true);
                        if (!postDeleteResult.Successful)
                        {
                            input.AddError(postDeleteResult.ProcessLog.FirstOrDefault());
                            return input;
                        }
                    }

                    input.EntityToProcess.Posts.Clear();

                    // Clear last post
                    input.EntityToProcess.LastPost = null;
                }

                // Finally delete the topic
                context.Topic.Remove(input.EntityToProcess);

                // Save here to clear the last post
                await context.SaveChangesAsync();

                // Clear some caches
                _cacheService.ClearStartsWith("HotTopics");
            }
            catch (Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }

            return input;
        }
    }
}