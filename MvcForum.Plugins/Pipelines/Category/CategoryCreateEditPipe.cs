namespace MvcForum.Plugins.Pipelines.Category
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Services;

    public class CategoryCreateEditPipe : IPipe<IPipelineProcess<Category>>
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILoggingService _loggingService;
        private readonly ICategoryService _categoryService;
        private readonly ICacheService _cacheService;

        public CategoryCreateEditPipe(ILocalizationService localizationService, ILoggingService loggingService, ICategoryService categoryService, ICacheService cacheService)
        {
            _localizationService = localizationService;
            _loggingService = loggingService;
            _categoryService = categoryService;
            _cacheService = cacheService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Category>> Process(IPipelineProcess<Category> input,
            IMvcForumContext context)
        {
            _localizationService.RefreshContext(context);
            _categoryService.RefreshContext(context);

            try
            {
                Guid? parentCategoryGuid = null;
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.ParentCategory))
                {
                    parentCategoryGuid = input.ExtendedData[Constants.ExtendedDataKeys.ParentCategory] as Guid?;
                }

                // Sort if this is a section
                Section section = null;
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.Section))
                {
                    if (input.ExtendedData[Constants.ExtendedDataKeys.Section] is Guid guid)
                    {
                        section = _categoryService.GetSection(guid);
                    }
                }

                // Sort the section - If it's null remove it
                input.EntityToProcess.Section = section ?? null;

                var isEdit = false;
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.IsEdit))
                {
                    isEdit = input.ExtendedData[Constants.ExtendedDataKeys.IsEdit] as bool? == true;
                }

                if (isEdit)
                {

                    var parentCat = parentCategoryGuid != null
                        ? _categoryService.Get(parentCategoryGuid.Value)
                        : null;

                    // Check they are not trying to add a subcategory of this category as the parent or it will break
                    if (parentCat?.Path != null && parentCategoryGuid != null)
                    {
                        var parentCats = parentCat.Path.Split(',').Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(x => new Guid(x)).ToList();
                        if (parentCats.Contains(input.EntityToProcess.Id))
                        {
                            // Remove the parent category, but still let them create the catgory
                            parentCategoryGuid = null;
                        }
                    }

                    if (parentCategoryGuid != null)
                    {
                        // Set the parent category
                        var parentCategory = _categoryService.Get(parentCategoryGuid.Value);
                        input.EntityToProcess.ParentCategory = parentCategory;

                        // Append the path from the parent category
                        _categoryService.SortPath(input.EntityToProcess, parentCategory);
                    }
                    else
                    {
                        // Must access property (trigger lazy-loading) before we can set it to null (Entity Framework bug!!!)
                        var triggerEfLoad = input.EntityToProcess.ParentCategory;
                        input.EntityToProcess.ParentCategory = null;

                        // Also clear the path
                        input.EntityToProcess.Path = null;
                    }

                    _categoryService.UpdateSlugFromName(input.EntityToProcess);

                    await context.SaveChangesAsync();
                }
                else
                {
                    if (parentCategoryGuid != null)
                    {
                        var parentCategory = await context.Category.FirstOrDefaultAsync(x => x.Id == parentCategoryGuid.Value);
                        input.EntityToProcess.ParentCategory = parentCategory;
                        _categoryService.SortPath(input.EntityToProcess, parentCategory);
                    }


                    // url slug generator
                    input.EntityToProcess.Slug = ServiceHelpers.GenerateSlug(input.EntityToProcess.Name,
                        _categoryService.GetBySlugLike(ServiceHelpers.CreateUrl(input.EntityToProcess.Name)).Select(x => x.Slug).ToList(), null);

                    // Add the category
                    context.Category.Add(input.EntityToProcess);
                }

                await context.SaveChangesAsync();

                _cacheService.ClearStartsWith("CategoryList");

            }
            catch (Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }

            return input;
        }
    }
}

