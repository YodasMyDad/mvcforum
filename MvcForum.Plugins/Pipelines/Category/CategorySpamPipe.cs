namespace MvcForum.Plugins.Pipelines.Category
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class CategorySpamPipe : IPipe<IPipelineProcess<Category>>
    {
        private readonly IBannedWordService _bannedWordService;
        private readonly ISpamService _spamService;
        private readonly ILocalizationService _localizationService;
        private readonly ILoggingService _loggingService;

        public CategorySpamPipe(IBannedWordService bannedWordService, ILocalizationService localizationService, ISpamService spamService, ILoggingService loggingService)
        {
            _bannedWordService = bannedWordService;
            _localizationService = localizationService;
            _spamService = spamService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Category>> Process(IPipelineProcess<Category> input,
            IMvcForumContext context)
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
                    if (input.EntityToProcess.Name.IndexOf(stopWord, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                        input.EntityToProcess.Description.IndexOf(stopWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        input.AddError(_localizationService.GetResourceString("StopWord.Error"));
                        return input;
                    }
                }

                // Sanitise the banned words
                var bannedWords = allWords.Where(x => x.IsStopWord != true).Select(x => x.Word).ToArray();
                input.EntityToProcess.Name = _bannedWordService.SanitiseBannedWords(input.EntityToProcess.Name, bannedWords);
                input.EntityToProcess.Description = _bannedWordService.SanitiseBannedWords(input.EntityToProcess.Description, bannedWords);

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