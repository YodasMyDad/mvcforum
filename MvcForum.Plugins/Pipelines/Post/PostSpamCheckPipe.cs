namespace MvcForum.Plugins.Pipelines.Post
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class PostSpamCheckPipe : IPipe<IPipelineProcess<Post>>
    {
        private readonly IBannedWordService _bannedWordService;
        private readonly ILocalizationService _localizationService;
        private readonly ISpamService _spamService;
        private readonly ILoggingService _loggingService;

        public PostSpamCheckPipe(IBannedWordService bannedWordService, ILocalizationService localizationService, ISpamService spamService, ILoggingService loggingService)
        {
            _bannedWordService = bannedWordService;
            _localizationService = localizationService;
            _spamService = spamService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            _bannedWordService.RefreshContext(context);
            _localizationService.RefreshContext(context);
            _spamService.RefreshContext(context);

            try
            {
                // Get the all the words I need
                var allWords = await context.BannedWord.ToListAsync();

                // Do the stop words first
                foreach (var stopWord in allWords.Where(x => x.IsStopWord == true).Select(x => x.Word).ToArray())
                {
                    if (input.EntityToProcess.PostContent.IndexOf(stopWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        input.AddError(_localizationService.GetResourceString("StopWord.Error"));
                        return input;
                    }
                }

                // Check Akismet
                if (_spamService.IsSpam(input.EntityToProcess))
                {
                    input.EntityToProcess.Pending = true;
                    input.ExtendedData.Add(Constants.ExtendedDataKeys.Moderate, true);
                }

                // Sanitise the banned words
                var bannedWords = allWords.Where(x => x.IsStopWord != true).Select(x => x.Word).ToArray();
                input.EntityToProcess.PostContent = _bannedWordService.SanitiseBannedWords(input.EntityToProcess.PostContent, bannedWords);
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