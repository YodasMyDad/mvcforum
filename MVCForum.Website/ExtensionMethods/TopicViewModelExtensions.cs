namespace MvcForum.Web.ViewModels.ExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Models.Entities;
    using Topic;

    public static partial class TopicViewModelExtensions
    {
        public static Topic ToTopic(this CreateEditTopicViewModel viewModel, Category category, MembershipUser user, Topic existingTopic)
        {
            if (existingTopic == null)
            {
                existingTopic = new Topic
                {
                    CreateDate = DateTime.UtcNow,
                    User = user
                };
            }

            existingTopic.Name = viewModel.Name;
            existingTopic.Category = category;
            existingTopic.IsLocked = viewModel.IsLocked;
            existingTopic.IsSticky = viewModel.IsSticky;

            // See if we have a poll and add it unless there is already one, as we'll need to refresh
            // The poll in a later pipeline
            if (viewModel.PollAnswers.Any(x => x != null) && existingTopic.Poll == null)
            {
                // Create a new Poll as one does not already exist
                var newPoll = new Poll
                {
                    User = user,
                    ClosePollAfterDays = viewModel.PollCloseAfterDays,
                    DateCreated = DateTime.UtcNow
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
                existingTopic.Poll = newPoll;
            }

            return existingTopic;
        }
    }
}