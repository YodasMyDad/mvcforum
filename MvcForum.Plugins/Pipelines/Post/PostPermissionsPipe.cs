namespace MvcForum.Plugins.Pipelines.Post
{
    using System.Data.Entity;
    using System.Threading.Tasks;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class PostPermissionsPipe : IPipe<IPipelineProcess<Post>>
    {
        private readonly IRoleService _roleService;
        private readonly ILocalizationService _localizationService;
        private readonly ILoggingService _loggingService;

        public PostPermissionsPipe(IRoleService roleService, ILocalizationService localizationService, ILoggingService loggingService)
        {
            _roleService = roleService;
            _localizationService = localizationService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Post>> Process(IPipelineProcess<Post> input, IMvcForumContext context)
        {
            _roleService.RefreshContext(context);
            _localizationService.RefreshContext(context);

            try
            {
                // Get the Current user from ExtendedData
                var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;

                // IS this an existing topic
                var existingPost = await context.Post.Include(x => x.User)
                                    .FirstOrDefaultAsync(x => x.Id == input.EntityToProcess.Id);

                input.ExtendedData.Add(Constants.ExtendedDataKeys.IsEdit, existingPost != null);

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
                        var permissions = _roleService.GetPermissions(input.EntityToProcess.Topic.Category, loggedOnUsersRole);
                        input.ExtendedData.Add(Constants.ExtendedDataKeys.PermissionSet, permissions);

                        // Check this users role has permission to create a post
                        if (permissions[ForumConfiguration.Instance.PermissionDenyAccess].IsTicked || permissions[ForumConfiguration.Instance.PermissionReadOnly].IsTicked)
                        {
                            input.AddError(_localizationService.GetResourceString("Errors.NoPermission"));
                            return input;
                        }

                        // Files? Check and then check permissions
                        if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.PostedFiles))
                        {
                            if (permissions[ForumConfiguration.Instance.PermissionAttachFiles].IsTicked == false ||
                                loggedOnUser.DisableFileUploads == true)
                            {
                                // Not allowed to upload files
                                input.AddError(_localizationService.GetResourceString("Errors.NoPermission"));
                                return input;
                            }
                        }

                        // What if this is an edit
                        if (existingPost != null)
                        {
                            if (existingPost.User.Id != loggedOnUser.Id &&
                                !permissions[ForumConfiguration.Instance.PermissionEditPosts].IsTicked)
                            {
                                // Not allowed to edit this post
                                input.AddError(_localizationService.GetResourceString("Errors.NoPermission"));
                                return input;
                            }
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
            catch (System.Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }
            return input;
        }
    }
}