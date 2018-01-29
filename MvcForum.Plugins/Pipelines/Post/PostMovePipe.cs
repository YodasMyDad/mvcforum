namespace MvcForum.Plugins.Pipelines.Post
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class PostMovePipe : IPipe<IPipelineProcess<Post>>
    {
        private readonly IRoleService _roleService;
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;

        public PostMovePipe(IRoleService roleService, ICategoryService categoryService, ILocalizationService localizationService, ITopicService topicService, IPostService postService)
        {
            _roleService = roleService;
            _categoryService = categoryService;
            _localizationService = localizationService;
            _topicService = topicService;
            _postService = postService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            var newTopicTitle = string.Empty;
            if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.Name))
            {
                newTopicTitle = input.ExtendedData[Constants.ExtendedDataKeys.Name] as string;
            }

            Guid? newTopicId = null;
            if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.TopicId))
            {
                newTopicId = input.ExtendedData[Constants.ExtendedDataKeys.TopicId] as Guid?;
            }

            var moveReplayPosts = input.ExtendedData[Constants.ExtendedDataKeys.MovePosts] as bool?;

            var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;

            var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);
            var loggedOnUsersRole = loggedOnUser.GetRole(_roleService);
            var permissions = _roleService.GetPermissions(input.EntityToProcess.Topic.Category, loggedOnUsersRole);
            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);

            var previousTopic = input.EntityToProcess.Topic;
            var category = input.EntityToProcess.Topic.Category;
            var postCreator = input.EntityToProcess.User;

            // Hold the topic
            Topic topic = null;

            // If the dropdown has a value, then we choose that first
            if (newTopicId != null)
            {
                // Get the selected topic
                topic = _topicService.Get(newTopicId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(newTopicTitle))
            {
                // TODO - If we get here, we should really use the topic create pipeline!!

                //// We get the banned words here and pass them in, so its just one call
                //// instead of calling it several times and each call getting all the words back
                //var bannedWordsList = _bannedWordService.GetAll();
                //List<string> bannedWords = null;
                //if (bannedWordsList.Any())
                //{
                //    bannedWords = bannedWordsList.Select(x => x.Word).ToList();
                //}

                //// Create the topic
                //topic = new Topic
                //{
                //    Name = _bannedWordService.SanitiseBannedWords(viewModel.TopicTitle, bannedWords),
                //    Category = category,
                //    User = postCreator
                //};

                //// Create the topic
                //topic = _topicService.Add(topic);

                //// Save the changes
                //await context.SaveChangesAsync();

                //// Set the post to be a topic starter
                //input.EntityToProcess.IsTopicStarter = true;
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

            await context.SaveChangesAsync();

            // Update Last post..  As we have done a save, we should get all posts including the added ones
            var lastPost = topic.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();
            topic.LastPost = lastPost;

            // If any of the posts we are moving, were the last post - We need to update the old Topic
            var previousTopicLastPost =
                previousTopic.Posts.OrderByDescending(x => x.DateCreated).FirstOrDefault();
            previousTopic.LastPost = previousTopicLastPost;

            await context.SaveChangesAsync();

            return input;
        }
    }
}