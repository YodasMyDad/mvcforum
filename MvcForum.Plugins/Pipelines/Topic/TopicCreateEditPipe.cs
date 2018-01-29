namespace MvcForum.Plugins.Pipelines.Topic
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class TopicCreateEditPipe : IPipe<IPipelineProcess<Topic>>
    {
        private readonly IPostService _postService;
        private readonly IPollService _pollService;
        private readonly ILocalizationService _localizationService;

        public TopicCreateEditPipe(IPostService postService, ILocalizationService localizationService, IPollService pollService)
        {
            _postService = postService;
            _localizationService = localizationService;
            _pollService = pollService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            // Create the post
            HttpPostedFileBase[] files = null;
            if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.PostedFiles))
            {
                files = input.ExtendedData[Constants.ExtendedDataKeys.PostedFiles] as HttpPostedFileBase[];
            }            

            // Are we in an edit mode
            var isEdit = input.ExtendedData[Constants.ExtendedDataKeys.IsEdit] as bool? == true;

            // Get the correct pipeline
            IPipelineProcess<Post> postPipelineResult;
            if (isEdit)
            {
                // Get the topic starter post
                var post = input.EntityToProcess.Posts.FirstOrDefault(x => x.IsTopicStarter);

                // Pass to edit
                postPipelineResult = await _postService.Edit(post, files, true, input.ExtendedData[Constants.ExtendedDataKeys.Name] as string);
            }
            else
            {
                postPipelineResult = await _postService.Create(
                    input.ExtendedData[Constants.ExtendedDataKeys.Content] as string,
                    input.EntityToProcess, input.EntityToProcess.User, files, true);
            }

            if (!postPipelineResult.Successful)
            {
                input.AddError(postPipelineResult.ProcessLog.FirstOrDefault());
                return input;
            }

            // make it last post
            input.EntityToProcess.LastPost = postPipelineResult.EntityToProcess;

            // Was the post successful
            if (await context.SaveChangesAsync() <= 0)
            {
                // Problem
                input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
            }

            // Sort the poll, if this is an edit we need to refresh and save the poll data
            if (isEdit)
            {
                var newPollAnswers = input.ExtendedData[Constants.ExtendedDataKeys.PollNewAnswers] as List<PollAnswer>;
                var pollCloseafterDays = input.ExtendedData[Constants.ExtendedDataKeys.PollCloseAfterDays] as int?;
                _pollService.RefreshEditedPoll(input.EntityToProcess, newPollAnswers, pollCloseafterDays ?? 0);
            }
            

            return input;
        }
    }
}