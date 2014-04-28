using System;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public partial class EmailController : BaseController
    {
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly ICategoryNotificationService _categoryNotificationService;
        private readonly ICategoryService _categoryService;
        private readonly ITopicService _topicService;

        private MembershipUser LoggedOnUser;

        public EmailController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            ITopicNotificationService topicNotificationService, ICategoryNotificationService categoryNotificationService, ICategoryService categoryService,
            ITopicService topicService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _topicNotificationService = topicNotificationService;
            _categoryNotificationService = categoryNotificationService;
            _categoryService = categoryService;
            _topicService = topicService;

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
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
                        var id = subscription.Id;

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
                                    User = LoggedOnUser
                                };
                                //save

                                _categoryNotificationService.Add(categoryNotification);   
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
                                    User = LoggedOnUser
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
                        var id = subscription.Id;

                        if (isCategory)
                        {
                            // get the category
                            var cat = _categoryService.Get(id);

                            if (cat != null)
                            {        
                                // get the notifications by user
                                var notifications = _categoryNotificationService.GetByUserAndCategory(LoggedOnUser, cat);

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
                        else
                        {
                            // get the topic
                            var topic = _topicService.Get(id);

                            if (topic != null)
                            {
                                // get the notifications by user
                                var notifications = _topicNotificationService.GetByUserAndTopic(LoggedOnUser, topic);

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
