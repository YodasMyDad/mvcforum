using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Utilities;


namespace MVCForum.Data.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public CategoryRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public IList<Category> GetAll()
        {
            return _context.Category
                    .OrderBy(x => x.SortOrder)
                    .ToList();
        }

        public Category Get(Guid id)
        {
            return _context.Category.FirstOrDefault(x => x.Id == id);
        }

        public void Update(Category item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.Category.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified; 
        }

        public IList<Category> GetAllSubCategories(Guid parentId)
        {
            return _context.Category
                    .Where(x => x.ParentCategory.Id == parentId)
                    .OrderBy(x => x.SortOrder)
                    .ToList();
        }

        public IList<Category> GetMainCategories()
        {
            return _context.Category
                     .Where(cat => cat.ParentCategory == null)
                     .OrderBy(x => x.SortOrder)
                     .ToList();
        }


        public Category Add(Category category)
        {
            category.Id = GuidComb.GenerateComb();
            _context.Category.Add(category);
            return category;
        }

        public Category GetBySlug(string slug)
        {
            return _context.Category.SingleOrDefault(x => x.Slug == slug);
        }

        public IList<Category> GetBySlugLike(string slug)
        {
            return _context.Category
                .Where(x => x.Slug.ToUpper().Contains(slug.ToUpper()))
                .ToList();
        }

        public void Delete(Category category)
        {
            _context.Category.Remove(category);
        }
    }
}
