namespace MvcForum.Plugins.Pipelines.User
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Utilities;

    public class CheckBannedWordsPipe : IPipe<IPipelineProcess<MembershipUser>>
    {
        private readonly IBannedWordService _bannedWordService;
        public CheckBannedWordsPipe(IBannedWordService bannedWordService)
        {
            _bannedWordService = bannedWordService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<MembershipUser>> Process(IPipelineProcess<MembershipUser> input,
            IMvcForumContext context)
        {
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

                    // Check signature
                    if (_bannedWordService.ContainsStopWords(input.EntityToProcess.Signature, allStopWords))
                    {
                        input.Successful = false;
                        input.ProcessLog.Clear();
                        input.ProcessLog.Add($"The Username {userName} contains a stop word");
                    }
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
                input.Successful = false;
                input.ProcessLog.Clear();
                input.ProcessLog.Add(ex.Message);
            }

            if (input.Successful)
            {
                input.ProcessLog.Add("CheckBannedWordsPipe completed successfully");
            }
            return input;
        }
    }
}