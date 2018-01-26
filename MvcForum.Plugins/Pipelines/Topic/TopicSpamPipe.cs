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

    /// <summary>
    /// Checks the topic for spam
    /// </summary>
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
                // Check name
                if (input.EntityToProcess.Name.IndexOf(stopWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    input.AddError(_localizationService.GetResourceString("StopWord.Error"));
                    return input;
                }

                // Check poll answers
                if (input.EntityToProcess.Poll != null && input.EntityToProcess.Poll.PollAnswers.Any())
                {
                    foreach (var pollAnswer in input.EntityToProcess.Poll.PollAnswers)
                    {
                        if (input.EntityToProcess.Name.IndexOf(pollAnswer.Answer, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            input.AddError(_localizationService.GetResourceString("StopWord.Error"));
                            return input;
                        }
                    }
                }

                // Check tags
                if (input.EntityToProcess.Tags != null && input.EntityToProcess.Tags.Any())
                {
                    foreach (var topicTag in input.EntityToProcess.Tags)
                    {
                        if (input.EntityToProcess.Name.IndexOf(topicTag.Tag, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            input.AddError(_localizationService.GetResourceString("StopWord.Error"));
                            return input;
                        }
                    }
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

            // Topic name
            input.EntityToProcess.Name = _bannedWordService.SanitiseBannedWords(input.EntityToProcess.Name, bannedWords);

            // Sanitise Poll
            if (input.EntityToProcess.Poll != null && input.EntityToProcess.Poll.PollAnswers.Any())
            {
                foreach (var pollAnswer in input.EntityToProcess.Poll.PollAnswers)
                {
                    pollAnswer.Answer = _bannedWordService.SanitiseBannedWords(pollAnswer.Answer, bannedWords);
                }
            }

            // Sanitise Tags
            if (input.EntityToProcess.Tags != null && input.EntityToProcess.Tags.Any())
            {
                foreach (var topicTag in input.EntityToProcess.Tags)
                {
                    topicTag.Tag = _bannedWordService.SanitiseBannedWords(topicTag.Tag, bannedWords);
                }
            }

            return input;
        }
    }
}