using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class CategoryNotificationService : ICategoryNotificationService
    {
        private readonly ICategoryNotificationRepository _categoryNotificationRepository;

        public CategoryNotificationService(ICategoryNotificationRepository categoryNotificationRepository)
        {
            _categoryNotificationRepository = categoryNotificationRepository;
        }

        /// <summary>
        /// Return all category notifications
        /// </summary>
        /// <returns></returns>
        public IList<CategoryNotification> GetAll()
        {
            return _categoryNotificationRepository.GetAll();
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        /// <param name="notification"></param>
        public void Delete(CategoryNotification notification)
        {
            _categoryNotificationRepository.Delete(notification);
        }

        /// <summary>
        /// Return all notifications by a specified category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public IList<CategoryNotification> GetByCategory(Category category)
        {
            return _categoryNotificationRepository.GetByCategory(category);
        }

        /// <summary>
        /// Return notifications for a specified user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IList<CategoryNotification> GetByUser(MembershipUser user)
        {
            return _categoryNotificationRepository.GetByUser(user);
        }

        /// <summary>
        /// Return notifications for a specified user and category
        /// </summary>
        /// <param name="user"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public IList<CategoryNotification> GetByUserAndCategory(MembershipUser user, Category category)
        {
            return _categoryNotificationRepository.GetByUserAndCategory(user, category);
        }

        /// <summary>
        /// Add a new category notification
        /// </summary>
        /// <param name="category"></param>
        public void Add(CategoryNotification category)
        {
            _categoryNotificationRepository.Add(category);

        }
    }
}
