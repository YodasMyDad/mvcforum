using System;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    [Authorize]
    public partial class PollController : BaseController
    {
        private readonly IPollService _pollService;
        private readonly IPollVoteService _pollVoteService;
        private readonly IPollAnswerService _pollAnswerService;

        private MembershipUser LoggedOnUser;

        public PollController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, IPollService pollService, IPollVoteService pollVoteService, 
            IPollAnswerService pollAnswerService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _pollService = pollService;
            _pollAnswerService = pollAnswerService;
            _pollVoteService = pollVoteService;


            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
        }

        [HttpPost]
        public PartialViewResult UpdatePoll(UpdatePollViewModel updatePollViewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var poll = _pollService.Get(updatePollViewModel.PollId);
                    // Fist need to check this user hasn't voted already and is trying to fudge the system
                    if(!_pollVoteService.HasUserVotedAlready(updatePollViewModel.AnswerIds, LoggedOnUser.Id))
                    {
                        if (updatePollViewModel.AnswerIds.Count > 1 && !poll.IsMultipleChoice)
                        {
                            throw new InvalidOperationException("Can only select multiple answers if poll creator allows for it.");
                        }
                        foreach(Guid answerID in updatePollViewModel.AnswerIds)
                        {
                            // Get the answer
                            var pollAnswer = _pollAnswerService.Get(answerID);

                            // create a new vote
                            var pollVote = new PollVote { PollAnswer = pollAnswer, User = LoggedOnUser };

                            // Add it
                            _pollVoteService.Add(pollVote);
                        }

                        // Update the context so the changes are reflected in the viewmodel below
                        unitOfWork.SaveChanges();
                    }

                    // Create the view model and get ready return the poll partial view
                    var votes = poll.PollAnswers.SelectMany(x => x.PollVotes).ToList();
                    var alreadyVoted = (votes.Count(x => x.User.Id == LoggedOnUser.Id) > 0);
                    var viewModel = new PollViewModel { Poll = poll, TotalVotesInPoll = votes.Count(), UserHasAlreadyVoted = alreadyVoted };

                    // Commit the transaction
                    unitOfWork.Commit();

                    return PartialView("_Poll", viewModel);
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                } 
            }
        }

    }
}
