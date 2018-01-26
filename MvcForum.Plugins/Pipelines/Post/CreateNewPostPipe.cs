namespace MvcForum.Plugins.Pipelines.Post
{
    using System.Threading.Tasks;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class CreateNewPostPipe : IPipe<IPipelineProcess<Post>>
    {
        private readonly ILocalizationService _localizationService;

        public CreateNewPostPipe(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            // This one is a bit pointless but it's here in case I need to do anything else
            var saved = await context.SaveChangesAsync();
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