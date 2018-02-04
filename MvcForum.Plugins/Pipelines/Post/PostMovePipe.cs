namespace MvcForum.Plugins.Pipelines.Post
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class PostMovePipe : IPipe<IPipelineProcess<Post>>
    {
        private readonly ILocalizationService _localizationService;
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;
        private readonly ILoggingService _loggingService;

        public PostMovePipe(ILocalizationService localizationService, ITopicService topicService, IPostService postService, ILoggingService loggingService)
        {
            _localizationService = localizationService;
            _topicService = topicService;
            _postService = postService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            _localizationService.RefreshContext(context);
            _topicService.RefreshContext(context);
            _postService.RefreshContext(context);
            try
            {

                // Do we have a topic title in the extended data
                var newTopicTitle = string.Empty;
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.Name))
                {
                    newTopicTitle = input.ExtendedData[Constants.ExtendedDataKeys.Name] as string;
                }

                // Do we have a topic id in the extended data
                Guid? newTopicId = null;
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.TopicId))
                {
                    newTopicId = input.ExtendedData[Constants.ExtendedDataKeys.TopicId] as Guid?;
                }

                // Flag whether we should also move any reply to posts
                var moveReplayPosts = input.ExtendedData[Constants.ExtendedDataKeys.MovePosts] as bool?;

                // Hold the previous topic
                var previousTopic = input.EntityToProcess.Topic;

                // Hold the previous category
                var category = input.EntityToProcess.Topic.Category;

                // Hold the post creator
                var postCreator = input.EntityToProcess.User;

                // Hold the topic
                Topic topic;

                // If the dropdown has a value, then we choose that first
                if (newTopicId != null)
                {
                    // Get the selected topic
                    topic = _topicService.Get(newTopicId.Value);
                }
                else if (!string.IsNullOrWhiteSpace(newTopicTitle))
                {
                    // If we get here, we use the topic create pipeline!!
                    // Create the topic
                    topic = new Topic
                    {
                        Name = newTopicTitle,
                        Category = category,
                        User = postCreator
                    };

                    // Run the create pipeline
                    var createPipeLine = await _topicService.Create(topic, null, string.Empty, true, input.EntityToProcess.PostContent, null);
                    if (createPipeLine.Successful == false)
                    {
                        // Tell the user the topic is awaiting moderation
                        input.AddError(createPipeLine.ProcessLog.FirstOrDefault());
                        return input;
                    }

                    // Set the post to be a topic starter
                    input.EntityToProcess.IsTopicStarter = true;

                    // Save the changes
                    await context.SaveChangesAsync();

                    // Set the topic
                    topic = createPipeLine.EntityToProcess;
                }
                else
                {
                    // No selected topic OR topic title, just redirect back to the topic
                    return input;
                }

                // Now update the post to the new topic
                input.EntityToProcess.Topic = topic;

                // Also move any posts, which were in reply to this post
                if (moveReplayPosts == true)
                {
                    var relatedPosts = _postService.GetReplyToPosts(input.EntityToProcess.Id);
                    foreach (var relatedPost in relatedPosts)
                    {
                        relatedPost.Topic = topic;
                    }
                }

                var saveChanges = await context.SaveChangesAsync();
                if (saveChanges <= 0)
                {
                    // Nothing was saved so throw error message
                    input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
                    return input;
                }

                // Update Last post..  As we have done a save, we should get all posts including the added ones
                var lastPost = topic.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();
                topic.LastPost = lastPost;

                // If any of the posts we are moving, were the last post - We need to update the old Topic
                var previousTopicLastPost =
                    previousTopic.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();
                previousTopic.LastPost = previousTopicLastPost;

                // Do a final save
                saveChanges = await context.SaveChangesAsync();
                if (saveChanges <= 0)
                {
                    // Nothing was saved so throw error message
                    input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
                    return input;
                }

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