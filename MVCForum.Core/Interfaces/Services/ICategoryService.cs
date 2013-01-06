using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAll();
        IEnumerable<Category> GetAllSubCategories(Guid parentId);
        IEnumerable<Category> GetAllMainCategories();
        IEnumerable<Category> GetAllowedCategories(MembershipRole role);
        Category Get(Guid id);
        Category Get(string slug);
        void Delete(Category category);
        void Add(Category category);
        void Save(Category category);
        void UpdateSlugFromName(Category category);
        Category SanitizeCategory(Category category);
    }
}
