using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface ICategoryService
    {
        IEnumerable<Category> GetAll();
        IEnumerable<Category> GetAllMainCategories();
        IEnumerable<Category> GetAllowedCategories(MembershipRole role);
        IEnumerable<Category> GetAllSubCategories(Guid parentId);
        Category Get(Guid id);
        IList<Category> Get(IList<Guid> ids);
        CategoryWithSubCategories GetBySlugWithSubCategories(string slug);
        Category Get(string slug);
        IList<Category> GetCategoryParents(Category category);
        void Delete(Category category);
        void Add(Category category);
        void Save(Category category);
        void UpdateSlugFromName(Category category);
        Category SanitizeCategory(Category category);
    }
}
