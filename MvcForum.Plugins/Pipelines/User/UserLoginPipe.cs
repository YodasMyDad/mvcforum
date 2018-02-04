namespace MvcForum.Plugins.Pipelines.User
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Web.Security;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.Enums;

    /// <summary>
    /// Pipe to login a member
    /// </summary>
    public class UserLoginPipe : IPipe<IPipelineProcess<MembershipUser>>
    {
        private readonly ILocalizationService _localizationService;
        private readonly IMembershipService _membershipService;
        private readonly ILoggingService _loggingService;

        public UserLoginPipe(ILocalizationService localizationService, IMembershipService membershipService, ILoggingService loggingService)
        {
            _localizationService = localizationService;
            _membershipService = membershipService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<MembershipUser>> Process(IPipelineProcess<MembershipUser> input,
            IMvcForumContext context)
        {
           _localizationService.RefreshContext(context);
            _membershipService.RefreshContext(context);

            try
            {
                var username = input.ExtendedData[Constants.ExtendedDataKeys.Username] as string;
                var password = input.ExtendedData[Constants.ExtendedDataKeys.Password] as string;

                // Validate login
                if (_membershipService.ValidateUser(username, password, Membership.MaxInvalidPasswordAttempts))
                {
                    // Set last login date as users details are ok
                    input.EntityToProcess = await context.MembershipUser.FirstOrDefaultAsync(x => x.UserName == username);

                    if (input.EntityToProcess.IsApproved && !input.EntityToProcess.IsLockedOut &&
                        !input.EntityToProcess.IsBanned)
                    {
                        input.EntityToProcess.LastLoginDate = DateTime.UtcNow;
                        input.Successful = true;
                        return input;
                    }

                    input.Successful = false;
                    input.ProcessLog.Clear();

                    if (input.EntityToProcess.IsLockedOut)
                    {
                        input.ProcessLog.Add(_localizationService.GetResourceString("Members.Errors.UserLockedOut"));
                    }
                    if (!input.EntityToProcess.IsApproved)
                    {
                        input.ProcessLog.Add(_localizationService.GetResourceString("Members.Errors.UserNotApproved"));
                    }
                    if (input.EntityToProcess.IsBanned)
                    {
                        input.ProcessLog.Add(_localizationService.GetResourceString("Members.NowBanned"));
                    }
                }
                else
                {
                    // get here Login failed, check the login status
                    var loginStatus = _membershipService.LastLoginStatus;

                    input.Successful = false;
                    input.ProcessLog.Clear();

                    switch (loginStatus)
                    {
                        case LoginAttemptStatus.UserNotFound:
                        case LoginAttemptStatus.PasswordIncorrect:
                            input.ProcessLog.Add(_localizationService.GetResourceString("Members.Errors.PasswordIncorrect"));
                            break;

                        case LoginAttemptStatus.PasswordAttemptsExceeded:
                            input.ProcessLog.Add(_localizationService.GetResourceString("Members.Errors.PasswordAttemptsExceeded"));
                            break;

                        case LoginAttemptStatus.UserLockedOut:
                            input.ProcessLog.Add(_localizationService.GetResourceString("Members.Errors.UserLockedOut"));
                            break;

                        case LoginAttemptStatus.Banned:
                            input.ProcessLog.Add(_localizationService.GetResourceString("Members.NowBanned"));
                            break;

                        case LoginAttemptStatus.UserNotApproved:
                            input.ProcessLog.Add(_localizationService.GetResourceString("Members.Errors.UserNotApproved"));
                            break;

                        default:
                            input.ProcessLog.Add(_localizationService.GetResourceString("Members.Errors.LogonGeneric"));
                            break;
                    }
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