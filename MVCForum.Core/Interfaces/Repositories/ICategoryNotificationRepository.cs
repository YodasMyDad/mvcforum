using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface ICategoryNotificationRepository
    {
        IList<CategoryNotification> GetAll();
        IList<CategoryNotification> GetByCategory(Category category);
        IList<CategoryNotification> GetByUser(MembershipUser user);
        IList<CategoryNotification> GetByUserAndCategory(MembershipUser user, Category category, bool addTracking = false);
        CategoryNotification Add(CategoryNotification category);
        void Delete(CategoryNotification category);
        CategoryNotification Get(Guid id);
        void Update(CategoryNotification item);
    }
}
