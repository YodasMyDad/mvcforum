namespace MvcForum.Plugins.Pipelines.User
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;


    public class UserEditPermissionsPipe : IPipe<IPipelineProcess<MembershipUser>>
    {
        private readonly IRoleService _roleService;
        private readonly ILocalizationService _localizationService;

        public UserEditPermissionsPipe(IRoleService roleService, ILocalizationService localizationService)
        {
            _roleService = roleService;
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<MembershipUser>> Process(IPipelineProcess<MembershipUser> input,
            IMvcForumContext context)
        {
            _roleService.RefreshContext(context);
            _localizationService.RefreshContext(context);

            // Get the Current user from ExtendedData
            var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;

            // See if we can get the username
            if (!string.IsNullOrWhiteSpace(username))
            {
                var loggedOnUser = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);
                var loggedOnUsersRole = loggedOnUser.GetRole(_roleService);
                var loggedOnUserId = loggedOnUser?.Id ?? Guid.Empty;
                var permissions = _roleService.GetPermissions(null, loggedOnUsersRole);

                if (loggedOnUsersRole.RoleName == Constants.AdminRoleName || loggedOnUserId == input.EntityToProcess.Id ||
                    permissions[ForumConfiguration.Instance.PermissionEditMembers].IsTicked)
                {
                    input.Successful = true;
                    input.ProcessLog.Add("Completed EditUserPermissionsPipe successfully");
                    return input;
                }
            }

            // No permission
            input.AddError(_localizationService.GetResourceString("Errors.NoPermission"));

            return input;
        }
    }
}