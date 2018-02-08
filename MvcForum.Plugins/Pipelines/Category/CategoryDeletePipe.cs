namespace MvcForum.Plugins.Pipelines.Category
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class CategoryDeletePipe : IPipe<IPipelineProcess<Category>>
    {
        private readonly ICategoryPermissionForRoleService _categoryPermissionForRoleService;
        private readonly INotificationService _notificationService;
        private readonly ILoggingService _loggingService;
        private readonly ICacheService _cacheService;

        public CategoryDeletePipe(ICategoryPermissionForRoleService categoryPermissionForRoleService, 
            INotificationService notificationService, ILoggingService loggingService, ICacheService cacheService)
        {
            _categoryPermissionForRoleService = categoryPermissionForRoleService;
            _notificationService = notificationService;
            _loggingService = loggingService;
            _cacheService = cacheService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Category>> Process(IPipelineProcess<Category> input,
            IMvcForumContext context)
        {
            _categoryPermissionForRoleService.RefreshContext(context);
            _notificationService.RefreshContext(context);

            try
            {
                // Check if anyone else if using this role
                var okToDelete = !input.EntityToProcess.Topics.Any();

                if (okToDelete)
                {
                    // Get any categorypermissionforoles and delete these first
                    var rolesToDelete = _categoryPermissionForRoleService.GetByCategory(input.EntityToProcess.Id);

                    foreach (var categoryPermissionForRole in rolesToDelete)
                    {
                        _categoryPermissionForRoleService.Delete(categoryPermissionForRole);
                    }

                    var categoryNotificationsToDelete = new List<CategoryNotification>();
                    categoryNotificationsToDelete.AddRange(input.EntityToProcess.CategoryNotifications);
                    foreach (var categoryNotification in categoryNotificationsToDelete)
                    {
                        _notificationService.Delete(categoryNotification);
                    }

                    context.Category.Remove(input.EntityToProcess);

                    await context.SaveChangesAsync();

                    _cacheService.ClearStartsWith("CategoryList");
                }
                else
                {
                    input.AddError($"In use by {input.EntityToProcess.Topics} entities");
                    return input;
                }
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