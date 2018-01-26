namespace MvcForum.Web.ViewModels.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Models.Entities;
    using Topic;

    public static partial class TopicViewModelExtensions
    {
        public static Topic ToTopic(this CreateEditTopicViewModel viewModel, Category category, MembershipUser user)
        {
            // Create the topic
            var topic = new Topic
            {
                Name = viewModel.Name,
                Category = category,
                User = user,
                CreateDate = DateTime.UtcNow,
                IsLocked = viewModel.IsLocked,
                IsSticky = viewModel.IsSticky
            };

            // See if we have a poll and add it
            if (viewModel.PollAnswers.Any(x => x != null))
            {
                // Create a new Poll
                var newPoll = new Poll
                {
                    User = user,
                    ClosePollAfterDays = viewModel.PollCloseAfterDays
                };

                // Now sort the answers
                var newPollAnswers = new List<PollAnswer>();
                foreach (var pollAnswer in viewModel.PollAnswers)
                {
                    if (pollAnswer.Answer != null)
                    {
                        // Attach newly created poll to each answer
                        pollAnswer.Poll = newPoll;
                        newPollAnswers.Add(pollAnswer);
                    }
                }
                // Attach answers to poll
                newPoll.PollAnswers = newPollAnswers;

                // Add the poll to the topic
                topic.Poll = newPoll;
            }

            // TODO - See if we have any files
            if (viewModel.Files.Any(x => x != null))
            {
                // TODO - How are we storing them???
            }

            // TODO - See if we have any tags
            if (!string.IsNullOrWhiteSpace(viewModel.Tags))
            {
                
            }

            // TODO - Search Field

            // TODO - Subscribe to topic ()

            return topic;
        }
    }
}