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
        private readonly ILocalizationService _localizationService;
        private readonly IPostEditService _postEditService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        private readonly ISettingsService _settingsService;

        public PostCreateEditPipe(ILocalizationService localizationService, IPostEditService postEditService, IMembershipUserPointsService membershipUserPointsService, ISettingsService settingsService)
        {
            _localizationService = localizationService;
            _postEditService = postEditService;
            _membershipUserPointsService = membershipUserPointsService;
            _settingsService = settingsService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            // Get the Current user from ExtendedData
            var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;
            var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);

            // Now save
            var saved = await context.SaveChangesAsync();
            if (saved <= 0)
            {
                input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
                return input;
            }


            // Is this an edit? If so, create a post edit
            var isEdit = input.ExtendedData[Constants.ExtendedDataKeys.IsEdit] as bool? == true;
            if (isEdit)
            {
                // Get the original post
                var originalPost = await context.Post.FirstOrDefaultAsync(x => x.Id == input.EntityToProcess.Id);

                // This is an edit of a post
                input.EntityToProcess.DateEdited = DateTime.UtcNow;

                // Grab the original name out the extended data
                var originalTopicName = input.ExtendedData[Constants.ExtendedDataKeys.Name] as string;

                // Create a post edit
                var postEdit = new PostEdit
                {
                    Post = input.EntityToProcess,
                    DateEdited = input.EntityToProcess.DateEdited,
                    EditedBy = loggedOnUser,
                    OriginalPostContent = originalPost.PostContent,
                    OriginalPostTitle = originalPost.IsTopicStarter ? originalTopicName : string.Empty
                };

                // Add the post edit too
                _postEditService.Add(postEdit);
            }


            // Update the users points score and post count for posting a new post
            if (!isEdit)
            {
                _membershipUserPointsService.Add(new MembershipUserPoints
                {
                    Points = _settingsService.GetSettings().PointsAddedPerPost,
                    User = input.EntityToProcess.User,
                    PointsFor = PointsFor.Post,
                    PointsForId = input.EntityToProcess.Id
                });
            }

            // Now do a final save
            saved = await context.SaveChangesAsync();
            if (saved <= 0)
            {
                input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
                return input;
            }

            input.ProcessLog.Add("Post created successfully");
            return input;
        }
    }
}