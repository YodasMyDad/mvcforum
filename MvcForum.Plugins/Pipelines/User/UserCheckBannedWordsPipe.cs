namespace MvcForum.Plugins.Pipelines.User
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
    using Core.Utilities;

    public class UserCheckBannedWordsPipe : IPipe<IPipelineProcess<MembershipUser>>
    {
        private readonly IBannedWordService _bannedWordService;
        private readonly ILoggingService _loggingService;

        public UserCheckBannedWordsPipe(IBannedWordService bannedWordService, ILoggingService loggingService)
        {
            _bannedWordService = bannedWordService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<MembershipUser>> Process(IPipelineProcess<MembershipUser> input,
            IMvcForumContext context)
        {
            _bannedWordService.RefreshContext(context);

            try
            {
                var email = input.EntityToProcess.Email;
                var userName = input.EntityToProcess.UserName;

                // Grab the email from the banned list
                var emailIsBanned = await context.BannedEmail.AnyAsync(x => x.Email == email);
                if (emailIsBanned == false)
                {
                    // Email is ok, now check for banned words
                    // Doesn't matter if it's a stop word or banned word, we'll reject it
                    var allBannedWords = await context.BannedWord.ToListAsync();
                    var allStopWords = allBannedWords.Where(x => x.IsStopWord == true).Select(x => x.Word).ToArray();

                    // Loop banned Words
                    foreach (var bannedWord in allBannedWords)
                    {
                        // Email
                        if (email.ContainsCaseInsensitive(bannedWord.Word))
                        {
                            input.Successful = false;
                            input.ProcessLog.Clear();
                            input.ProcessLog.Add($"The email {email} contains a stop word");
                            return input;
                        }

                        // Username
                        if (userName.ContainsCaseInsensitive(bannedWord.Word))
                        {
                            input.Successful = false;
                            input.ProcessLog.Clear();
                            input.ProcessLog.Add($"The Username {userName} contains a stop word");
                            return input;
                        }
                    }

                    // Loop all stop words
                    foreach (var allStopWord in allStopWords)
                    {
                        // Check for stop words
                        if (input.EntityToProcess.Facebook != null &&
                            input.EntityToProcess.Facebook.IndexOf(allStopWord, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            input.EntityToProcess.Location != null && 
                            input.EntityToProcess.Location.IndexOf(allStopWord, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            input.EntityToProcess.Signature != null && 
                            input.EntityToProcess.Signature.IndexOf(allStopWord, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            input.EntityToProcess.Twitter != null && 
                            input.EntityToProcess.Twitter.IndexOf(allStopWord, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                            input.EntityToProcess.Website != null && 
                            input.EntityToProcess.Website.IndexOf(allStopWord, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        {
                            input.Successful = false;
                            input.ProcessLog.Clear();
                            input.ProcessLog.Add($"The Username {userName} contains a stop word");
                        }
                    }

                    // Santise the fields now 
                    var justBannedWord = allBannedWords.Where(x => x.IsStopWord == false).Select(x => x.Word).ToArray();
                   
                    // Sanitised
                    input.EntityToProcess.Facebook = _bannedWordService.SanitiseBannedWords(input.EntityToProcess.Facebook, justBannedWord);
                    input.EntityToProcess.Location = _bannedWordService.SanitiseBannedWords(input.EntityToProcess.Location, justBannedWord);
                    input.EntityToProcess.Signature = _bannedWordService.SanitiseBannedWords(StringUtils.ScrubHtml(input.EntityToProcess.Signature, true), justBannedWord);
                    input.EntityToProcess.Twitter = _bannedWordService.SanitiseBannedWords(input.EntityToProcess.Twitter, justBannedWord);
                    input.EntityToProcess.Website = _bannedWordService.SanitiseBannedWords(input.EntityToProcess.Website, justBannedWord);
                }
                else
                {
                    input.Successful = false;
                    input.ProcessLog.Clear();
                    input.ProcessLog.Add($"The email {email} is banned");
                }
            }
            catch (Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }

            if (input.Successful)
            {
                input.ProcessLog.Add("CheckBannedWordsPipe completed successfully");
            }

            return input;
        }
    }
}