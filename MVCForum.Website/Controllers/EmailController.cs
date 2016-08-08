namespace MVCForum.Website.Controllers
{
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using Domain.DomainModel;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;
    using ViewModels;

    public partial class EmailController : BaseController
    {
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly ICategoryNotificationService _categoryNotificationService;
        private readonly ITagNotificationService _tagNotificationService;
        private readonly ICategoryService _categoryService;
        private readonly ITopicService _topicService;
        private readonly ITopicTagService _topicTagService;

        public EmailController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            ITopicNotificationService topicNotificationService, ICategoryNotificationService categoryNotificationService, ICategoryService categoryService,
            ITopicService topicService, ITopicTagService topicTagService, ITagNotificationService tagNotificationService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService, cacheService)
        {
            _topicNotificationService = topicNotificationService;
            _categoryNotificationService = categoryNotificationService;
            _categoryService = categoryService;
            _topicService = topicService;
            _topicTagService = topicTagService;
            _tagNotificationService = tagNotificationService;
        }

        [HttpPost]
        [Authorize]
        public void Subscribe(SubscribeEmailViewModel subscription)
        {
            if(Request.IsAjaxRequest())
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        // Add logic to add subscr
                        var isCategory = subscription.SubscriptionType.Contains("category");
                        var isTag = subscription.SubscriptionType.Contains("tag");
                        var id = subscription.Id;
                        var dbUser = MembershipService.GetUser(LoggedOnReadOnlyUser.Id);

                        if (isCategory)
                        {
                            // get the category
                            var cat = _categoryService.Get(id);

                            if(cat != null)
                            {
                                
                                // Create the notification
                                var categoryNotification = new CategoryNotification
                                {
                                    Category = cat,
                                    User = dbUser
                                };
                                //save

                                _categoryNotificationService.Add(categoryNotification);   
                            }
                        }
                        else if (isTag)
                        {
                            // get the tag
                            var tag = _topicTagService.Get(id);

                            if (tag != null)
                            {

                                // Create the notification
                                var tagNotification = new TagNotification
                                {
                                    Tag = tag,
                                    User = dbUser
                                };
                                //save

                                _tagNotificationService.Add(tagNotification);
                            }
                        }
                        else
                        {
                            // get the category
                            var topic = _topicService.Get(id);

                            // check its not null
                            if (topic != null)
                            {

                                // Create the notification
                                var topicNotification = new TopicNotification
                                {
                                    Topic = topic,
                                    User = dbUser
                                };
                                //save

                                _topicNotificationService.Add(topicNotification);
                            }
                        }

                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }                   
                }
            }
            else
            {
                throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));                
            }
        }

        [HttpPost]
        [Authorize]
        public void UnSubscribe(UnSubscribeEmailViewModel subscription)
        {
            if (Request.IsAjaxRequest())
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {
                        // Add logic to add subscr
                        var isCategory = subscription.SubscriptionType.Contains("category");
                        var isTag = subscription.SubscriptionType.Contains("tag");
                        var id = subscription.Id;
                        var dbUser = MembershipService.GetUser(LoggedOnReadOnlyUser.Id);
                        if (isCategory)
                        {
                            // get the category
                            var cat = _categoryService.Get(id);

                            if (cat != null)
                            {        
                                // get the notifications by user
                                var notifications = _categoryNotificationService.GetByUserAndCategory(LoggedOnReadOnlyUser, cat, true);

                                if(notifications.Any())
                                {
                                    foreach (var categoryNotification in notifications)
                                    {
                                        // Delete
                                        _categoryNotificationService.Delete(categoryNotification);
                                    }         
                                }
                           
                            }
                        }
                        else if (isTag)
                        {
                            // get the tag
                            var tag = _topicTagService.Get(id);

                            if (tag != null)
                            {
                                // get the notifications by user
                                var notifications = _tagNotificationService.GetByUserAndTag(LoggedOnReadOnlyUser, tag, true);

                                if (notifications.Any())
                                {
                                    foreach (var n in notifications)
                                    {
                                        // Delete
                                        _tagNotificationService.Delete(n);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // get the topic
                            var topic = _topicService.Get(id);

                            if (topic != null)
                            {
                                // get the notifications by user
                                var notifications = _topicNotificationService.GetByUserAndTopic(LoggedOnReadOnlyUser, topic, true);

                                if (notifications.Any())
                                {
                                    foreach (var topicNotification in notifications)
                                    {
                                        // Delete
                                        _topicNotificationService.Delete(topicNotification);
                                    }
                                }

                            }
                        }

                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }
            else
            {
                throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }
        }
    }
}
