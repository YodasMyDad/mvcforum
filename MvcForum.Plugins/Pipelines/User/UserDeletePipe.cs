namespace MvcForum.Plugins.Pipelines.User
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;

    public class UserDeletePipe : IPipe<IPipelineProcess<MembershipUser>>
    {
        private readonly ILocalizationService _localizationService;
        private readonly IMembershipService _membershipService;
        private readonly ILoggingService _loggingService;

        public UserDeletePipe(IMembershipService membershipService, ILoggingService loggingService, ILocalizationService localizationService)
        {
            _membershipService = membershipService;
            _loggingService = loggingService;
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<MembershipUser>> Process(IPipelineProcess<MembershipUser> input,
            IMvcForumContext context)
        {
            _membershipService.RefreshContext(context);
            _localizationService.RefreshContext(context);

            try
            {
                // Scrub all member data
                var scrubResult = await _membershipService.ScrubUsers(input.EntityToProcess);
                if (!scrubResult.Successful)
                {
                    input.AddError(scrubResult.ProcessLog.FirstOrDefault());
                    return input;
                }

                // Just clear the roles, don't delete them
                input.EntityToProcess.Roles.Clear();

                // Now delete the member
                context.MembershipUser.Remove(input.EntityToProcess);

                var saved = await context.SaveChangesAsync();
                if (saved <= 0)
                {
                    // Nothing saved
                    input.AddError(_localizationService.GetResourceString("Errors.GenericMessage"));
                }
                else
                {
                    input.ProcessLog.Add("Member deleted successfully");
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
                input.AddError(ex.Message);
            }
            
            
            return input;
        }
    }
}