namespace MvcForum.Plugins.Pipelines.Topic
{
    using System;
    using System.Collections.Generic;
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
        private readonly ILoggingService _loggingService;
        private readonly ITopicService _topicService;

        public TopicSpamPipe(IBannedWordService bannedWordService, ILocalizationService localizationService, ISpamService spamService, 
            ILoggingService loggingService, ITopicService topicService)
        {
            _bannedWordService = bannedWordService;
            _localizationService = localizationService;
            _spamService = spamService;
            _loggingService = loggingService;
            _topicService = topicService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            _bannedWordService.RefreshContext(context);
            _localizationService.RefreshContext(context);
            _spamService.RefreshContext(context);

            try
            {
                // Get the all the words I need
                var allWords = await context.BannedWord.ToListAsync();

                var hasPollAnswers = input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.PollNewAnswers);
                var newPollAnswers = new List<PollAnswer>();
                if (hasPollAnswers)
                {
                    newPollAnswers = input.ExtendedData[Constants.ExtendedDataKeys.PollNewAnswers] as List<PollAnswer>;
                }

                // Do the stop words first
                foreach (var stopWord in allWords.Where(x => x.IsStopWord == true).Select(x => x.Word).ToArray())
                {
                    // Check name
                    if (input.EntityToProcess.Name.IndexOf(stopWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        input.AddError(_localizationService.GetResourceString("StopWord.Error"));
                        return input;
                    }

                    // Check poll answers on entity or ones we are waiting to update
                    if (input.EntityToProcess.Poll != null && input.EntityToProcess.Poll.PollAnswers.Any())
                    {
                        foreach (var pollAnswer in input.EntityToProcess.Poll.PollAnswers)
                        {
                            if (pollAnswer.Answer.IndexOf(stopWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
                            {
                                input.AddError(_localizationService.GetResourceString("StopWord.Error"));
                                return input;
                            }
                        }
                    }

                    if (hasPollAnswers)
                    {
                        if (newPollAnswers != null)
                        {
                            foreach (var pollAnswer in newPollAnswers)
                            {
                                if (pollAnswer.Answer.IndexOf(stopWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                {
                                    input.AddError(_localizationService.GetResourceString("StopWord.Error"));
                                    return input;
                                }
                            }
                        }
                    }

                    // Check tags
                    if (input.EntityToProcess.Tags != null && input.EntityToProcess.Tags.Any())
                    {
                        foreach (var topicTag in input.EntityToProcess.Tags)
                        {
                            if (topicTag.Tag.IndexOf(stopWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
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

                // Flood Check - Only if we are not editing
                // Are we in an edit mode
                var isEdit = input.ExtendedData[Constants.ExtendedDataKeys.IsEdit] as bool? == true;
                if (!isEdit)
                {
                    // Get the Current user from ExtendedData
                    var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;
                    var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);
                    if (loggedOnUser != null)
                    {
                        if (!_topicService.PassedTopicFloodTest(input.EntityToProcess.Name, loggedOnUser))
                        {
                            input.AddError(_localizationService.GetResourceString("Spam.FloodTestFailed"));
                            return input;
                        }
                    }
                    else
                    {
                        input.AddError("Unable to get user from username");
                        return input;
                    }
                }

                // Sanitise Poll
                if (input.EntityToProcess.Poll != null && input.EntityToProcess.Poll.PollAnswers.Any())
                {
                    foreach (var pollAnswer in input.EntityToProcess.Poll.PollAnswers)
                    {
                        pollAnswer.Answer = _bannedWordService.SanitiseBannedWords(pollAnswer.Answer, bannedWords);
                    }
                }

                // Santise new poll answers
                if (hasPollAnswers)
                {
                    if (newPollAnswers != null)
                    {
                        foreach (var pollAnswer in newPollAnswers)
                        {
                            pollAnswer.Answer = _bannedWordService.SanitiseBannedWords(pollAnswer.Answer, bannedWords);
                        }

                        // Now re-assign them
                        input.ExtendedData[Constants.ExtendedDataKeys.PollNewAnswers] = newPollAnswers;
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