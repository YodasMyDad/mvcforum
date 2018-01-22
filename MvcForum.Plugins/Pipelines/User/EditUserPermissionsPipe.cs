namespace MvcForum.Plugins.Pipelines.User
{
    using System;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;


    public class EditUserPermissionsPipe : IPipe<IPipelineProcess<MembershipUser>>
    {
        private readonly IRoleService _roleService;
        private readonly IMembershipService _membershipService;
        private readonly ILocalizationService _localizationService;

        public EditUserPermissionsPipe(IRoleService roleService, IMembershipService membershipService, ILocalizationService localizationService)
        {
            _roleService = roleService;
            _membershipService = membershipService;
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public Task<IPipelineProcess<MembershipUser>> Process(IPipelineProcess<MembershipUser> input,
            IMvcForumContext context)
        {
            // Get the Current user from ExtendedData
            var user = input.ExtendedData.GetExtendedDataItem<IPrincipal>(Constants.ExtendedDataKeys.UserObject);

            var loggedOnUser = user.GetMembershipUser(_membershipService);
            var loggedOnUsersRole = loggedOnUser.GetRole(_roleService);
            var loggedOnUserId = loggedOnUser?.Id ?? Guid.Empty;
            var permissions = _roleService.GetPermissions(null, loggedOnUsersRole);

            if (user.IsInRole(Constants.AdminRoleName) || loggedOnUserId == input.EntityToProcess.Id ||
                permissions[ForumConfiguration.Instance.PermissionEditMembers].IsTicked)
            {
                input.Successful = true;
                input.ProcessLog.Add("Completed EditUserPermissionsPipe successfully");
            }
            else
            {
                // No permission
                input.AddError(_localizationService.GetResourceString("Errors.NoPermission"));
            }

            // TODO - Need to avoid doing this
            return Task.Run(() => input); 
        }
    }
}