namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Constants;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;
    using Utilities;

    public partial class PollService : IPollService
    {
        private readonly ICacheService _cacheService;
        private IMvcForumContext _context;

        public PollService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        #region Poll

        /// <inheritdoc />
        public List<Poll> GetAllPolls()
        {
            return _context.Poll.ToList();
        }

        /// <inheritdoc />
        public Poll Add(Poll poll)
        {
            poll.DateCreated = DateTime.UtcNow;
            poll.IsClosed = false;
            return _context.Poll.Add(poll);
        }

        /// <inheritdoc />
        public Poll Get(Guid id)
        {
            return _context.Poll.FirstOrDefault(x => x.Id == id);
        }

        /// <inheritdoc />
        public void Delete(Poll item)
        {
            var pollAnswers = new List<PollAnswer>();
            pollAnswers.AddRange(item.PollAnswers);
            foreach (var itemPollAnswer in pollAnswers)
            {
                var pollVotes = new List<PollVote>();
                pollVotes.AddRange(itemPollAnswer.PollVotes);
                foreach (var pollVote in pollVotes)
                {
                    itemPollAnswer.PollVotes.Remove(pollVote);
                    _context.PollVote.Remove(pollVote);
                }

                // Delete poll answer
                item.PollAnswers.Remove(itemPollAnswer);
                _context.PollAnswer.Remove(itemPollAnswer);
            }

            item.User = null;

            _context.Poll.Remove(item);
        }

        /// <inheritdoc />
        public void RefreshEditedPoll(Topic originalTopic, IList<PollAnswer> pollAnswers, int pollCloseAfterDays)
        {
            var panswers = pollAnswers.Where(x => !string.IsNullOrWhiteSpace(x?.Answer)).ToArray();
            if (panswers.Any() && originalTopic.Poll != null)
            {
                // Now sort the poll answers, what to add and what to remove
                // Poll answers already in this poll.
                var newPollAnswerIds = panswers.Select(x => x.Id);

                // This post might not have a poll on it, if not they are creating a poll for the first time
                var originalPollAnswerIds = originalTopic.Poll.PollAnswers.Select(p => p.Id).ToList();
                var pollAnswersToRemove = originalTopic.Poll.PollAnswers.Where(x => !newPollAnswerIds.Contains(x.Id))
                    .ToList();

                // Set the amount of days to close the poll
                originalTopic.Poll.ClosePollAfterDays = pollCloseAfterDays;

                // Get existing answers
                var existingAnswers = panswers.Where(x =>
                    !string.IsNullOrWhiteSpace(x.Answer) && originalPollAnswerIds.Contains(x.Id)).ToList();

                // Get new poll answers to add
                var newPollAnswers = panswers.Where(x =>
                    !string.IsNullOrWhiteSpace(x.Answer) && !originalPollAnswerIds.Contains(x.Id)).ToList();

                // Loop through existing and update names if need be
                // If name changes remove the poll
                foreach (var existPollAnswer in existingAnswers)
                {
                    // Get the existing answer from the current topic
                    var pa = originalTopic.Poll.PollAnswers.FirstOrDefault(x => x.Id == existPollAnswer.Id);
                    if (pa != null && pa.Answer != existPollAnswer.Answer)
                    {
                        var pollVotestToRemove = new List<PollVote>();
                        pollVotestToRemove.AddRange(pa.PollVotes);
                        // Remove all the poll votes, as the answer has changed
                        foreach (var answerPollVote in pollVotestToRemove)
                        {
                            pa.PollVotes.Remove(answerPollVote);
                            Delete(answerPollVote);
                        }
                        pa.PollVotes.Clear();
                        _context.SaveChanges();

                        // If the answer has changed then update it
                        pa.Answer = existPollAnswer.Answer;
                    }
                }

                // Save existing
                _context.SaveChanges();

                // Loop through and remove the old poll answers and delete
                foreach (var oldPollAnswer in pollAnswersToRemove)
                {
                    // Clear poll votes if it's changed
                    var pollVotestToRemove = new List<PollVote>();
                    pollVotestToRemove.AddRange(oldPollAnswer.PollVotes);
                    foreach (var answerPollVote in pollVotestToRemove)
                    {
                        oldPollAnswer.PollVotes.Remove(answerPollVote);
                        Delete(answerPollVote);
                    }
                    oldPollAnswer.PollVotes.Clear();
                    _context.SaveChanges();

                    // Remove from Poll
                    originalTopic.Poll.PollAnswers.Remove(oldPollAnswer);

                    // Delete
                    Delete(oldPollAnswer);
                }

                // Save removed
                _context.SaveChanges();

                // Poll answers to add
                foreach (var newPollAnswer in newPollAnswers)
                {
                    if (newPollAnswer != null)
                    {
                        var npa = new PollAnswer
                        {
                            Poll = originalTopic.Poll,
                            Answer = newPollAnswer.Answer
                        };
                        Add(npa);
                        originalTopic.Poll.PollAnswers.Add(npa);
                    }
                }
            }
            else if(originalTopic.Poll != null)
            {
                // Now delete the poll
                Delete(originalTopic.Poll);

                // Remove from topic.
                originalTopic.Poll = null;
            }
        }

        #endregion

        #region Poll answers

        /// <inheritdoc />
        public List<PollAnswer> GetAllPollAnswers()
        {
            return _context.PollAnswer.Include(x => x.Poll).ToList();
        }

        /// <inheritdoc />
        public PollAnswer Add(PollAnswer pollAnswer)
        {
            pollAnswer.Answer = StringUtils.SafePlainText(pollAnswer.Answer);
            return _context.PollAnswer.Add(pollAnswer);
        }

        /// <inheritdoc />
        public List<PollAnswer> GetAllPollAnswersByPoll(Poll poll)
        {
            return _context.PollAnswer
                .Include(x => x.Poll)
                .AsNoTracking()
                .Where(x => x.Poll.Id == poll.Id).ToList();
        }

        /// <inheritdoc />
        public PollAnswer GetPollAnswer(Guid id)
        {
            return _context.PollAnswer.FirstOrDefault(x => x.Id == id);
        }

        /// <inheritdoc />
        public void Delete(PollAnswer pollAnswer)
        {
            _context.PollAnswer.Remove(pollAnswer);
        }

        #endregion

        #region Poll Vote

        /// <inheritdoc />
        public List<PollVote> GetAllPollVotes()
        {
            return _context.PollVote.ToList();
        }

        /// <inheritdoc />
        public PollVote Add(PollVote pollVote)
        {
            return _context.PollVote.Add(pollVote);
        }

        /// <inheritdoc />
        public bool HasUserVotedAlready(Guid answerId, Guid userId)
        {
            var vote = _context.PollVote.Include(x => x.PollAnswer).Include(x => x.User)
                .FirstOrDefault(x => x.PollAnswer.Id == answerId && x.User.Id == userId);
            return vote != null;
        }

        /// <inheritdoc />
        public PollVote GetPollVote(Guid id)
        {
            return _context.PollVote.FirstOrDefault(x => x.Id == id);
        }

        /// <inheritdoc />
        public void Delete(PollVote pollVote)
        {
            _context.PollVote.Remove(pollVote);
        }

        #endregion
    }
}