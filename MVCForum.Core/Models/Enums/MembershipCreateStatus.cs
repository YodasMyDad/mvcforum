namespace MvcForum.Core.Models.Enums
{
    /// <summary>
    ///     Status values returned when creating a user
    /// </summary>
    public enum MembershipCreateStatus
    {
        Success,
        DuplicateUserName,
        DuplicateEmail,
        InvalidPassword,
        InvalidEmail,
        InvalidAnswer,
        InvalidQuestion,
        InvalidUserName,
        ProviderError,
        UserRejected
    }
}