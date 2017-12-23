namespace MVCForum.Services
{
    using Domain.Constants;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Domain.DomainModel;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;

    public partial class CategoryNotificationService : ICategoryNotificationService
    {
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        public CategoryNotificationService(IMVCForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Return all category notifications
        /// </summary>
        /// <returns></returns>
        public IList<CategoryNotification> GetAll()
        {
            var cacheKey = string.Concat(CacheKeys.CategoryNotification.StartsWith, "GetAll");
            return _cacheService.CachePerRequest(cacheKey, () => _context.CategoryNotification.ToList());
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        /// <param name="notification"></param>
        public void Delete(CategoryNotification notification)
        {
            _context.CategoryNotification.Remove(notification);
        }

        /// <summary>
        /// Return all notifications by a specified category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public IList<CategoryNotification> GetByCategory(Category category)
        {
            var cacheKey = string.Concat(CacheKeys.CategoryNotification.StartsWith, "GetByCategory-", category.Id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.CategoryNotification
                                                                        .AsNoTracking()
                                                                        .Where(x => x.Category.Id == category.Id)
                                                                        .ToList());
        }

        /// <summary>
        /// Return notifications for a specified user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IList<CategoryNotification> GetByUser(MembershipUser user)
        {
            var cacheKey = string.Concat(CacheKeys.CategoryNotification.StartsWith, "GetByUser-", user.Id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.CategoryNotification
                                                                        .Where(x => x.User.Id == user.Id)
                                                                        .ToList());
        }

        /// <summary>
        /// Return notifications for a specified user and category
        /// </summary>
        /// <param name="user"></param>
        /// <param name="category"></param>
        /// <param name="addTracking"></param>
        /// <returns></returns>
        public IList<CategoryNotification> GetByUserAndCategory(MembershipUser user, Category category, bool addTracking = false)
        {
            var cacheKey = string.Concat(CacheKeys.CategoryNotification.StartsWith, "GetByUserAndCategory-", user.Id, "-", category.Id, "-", addTracking);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var notifications = _context.CategoryNotification.Where(x => x.Category.Id == category.Id && x.User.Id == user.Id);
                if (addTracking)
                {
                    return notifications.ToList();
                }
                return notifications.AsNoTracking().ToList();
            });
        }

        /// <summary>
        /// Add a new category notification
        /// </summary>
        /// <param name="category"></param>
        public CategoryNotification Add(CategoryNotification category)
        {
            return _context.CategoryNotification.Add(category);

        }

        public CategoryNotification Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.CategoryNotification.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.CategoryNotification.FirstOrDefault(cat => cat.Id == id));
        }
    }
}
