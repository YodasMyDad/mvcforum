namespace MvcForum.Core.Models.Enums
{
    public enum LoginAttemptStatus
    {
        LoginSuccessful,
        UserNotFound,
        PasswordIncorrect,
        PasswordAttemptsExceeded,
        UserLockedOut,
        UserNotApproved,
        Banned
    }
}