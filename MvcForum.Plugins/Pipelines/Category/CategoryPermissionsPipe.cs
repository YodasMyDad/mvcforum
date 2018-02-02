namespace MvcForum.Plugins.Pipelines.Category
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class CategoryPermissionsPipe : IPipe<IPipelineProcess<Category>>
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILoggingService _loggingService;
        private readonly IRoleService _roleService;

        public CategoryPermissionsPipe(IRoleService roleService, ILocalizationService localizationService,
            ILoggingService loggingService)
        {
            _roleService = roleService;
            _localizationService = localizationService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Category>> Process(IPipelineProcess<Category> input,
            IMvcForumContext context)
        {
            _roleService.RefreshContext(context);
            _localizationService.RefreshContext(context);

            try
            {
                // Get the Current user from ExtendedData
                var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;

                // See if we can get the username
                if (!string.IsNullOrWhiteSpace(username))
                {
                    // Get logged on user
                    var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);
                    if (loggedOnUser != null)
                    {
                        // Users role
                        var loggedOnUsersRole = loggedOnUser.GetRole(_roleService);

                        // Get the permissions and add to extendeddata as we'll use it again
                        var permissions = _roleService.GetPermissions(input.EntityToProcess, loggedOnUsersRole);
                        input.ExtendedData.Add(Constants.ExtendedDataKeys.PermissionSet, permissions);

                        // Check user is admin
                        if (!loggedOnUser.IsAdmin())
                        {
                            input.AddError(_localizationService.GetResourceString("Errors.NoPermission"));
                            return input;
                        }
                    }
                    else
                    {
                        input.AddError("Unable to get user from username");
                        return input;
                    }
                }
                else
                {
                    input.AddError("Unable to get username");
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