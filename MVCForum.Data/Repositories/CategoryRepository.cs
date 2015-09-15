using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class CategoryRepository : ICategoryRepository
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
                    .Include(x => x.ParentCategory)
                    .AsNoTracking()
                    .OrderBy(x => x.SortOrder)
                    .ToList();

        }

        public Category Get(Guid id)
        {
            return _context.Category.FirstOrDefault(x => x.Id == id);
        }

        public IList<Category> Get(IList<Guid> ids, bool fullGraph = false)
        {
            if (fullGraph)
            {
                return _context.Category
                        .AsNoTracking()
                        .Include(x => x.Topics.Select(l => l.LastPost.User))
                        .Include(x => x.ParentCategory)
                        .Where(x => ids.Contains(x.Id)).ToList();
            }
            return _context.Category
                .AsNoTracking().Where(x => ids.Contains(x.Id)).ToList();
        }

        public IList<Category> GetAllSubCategories(Guid parentId)
        {
            return _context.Category
                    .Where(x => x.ParentCategory.Id == parentId)
                    .OrderBy(x => x.SortOrder)
                    .ToList();
        }

        /// <summary>
        /// Gets all categories right the way down
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public IList<Category> GetAllDeepSubCategories(Category category)
        {
            var catGuid = category.Id.ToString().ToLower();
            return _context.Category
                    .Where(x => x.Path != null && x.Path.ToLower().Contains(catGuid))
                    .OrderBy(x => x.SortOrder)
                    .ToList();
        }

        public IList<Category> GetMainCategories()
        {
            var categories = _context.Category
                                .Include(x => x.ParentCategory)
                                .Include(x => x.Topics.Select(l => l.LastPost))
                                .Include(x => x.Topics.Select(l => l.Posts))
                                .Where(cat => cat.ParentCategory == null)
                                .OrderBy(x => x.SortOrder)
                                .ToList();

            return categories;
        }


        public Category Add(Category category)
        {
            _context.Category.Add(category);
            return category;
        }

        public CategoryWithSubCategories GetBySlugWithSubCategories(string slug)
        {
            var cat = (from category in _context.Category
                          where category.Slug == slug
                          select new CategoryWithSubCategories
                              {
                                  Category = category,
                                  SubCategories = (from cats in _context.Category
                                                   where cats.ParentCategory.Id == category.Id
                                                   select cats)
                              }).FirstOrDefault();

            return cat;
        }

        public Category GetBySlug(string slug)
        {
            return _context.Category.FirstOrDefault(x => x.Slug == slug);
        }


        public IList<Category> GetBySlugLike(string slug)
        {
            return _context.Category
                    .Where(x => x.Slug.Contains(slug))
                    .ToList();
        }

        /// <summary>
        /// Gets all parent categorys, all the way up to the root of the forum
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public IList<Category> GetCategoryParents(Category category)
        {
            var path = category.Path;
            var cats = new List<Category>();
            if (!string.IsNullOrEmpty(path))
            {
                var catGuids = path.Trim().Split(',').Select(x => new Guid(x)).ToList();
                if (!catGuids.Contains(category.Id))
                {
                    catGuids.Add(category.Id);
                }
                cats = Get(catGuids).ToList();
            }            
            return cats;
        }

        public void Delete(Category category)
        {
            _context.Category.Remove(category);
        }
    }
}
