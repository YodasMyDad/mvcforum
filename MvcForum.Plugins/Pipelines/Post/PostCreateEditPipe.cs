namespace MvcForum.Plugins.Pipelines.Post
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.Enums;

    public class PostCreateEditPipe : IPipe<IPipelineProcess<Post>>
    {
        private readonly IPostEditService _postEditService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ISettingsService _settingsService;
        private readonly IActivityService _activityService;
        private readonly INotificationService _notificationService;
        private readonly ILoggingService _loggingService;

        public PostCreateEditPipe(IPostEditService postEditService, IMembershipUserPointsService membershipUserPointsService, ISettingsService settingsService, IActivityService activityService, INotificationService notificationService, ILoggingService loggingService)
        {
            _postEditService = postEditService;
            _membershipUserPointsService = membershipUserPointsService;
            _settingsService = settingsService;
            _activityService = activityService;
            _notificationService = notificationService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            // Refresh contexts for all services
            _postEditService.RefreshContext(context);
            _membershipUserPointsService.RefreshContext(context);
            _settingsService.RefreshContext(context);
            _activityService.RefreshContext(context);
            _notificationService.RefreshContext(context);

            try
            {
                // Get the Current user from ExtendedData
                var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;
                var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);

                // Is this an edit? If so, create a post edit
                var isEdit = false;
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.IsEdit))
                {
                    isEdit = input.ExtendedData[Constants.ExtendedDataKeys.IsEdit] as bool? == true;
                }

                if (isEdit)
                {
                    // Get the original post
                    var originalPost = await context.Post.Include(x => x.Topic).FirstOrDefaultAsync(x => x.Id == input.EntityToProcess.Id);

                    // Get content from Extended data
                    var postedContent = input.ExtendedData[Constants.ExtendedDataKeys.Content] as string;
                    input.EntityToProcess.PostContent = postedContent;

                    // This is an edit of a post
                    input.EntityToProcess.DateEdited = DateTime.UtcNow;

                    // Grab the original name out the extended data
                    var topicName = input.ExtendedData[Constants.ExtendedDataKeys.Name] as string;
                    if (!originalPost.PostContent.Equals(postedContent, StringComparison.OrdinalIgnoreCase) || !originalPost.Topic.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Create a post edit
                        var postEdit = new PostEdit
                        {
                            Post = input.EntityToProcess,
                            DateEdited = input.EntityToProcess.DateEdited,
                            EditedBy = loggedOnUser,
                            OriginalPostContent = originalPost.PostContent,
                            OriginalPostTitle = originalPost.IsTopicStarter ? originalPost.Topic.Name : string.Empty
                        };

                        // Add the post edit too
                        _postEditService.Add(postEdit);
                    }
                }
                else
                {
                    // Add the post
                    context.Post.Add(input.EntityToProcess);
                }

                // Now do a save
                await context.SaveChangesAsync();

                // Update the users points score and post count for posting a new post
                if (!isEdit)
                {
                    // make it last post if this is a new post
                    input.EntityToProcess.Topic.LastPost = input.EntityToProcess;

                    await _membershipUserPointsService.Add(new MembershipUserPoints
                    {
                        Points = _settingsService.GetSettings().PointsAddedPerPost,
                        User = input.EntityToProcess.User,
                        PointsFor = PointsFor.Post,
                        PointsForId = input.EntityToProcess.Id
                    });

                    // Add post activity if it's not an edit, or topic starter and it's not pending
                    if (input.EntityToProcess.IsTopicStarter == false && input.EntityToProcess.Pending != true)
                    {
                        _activityService.PostCreated(input.EntityToProcess);
                    }
                }


                if (input.EntityToProcess.IsTopicStarter == false && input.EntityToProcess.Pending != true)
                {
                    // Send notifications
                    _notificationService.Notify(input.EntityToProcess.Topic, loggedOnUser, NotificationType.Post);
                }

                // Now do a final save
                await context.SaveChangesAsync();

                input.ProcessLog.Add("Post created successfully");
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