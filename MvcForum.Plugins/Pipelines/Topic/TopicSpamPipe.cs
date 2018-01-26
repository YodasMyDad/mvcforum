namespace MvcForum.Plugins.Pipelines.Topic
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

    public class TopicSpamPipe : IPipe<IPipelineProcess<Topic>>
    {
        private readonly IBannedWordService _bannedWordService;
        private readonly ILocalizationService _localizationService;
        private readonly ISpamService _spamService;

        public TopicSpamPipe(IBannedWordService bannedWordService, ILocalizationService localizationService, ISpamService spamService)
        {
            _bannedWordService = bannedWordService;
            _localizationService = localizationService;
            _spamService = spamService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            // Get the all the words I need
            var allWords = await context.BannedWord.ToListAsync();

            // Do the stop words first
            foreach (var stopWord in allWords.Where(x => x.IsStopWord == true).Select(x => x.Word).ToArray())
            {
                if (input.EntityToProcess.Name.IndexOf(stopWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
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
            input.EntityToProcess.Name = _bannedWordService.SanitiseBannedWords(input.EntityToProcess.Name, bannedWords);

            return input;
        }
    }
}