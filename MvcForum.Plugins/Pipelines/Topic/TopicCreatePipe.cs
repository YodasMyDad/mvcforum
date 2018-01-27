namespace MvcForum.Plugins.Pipelines.Topic
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class TopicCreatePipe : IPipe<IPipelineProcess<Topic>>
    {
        private readonly IPostService _postService;
        private readonly ILocalizationService _localizationService;

        public TopicCreatePipe(IPostService postService, ILocalizationService localizationService)
        {
            _postService = postService;
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            // Create the post
            var files = input.ExtendedData[Constants.ExtendedDataKeys.PostedFiles] as HttpPostedFileBase[];

            var postPipelineResult = await _postService.Create(
                input.ExtendedData[Constants.ExtendedDataKeys.Content] as string,
                input.EntityToProcess, input.EntityToProcess.User, files, true);

            if (!postPipelineResult.Successful)
            {
                input.AddError(postPipelineResult.ProcessLog.FirstOrDefault());
                return input;
            }

            // make it last post
            input.EntityToProcess.LastPost = postPipelineResult.EntityToProcess;

            if (await context.SaveChangesAsync() <= 0)
            {
                // Problem
                input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
            }

            return input;
        }
    }
}