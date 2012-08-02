using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        IList<Category> GetAll();
        IList<Category> GetAllSubCategories(Guid parentId);
        IList<Category> GetMainCategories();
        Category Add(Category newsItem);
        Category GetBySlug(string slug);
        IList<Category> GetBySlugLike(string slug);
        void Delete(Category category);
        Category Get(Guid id);
        void Update(Category item);
    }
}
