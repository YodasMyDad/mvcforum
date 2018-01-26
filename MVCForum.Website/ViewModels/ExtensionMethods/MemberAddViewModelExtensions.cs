namespace MvcForum.Web.ViewModels.ExtensionMethods
{
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Models.Entities;
    using Core.Models.Enums;
    using Member;

    public static partial class MemberAddViewModelExtensions
    {
        /// <summary>
        /// Converts a add view model to a membershipuser
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public static MembershipUser ToMembershipUser(this MemberAddViewModel viewModel)
        {
            var userToSave = new MembershipUser
            {
                UserName = viewModel.UserName,
                Email = viewModel.Email,
                Password = viewModel.Password,
                IsApproved = viewModel.IsApproved,
                Comment = viewModel.Comment
            };

            if (viewModel.LoginType == LoginType.Facebook)
            {
                userToSave.FacebookAccessToken = viewModel.UserAccessToken;
            }
            if (viewModel.LoginType == LoginType.Google)
            {
                userToSave.GoogleAccessToken = viewModel.UserAccessToken;
            }
            if (viewModel.LoginType == LoginType.Microsoft)
            {
                userToSave.MicrosoftAccessToken = viewModel.UserAccessToken;
            }

            // Save the social url
            if (!string.IsNullOrWhiteSpace(viewModel.SocialProfileImageUrl))
            {
                // Save the SocialProfileImageUrl in ExtendedData as we'll need it
                userToSave.SetExtendedDataValue(Constants.ExtendedDataKeys.SocialProfileImageUrl, viewModel.SocialProfileImageUrl);
            }

            // Save the return url on the user to
            if (!string.IsNullOrWhiteSpace(viewModel.ReturnUrl))
            {
                userToSave.SetExtendedDataValue(Constants.ExtendedDataKeys.ReturnUrl, viewModel.ReturnUrl);
            }

            return userToSave;
        }
    }
}