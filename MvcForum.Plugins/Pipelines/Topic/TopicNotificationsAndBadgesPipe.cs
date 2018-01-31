namespace MvcForum.Plugins.Pipelines.Topic
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.Enums;

    public class TopicNotificationsAndBadgesPipe : IPipe<IPipelineProcess<Topic>>
    {
        private readonly IBadgeService _badgeService;
        private readonly INotificationService _notificationService;
        private readonly IActivityService _activityService;
        private readonly ILocalizationService _localizationService;

        public TopicNotificationsAndBadgesPipe(IBadgeService badgeService, INotificationService notificationService, 
            IActivityService activityService, ILocalizationService localizationService)
        {
            _badgeService = badgeService;
            _notificationService = notificationService;
            _activityService = activityService;
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            // Get the Current user from ExtendedData
            var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;
            var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);

            // Are we in an edit mode
            var isEdit = input.ExtendedData[Constants.ExtendedDataKeys.IsEdit] as bool? == true;

            // If the topic has tags then process
            if (input.EntityToProcess.Tags.Any())
            {
                _badgeService.ProcessBadge(BadgeType.Tag, input.EntityToProcess.User);   
            }

            if (isEdit == false)
            {
                // Subscribe the user to the topic as they have checked the checkbox
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.Subscribe))
                {
                    var subscribe = input.ExtendedData[Constants.ExtendedDataKeys.Subscribe] as bool?;
                    if (subscribe == true)
                    {
                        var alreadyHasNotification = await context.TopicNotification
                            .Include(x => x.Topic)
                            .Include(x => x.User)
                            .AnyAsync(x =>
                                x.Topic.Id == input.EntityToProcess.Id && x.User.Id == loggedOnUser.Id);

                        if (alreadyHasNotification == false)
                        {
                            // Create the notification
                            var topicNotification = new TopicNotification
                            {
                                Topic = input.EntityToProcess,
                                User = loggedOnUser
                            };

                            //save
                            _notificationService.Add(topicNotification);
                        }
                    }
                }

                // Should we add the topic created activity
                if (input.EntityToProcess.Pending != true)
                {
                    _activityService.TopicCreated(input.EntityToProcess);
                }

                // finally notify 
                _notificationService.Notify(input.EntityToProcess, loggedOnUser, NotificationType.Topic);
            }

            // Was the post successful
            if (await context.SaveChangesAsync() <= 0)
            {
                // Problem
                input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
            }

            return input;
        }
    }
}