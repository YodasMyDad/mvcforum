namespace MvcForum.Web.ViewModels.ExtensionMethods
{
    using Core.Models.Entities;
    using Member;

    /// <summary>
    /// Extension methods for the member edit view model
    /// </summary>
    public static partial class MemberFrontEndEditViewModelExtensions
    {
        /// <summary>
        /// Creates a MembershipUser from the view model
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static MembershipUser ToMembershipUser(this MemberFrontEndEditViewModel viewModel, MembershipUser user)
        {
            user.Id = viewModel.Id;
            user.UserName = viewModel.UserName;
            user.Email = viewModel.Email;
            user.Signature = viewModel.Signature;
            user.Age = viewModel.Age;
            user.Location = viewModel.Location;
            user.Website = viewModel.Website;
            user.Twitter = viewModel.Twitter;
            user.Facebook = viewModel.Facebook;
            if (!string.IsNullOrWhiteSpace(viewModel.Avatar))
            {
                user.Avatar = viewModel.Avatar;
            }           
            user.DisableEmailNotifications = viewModel.DisableEmailNotifications;      
            return user;
        }

        /// <summary>
        ///     Creates view model
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static MemberFrontEndEditViewModel PopulateMemberViewModel(this MembershipUser user)
        {
            var viewModel = new MemberFrontEndEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Signature = user.Signature,
                Age = user.Age,
                Location = user.Location,
                Website = user.Website,
                Twitter = user.Twitter,
                Facebook = user.Facebook,
                DisableFileUploads = user.DisableFileUploads == true,
                Avatar = user.Avatar,
                DisableEmailNotifications = user.DisableEmailNotifications == true,
                AmountOfPoints = user.TotalPoints
            };
            return viewModel;
        }
    }
}