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
        private readonly ILoggingService _loggingService;

        public TopicCreateEditPipe(IPostService postService, ILocalizationService localizationService, IPollService pollService, ILoggingService loggingService)
        {
            _postService = postService;
            _localizationService = localizationService;
            _pollService = pollService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            _postService.RefreshContext(context);
            _localizationService.RefreshContext(context);
            _pollService.RefreshContext(context);

            try
            {
                // Create the post
                HttpPostedFileBase[] files = null;
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.PostedFiles))
                {
                    files = input.ExtendedData[Constants.ExtendedDataKeys.PostedFiles] as HttpPostedFileBase[];
                }

                // Are we in an edit mode
                var isEdit = input.ExtendedData[Constants.ExtendedDataKeys.IsEdit] as bool? == true;

                // Add a variable for the post
                Post post = null;

                var isNew = false;

                // See if we have a post already (i.e. for when we move)
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.Post))
                {
                    // We have a post so set it
                    post = input.ExtendedData[Constants.ExtendedDataKeys.Post] as Post;
                }

                // If the post is not null and we are not editing
                if (post != null && !isEdit)
                {
                    // Set the topic
                    post.Topic = input.EntityToProcess;
                }
                else
                {
                    // Get the correct pipeline
                    IPipelineProcess<Post> postPipelineResult;

                    // Is this an edit
                    if (isEdit)
                    {
                        // Get the topic starter post
                        post = input.EntityToProcess.Posts.FirstOrDefault(x => x.IsTopicStarter);

                        // Pass to edit
                        postPipelineResult = await _postService.Edit(post, files, true,
                            input.ExtendedData[Constants.ExtendedDataKeys.Name] as string, input.ExtendedData[Constants.ExtendedDataKeys.Content] as string);
                    }
                    else
                    {
                        if (input.EntityToProcess.Poll != null)
                        {
                            context.Poll.Add(input.EntityToProcess.Poll);
                        }

                        // Add the topic to the context
                        context.Topic.Add(input.EntityToProcess);

                        // We are creating a new post
                        postPipelineResult = await _postService.Create(
                            input.ExtendedData[Constants.ExtendedDataKeys.Content] as string,
                            input.EntityToProcess, input.EntityToProcess.User, files, true, null);

                        // Set the new post flag
                        isNew = true;
                    }

                    // If there is an issue return the pipeline
                    if (!postPipelineResult.Successful)
                    {
                        input.AddError(postPipelineResult.ProcessLog.FirstOrDefault());
                        return input;
                    }

                    // Set the post as the post from the pipeline
                    post = postPipelineResult.EntityToProcess;

                    if (isNew)
                    {
                        // make it last post if this is a new post
                        input.EntityToProcess.LastPost = post;
                    }
                }

                // Do a final save
                await context.SaveChangesAsync();

                // Sort the poll, if this is an edit we need to refresh and save the poll data
                if (isEdit)
                {
                    var newPollAnswers = input.ExtendedData[Constants.ExtendedDataKeys.PollNewAnswers] as List<PollAnswer>;
                    var pollCloseafterDays = input.ExtendedData[Constants.ExtendedDataKeys.PollCloseAfterDays] as int?;
                    _pollService.RefreshEditedPoll(input.EntityToProcess, newPollAnswers, pollCloseafterDays ?? 0);
                }
            }
            catch (System.Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }
            
            return input;
        }
    }
}