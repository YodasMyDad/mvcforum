namespace MvcForum.Plugins.Pipelines.Topic
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Security;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class TopicPermissionsPipe : IPipe<IPipelineProcess<Topic>>
    {
        private readonly IRoleService _roleService;
        private readonly ITopicService _topicService;
        private readonly ILocalizationService _localizationService;
        private readonly ITopicTagService _topicTagService;
        private readonly ILoggingService _loggingService;

        public TopicPermissionsPipe(IRoleService roleService, ITopicService topicService, ILocalizationService localizationService, ITopicTagService topicTagService, ILoggingService loggingService)
        {
            _roleService = roleService;
            _topicService = topicService;
            _localizationService = localizationService;
            _topicTagService = topicTagService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            _roleService.RefreshContext(context);
            _topicService.RefreshContext(context);
            _localizationService.RefreshContext(context);
            _topicTagService.RefreshContext(context);

            try
            {
                // Get the Current user from ExtendedData
                var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;

                // IS this an existing topic
                var existingTopic = await context.Topic.FirstOrDefaultAsync(x => x.Id == input.EntityToProcess.Id);
                if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.IsEdit))
                {
                    input.ExtendedData[Constants.ExtendedDataKeys.IsEdit] = existingTopic != null;
                }
                else
                {
                    input.ExtendedData.Add(Constants.ExtendedDataKeys.IsEdit, existingTopic != null);
                }

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
                        var permissions = _roleService.GetPermissions(input.EntityToProcess.Category, loggedOnUsersRole);
                        input.ExtendedData.Add(Constants.ExtendedDataKeys.PermissionSet, permissions);

                        // Quick check to see if user is locked out, when logged in
                        if (loggedOnUser.IsLockedOut || loggedOnUser.DisablePosting == true ||
                            !loggedOnUser.IsApproved)
                        {
                            FormsAuthentication.SignOut();
                            input.AddError(_localizationService.GetResourceString("Errors.NoAccess"));
                            return input;
                        }

                        // Finally check permissions
                        if (permissions[ForumConfiguration.Instance.PermissionDenyAccess].IsTicked ||
                            permissions[ForumConfiguration.Instance.PermissionReadOnly].IsTicked ||
                            !permissions[ForumConfiguration.Instance.PermissionCreateTopics].IsTicked)
                        {
                            input.AddError(_localizationService.GetResourceString("Errors.NoPermission"));
                            return input;
                        }

                        // Check for moderation on Category
                        if (input.EntityToProcess.Category.ModerateTopics == true)
                        {
                            input.EntityToProcess.Pending = true;
                            input.ExtendedData.Add(Constants.ExtendedDataKeys.Moderate, true);
                        }

                        // Check Is Locked
                        if (existingTopic != null)
                        {
                            // If the locked has changed from original, check they have permission to change it
                            if (existingTopic.IsLocked != input.EntityToProcess.IsLocked &&
                                permissions[ForumConfiguration.Instance.PermissionLockTopics].IsTicked == false)
                            {
                                // Put it back
                                input.EntityToProcess.IsLocked = existingTopic.IsLocked;
                            }
                        }
                        else if (input.EntityToProcess.IsLocked &&
                                 permissions[ForumConfiguration.Instance.PermissionLockTopics].IsTicked == false)
                        {
                            // Put it back as not permission
                            input.EntityToProcess.IsLocked = false;
                        }

                        // Check Sticky
                        if (existingTopic != null)
                        {
                            // If the locked has changed from original, check they have permission to change it
                            if (existingTopic.IsSticky != input.EntityToProcess.IsSticky &&
                                permissions[ForumConfiguration.Instance.PermissionCreateStickyTopics].IsTicked == false)
                            {
                                // Put it back
                                input.EntityToProcess.IsSticky = existingTopic.IsSticky;
                            }
                        }
                        else if (input.EntityToProcess.IsSticky &&
                                 permissions[ForumConfiguration.Instance.PermissionCreateStickyTopics].IsTicked == false)
                        {
                            // Put it back as not permission
                            input.EntityToProcess.IsSticky = false;
                        }

                        // Finally check for a poll
                        if (input.EntityToProcess.Poll != null && input.EntityToProcess.Poll.PollAnswers.Any())
                        {
                            if (permissions[ForumConfiguration.Instance.PermissionCreatePolls].IsTicked == false)
                            {
                                input.AddError(_localizationService.GetResourceString("Errors.NoPermissionPolls"));
                                return input;
                            }
                        }

                        // Sort out tags so we can check permission for any new ones added
                        if (input.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.Tags))
                        {
                            var tags = _topicTagService.CreateTagsFromCsv(input.ExtendedData[Constants.ExtendedDataKeys.Tags] as string);

                            // Add the tags they are allowed to
                            _topicTagService.Add(tags.ToArray(), input.EntityToProcess, permissions[ForumConfiguration.Instance.PermissionCreateTags].IsTicked);
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