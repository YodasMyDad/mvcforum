namespace MvcForum.Plugins.Pipelines.User
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models;
    using Core.Models.Entities;
    using Core.Models.Enums;

    public class UserCreatePipe : IPipe<IPipelineProcess<MembershipUser>>
    {
        private readonly IActivityService _activityService;
        private readonly IEmailService _emailService;
        private readonly ILocalizationService _localizationService;
        private readonly ILoggingService _loggingService;
        private readonly IMembershipService _membershipService;
        private readonly ISettingsService _settingsService;

        public UserCreatePipe(IMembershipService membershipService, ILoggingService loggingService,
            ISettingsService settingsService, ILocalizationService localizationService, IEmailService emailService,
            IActivityService activityService)
        {
            _membershipService = membershipService;
            _loggingService = loggingService;
            _settingsService = settingsService;
            _localizationService = localizationService;
            _emailService = emailService;
            _activityService = activityService;
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<MembershipUser>> Process(IPipelineProcess<MembershipUser> input,
            IMvcForumContext context)
        {
            _membershipService.RefreshContext(context);
            _settingsService.RefreshContext(context);
            _localizationService.RefreshContext(context);
            _emailService.RefreshContext(context);
            _activityService.RefreshContext(context);

            try
            {
                if (string.IsNullOrWhiteSpace(input.EntityToProcess.UserName))
                {
                    input.ProcessLog.Clear();
                    input.ProcessLog.Add(_membershipService.ErrorCodeToString(MembershipCreateStatus.InvalidUserName));
                    input.Successful = false;
                    return input;
                }

                // get by username
                if (_membershipService.GetUser(input.EntityToProcess.UserName, true) != null)
                {
                    input.ProcessLog.Clear();
                    input.ProcessLog.Add(_membershipService.ErrorCodeToString(MembershipCreateStatus.DuplicateUserName));
                    input.Successful = false;
                    return input;
                }

                // Add get by email address
                if (_membershipService.GetUserByEmail(input.EntityToProcess.Email, true) != null)
                {
                    input.ProcessLog.Clear();
                    input.ProcessLog.Add(_membershipService.ErrorCodeToString(MembershipCreateStatus.DuplicateEmail));
                    input.Successful = false;
                    return input;
                }

                if (string.IsNullOrWhiteSpace(input.EntityToProcess.Password))
                {
                    input.ProcessLog.Clear();
                    input.ProcessLog.Add(_membershipService.ErrorCodeToString(MembershipCreateStatus.InvalidPassword));
                    input.Successful = false;
                    return input;
                }

                // Get the settings
                var settings = _settingsService.GetSettings(false);
                var manuallyAuthoriseMembers = settings.ManuallyAuthoriseNewMembers;
                var memberEmailAuthorisationNeeded = settings.NewMemberEmailConfirmation == true;

                // Check social login
                var isSocialLogin = !string.IsNullOrWhiteSpace(input.EntityToProcess.FacebookAccessToken) ||
                                    !string.IsNullOrWhiteSpace(input.EntityToProcess.GoogleAccessToken) ||
                                    !string.IsNullOrWhiteSpace(input.EntityToProcess.MicrosoftAccessToken);

                // If this is a social login, and memberEmailAuthorisationNeeded is true then we need to ignore it
                // and set memberEmailAuthorisationNeeded to false because the email addresses are validated by the social media providers
                if (isSocialLogin && manuallyAuthoriseMembers == false)
                {
                    memberEmailAuthorisationNeeded = false;
                    input.EntityToProcess.IsApproved = true;
                }
                else if (manuallyAuthoriseMembers || memberEmailAuthorisationNeeded)
                {
                    input.EntityToProcess.IsApproved = false;
                }
                else
                {
                    input.EntityToProcess.IsApproved = true;
                }

                // See if this is a social login and we have their profile pic
                var socialProfileImageUrl =
                    input.EntityToProcess.GetExtendedDataItem(Constants.ExtendedDataKeys.SocialProfileImageUrl);
                if (!string.IsNullOrWhiteSpace(socialProfileImageUrl))
                {
                    // We have an image url - Need to save it to their profile
                    var image = socialProfileImageUrl.GetImageFromExternalUrl();

                    // Set upload directory - Create if it doesn't exist
                    var uploadFolderPath =
                        HostingEnvironment.MapPath(string.Concat(ForumConfiguration.Instance.UploadFolderPath,
                            input.EntityToProcess.Id));
                    if (uploadFolderPath != null && !Directory.Exists(uploadFolderPath))
                    {
                        Directory.CreateDirectory(uploadFolderPath);
                    }

                    // Get the file name
                    var fileName = Path.GetFileName(socialProfileImageUrl);

                    // Create a HttpPostedFileBase image from the C# Image
                    using (var stream = new MemoryStream())
                    {
                        // Microsoft doesn't give you a file extension - See if it has a file extension
                        // Get the file extension
                        var fileExtension = Path.GetExtension(fileName);

                        // Fix invalid Illegal charactors
                        var regexSearch =
                            $"{new string(Path.GetInvalidFileNameChars())}{new string(Path.GetInvalidPathChars())}";
                        var reg = new Regex($"[{Regex.Escape(regexSearch)}]");
                        fileName = reg.Replace(fileName, "");

                        if (string.IsNullOrWhiteSpace(fileExtension))
                        {
                            // no file extension so give it one
                            fileName = string.Concat(fileName, ".jpg");
                        }

                        image.Save(stream, ImageFormat.Jpeg);
                        stream.Position = 0;
                        HttpPostedFileBase formattedImage = new MemoryFile(stream, "image/jpeg", fileName);

                        // Upload the file
                        var uploadResult = formattedImage.UploadFile(uploadFolderPath, _localizationService, true);

                        // Don't throw error if problem saving avatar, just don't save it.
                        if (uploadResult.UploadSuccessful)
                        {
                            input.EntityToProcess.Avatar = uploadResult.UploadedFileName;
                        }
                    }
                }

                input.EntityToProcess = context.MembershipUser.Add(input.EntityToProcess);
                var saved = await context.SaveChangesAsync();
                if (saved <= 0)
                {
                    input.ProcessLog.Add("Unable to save changes to the database");
                    input.Successful = false;
                    return input;
                }

                if (settings.EmailAdminOnNewMemberSignUp)
                {
                    var sb = new StringBuilder();
                    sb.Append(
                        $"<p>{string.Format(_localizationService.GetResourceString("Members.NewMemberRegistered"), settings.ForumName, settings.ForumUrl)}</p>");
                    sb.Append($"<p>{input.EntityToProcess.UserName} - {input.EntityToProcess.Email}</p>");
                    var email = new Email
                    {
                        EmailTo = settings.AdminEmailAddress,
                        NameTo = _localizationService.GetResourceString("Members.Admin"),
                        Subject = _localizationService.GetResourceString("Members.NewMemberSubject")
                    };
                    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                    _emailService.SendMail(email);
                }

                // Only send the email if the admin is not manually authorising emails or it's pointless
                _emailService.SendEmailConfirmationEmail(input.EntityToProcess, manuallyAuthoriseMembers,
                    memberEmailAuthorisationNeeded);

                // Now add a memberjoined activity
                _activityService.MemberJoined(input.EntityToProcess);

                // Set manuallyAuthoriseMembers, memberEmailAuthorisationNeeded in extendeddata
                input.ExtendedData.Add(Constants.ExtendedDataKeys.ManuallyAuthoriseMembers, manuallyAuthoriseMembers);
                input.ExtendedData.Add(Constants.ExtendedDataKeys.MemberEmailAuthorisationNeeded, memberEmailAuthorisationNeeded);
            }
            catch (Exception ex)
            {
                input.AddError(ex.Message);
                _loggingService.Error(ex);
            }

            if (input.Successful)
            {
                input.ProcessLog.Add("CreateNewUserPipe Successful");
            }

            return input;
        }
    }
}