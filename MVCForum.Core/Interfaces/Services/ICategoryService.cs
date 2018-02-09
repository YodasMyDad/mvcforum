namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Models;
    using Models.Entities;
    using Models.General;
    using Pipeline;

    public partial interface ICategoryService : IContextService
    {
        List<Category> GetAll();
        IEnumerable<Category> GetAllMainCategories();
        IEnumerable<CategorySummary> GetAllMainCategoriesInSummary();
        ILookup<Guid, CategorySummary> GetAllMainCategoriesInSummaryGroupedBySection();

        /// <summary>
        ///     Gets categories that the user has access to (i.e. There access is not denied)
        /// </summary>
        /// <param name="role">Users Role</param>
        /// <returns></returns>
        List<Category> GetAllowedCategories(MembershipRole role);

        /// <summary>
        ///     Get category permissions for a specific permission
        /// </summary>
        /// <param name="role">Users Role</param>
        /// <param name="actionType">
        ///     Pass in the permission you want to check, for example 'Delete Posts' - This will return a list
        ///     of categories that the user has permission to delete posts
        /// </param>
        /// <returns></returns>
        List<Category> GetAllowedCategories(MembershipRole role, string actionType);
        IEnumerable<Category> GetAllSubCategories(Guid parentId);
        Category Get(Guid id);
        IList<Category> Get(IList<Guid> ids, bool fullGraph = false);
        CategoryWithSubCategories GetBySlugWithSubCategories(string slug);
        Category Get(string slug);
        List<Category> GetCategoryParents(Category category, List<Category> allowedCategories);
        Task<IPipelineProcess<Category>> Delete(Category category);
        Task<IPipelineProcess<Category>> Create(Category category, HttpPostedFileBase[] postedFiles, Guid? parentCategory, Guid? section);
        Task<IPipelineProcess<Category>> Edit(Category category, HttpPostedFileBase[] postedFiles, Guid? parentCategory, Guid? section);
        void UpdateSlugFromName(Category category);
        Category SanitizeCategory(Category category);
        List<Category> GetSubCategories(Category category, List<Category> allCategories, int level = 2);
        List<SelectListItem> GetBaseSelectListCategories(List<Category> allowedCategories);
        Category GetBySlug(string slug);
        IList<Category> GetBySlugLike(string slug);
        IList<Category> GetAllDeepSubCategories(Category category);
        void SortPath(Category category, Category parentCategory);
        List<Section> GetAllSections();
        Section GetSection(Guid id);
        void DeleteSection(Guid id);
    }
}