using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface ICategoryRepository
    {
        IList<Category> GetAll();
        IList<Category> GetAllSubCategories(Guid parentId);
        IList<Category> GetAllDeepSubCategories(Category category);
        IList<Category> GetMainCategories();
        Category Add(Category newsItem);
        CategoryWithSubCategories GetBySlugWithSubCategories(string slug);
        Category GetBySlug(string slug);
        IList<Category> GetBySlugLike(string slug);
        IList<Category> GetCategoryParents(Category category);
        void Delete(Category category);
        Category Get(Guid id);
        IList<Category> Get(IList<Guid> ids, bool fullGraph = false);
    }
}
