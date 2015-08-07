using System;

namespace MVCForum.Website.Akismet.NET
{
    /// <summary>
    /// It describes the public interface for the <see cref="Validator"/> class.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// Check if the validator's key is valid or not.
        /// </summary>
        /// <returns>True if the key is valid, false otherwise.</returns>
        Boolean VerifyKey(String domain);

        /// <summary>
        /// Check if the input comment is valid or not.
        /// </summary>
        /// <param name="comment">The input comment to be checked.</param>
        /// <returns>True if the comment is valid, false otherwise.</returns>
        Boolean IsSpam(Comment comment);

        /// <summary>
        /// Submit the input comment as spam.
        /// </summary>
        /// <param name="comment">The input comment to be sent as spam.</param>
        /// <returns>True if the comment was successfully sent, false otherwise.</returns>
        void SubmitSpam(Comment comment);

        /// <summary>
        /// Submit the input comment as ham.
        /// </summary>
        /// <param name="comment">The input comment to be sent as ham.</param>
        /// <returns>True if the comment was successfully sent, false otherwise.</returns>
        void SubmitHam(Comment comment);
    }
}
