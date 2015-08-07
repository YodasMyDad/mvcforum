using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;

namespace MVCForum.Website.Akismet.NET
{
    /// <summary>
    /// This class is responsible validating information against Akismet.
    /// </summary>
    /// <example>
    ///     Comment comment = new Comment
    ///     {
    ///         blog = "Your-Akismet-Domain",
    ///         comment_type = "comment",
    ///         comment_author = "Sam Mulder",
    ///         comment_author_email = "samm@gmail.com",
    ///         comment_content = "Does this really working?",
    ///         permalink = String.Empty,
    ///         referrer = httpContext.Request.ServerVariables["HTTP_REFERER"],
    ///         user_agent = httpContext.Request.ServerVariables["HTTP_USER_AGENT"],
    ///         user_ip = httpContext.Request.ServerVariables["REMOTE_ADDR"]
    ///     };
    /// 
    ///     Validator validator = new Validator("Your-Akismet-Key");
    ///     if(validator.IsSpam(comment))
    ///     { // do something with the spam comment
    ///     }
    ///     else
    ///     { // this comment is not spam
    ///     }
    /// </example>
    public class Validator : IValidator
    {
        #region Class members

        /// <summary>
        /// The Akismet key, if any.
        /// </summary>
        protected String m_key = String.Empty;
        
        #endregion

        #region Class constructors
        
        /// <summary>
        /// Initialize class members based on the input parameters
        /// </summary>
        /// <param name="key">The input Akismet key.</param>
        public Validator(String key)
        {
            m_key = key;
        }

        #endregion

        #region IValidator implementation

        /// <summary>
        /// Check if the validator's key is valid or not.
        /// </summary>
        /// <returns>True if the key is valid, false otherwise.</returns>
        public Boolean VerifyKey(String domain)
        {
            // prepare pars for the request
            NameValueCollection pars = PreparePars(m_key, domain);
            if (null != pars)
            {
                // extract result from the request
                return ExtractResult(PostRequest("http://rest.akismet.com/1.1/verify-key", pars));
            }

            // return failure
            return false;
        }

        /// <summary>
        /// Check if the input comment is valid or not.
        /// </summary>
        /// <param name="comment">The input comment to be checked.</param>
        /// <returns>True if the comment is valid, false otherwise.</returns>
        public Boolean IsSpam(Comment comment)
        {
            // prepare pars for the request
            NameValueCollection pars = PreparePars(comment);
            if (null != pars)
            {
                // extract result from the request
                return ExtractResult(PostRequest(String.Format("http://{0}.rest.akismet.com/1.1/comment-check", m_key), pars));
            }

            // return no spam
            return false;
        }

        /// <summary>
        /// This call is for submitting comments that weren't marked as spam but should've been.
        /// </summary>
        /// <param name="comment">The input comment to be sent as spam.</param>
        /// <returns>True if the comment was successfully sent, false otherwise.</returns>
        public void SubmitSpam(Comment comment)
        {
            // prepare pars for the request
            NameValueCollection pars = PreparePars(comment);
            if (null != pars)
            {
                PostRequest(String.Format("http://{0}.rest.akismet.com/1.1/submit-spam", m_key), pars);
            }
        }

        /// <summary>
        /// This call is intended for the marking of false positives, things that were incorrectly marked as spam.
        /// </summary>
        /// <param name="comment">The input comment to be sent as ham.</param>
        /// <returns>True if the comment was successfully sent, false otherwise.</returns>
        public void SubmitHam(Comment comment)
        {
            // prepare pars for the request
            NameValueCollection pars = PreparePars(comment);
            if (null != pars)
            {
                PostRequest(String.Format("http://{0}.rest.akismet.com/1.1/submit-spam", m_key), pars);
            }
        }
       
        #endregion

        #region Class operations
        
        /// <summary>
        /// Post parameters to the input url and return the response.
        /// </summary>
        /// <param name="url">The input url (absolute).</param>
        /// <param name="pars">The input parameters to send.</param>
        /// <returns>The response, if any.</returns>
        protected virtual String PostRequest(String url, NameValueCollection pars)
        {
            // check input data
            if (String.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute) || (null == pars))
                return String.Empty;

            String content = String.Empty;
            // create content for the post request
            foreach (String key in pars.AllKeys)
            {
                if (String.IsNullOrEmpty(content))
                    content = String.Format("{0}={1}", key, pars[key]);
                else
                    content += String.Format("&{0}={1}", key, pars[key]);
            }

            // initialize request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentLength = content.Length;
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.UserAgent = "Akismet.NET";

            StreamWriter writer = null;
            try
            {
                // write request content
                writer = new StreamWriter(request.GetRequestStream());
                writer.Write(content);
            }
            catch (Exception)
            { // return failure
                return String.Empty;
            }
            finally
            { // close the writer, if any
                if (null != writer)
                    writer.Close();
            }

            // retrieve the response
            var response = (HttpWebResponse)request.GetResponse();
            using (var reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8, true))
            {
                // retrieve response
                String result = reader.ReadToEnd();

                // close the reader
                reader.Close();

                // return result
                return result;
            }
        }

        /// <summary>
        /// Prepare request parameters based on the input comment.
        /// </summary>
        /// <param name="comment">The input comment.</param>
        /// <returns>The prepared parameters if any.</returns>
        protected virtual NameValueCollection PreparePars(Comment comment)
        {
            // check the input parameters
            if ((null != comment) && (!comment.IsValid))
                return null;

            // initialize result
            var result = new NameValueCollection();

            // add required information
            result["blog"] = HttpUtility.UrlEncode(comment.blog);
            result["user_ip"] = HttpUtility.UrlEncode(comment.user_ip);
            result["user_agent"] = HttpUtility.UrlEncode(comment.user_agent);
            // add optional information
            result["referrer"] = String.IsNullOrEmpty(comment.referrer) ? String.Empty : HttpUtility.UrlEncode(comment.referrer);
            result["permalink"] = String.IsNullOrEmpty(comment.permalink) ? String.Empty : HttpUtility.UrlEncode(comment.permalink);
            result["comment_type"] = String.IsNullOrEmpty(comment.comment_type) ? String.Empty : HttpUtility.UrlEncode(comment.comment_type);
            result["comment_author"] = String.IsNullOrEmpty(comment.comment_author) ? String.Empty : HttpUtility.UrlEncode(comment.comment_author);
            result["comment_author_email"] = String.IsNullOrEmpty(comment.comment_author_email) ? String.Empty : HttpUtility.UrlEncode(comment.comment_author_email);
            result["comment_author_url"] = String.IsNullOrEmpty(comment.comment_author_url) ? String.Empty : HttpUtility.UrlEncode(comment.comment_author_url);
            result["comment_content"] = String.IsNullOrEmpty(comment.comment_content) ? String.Empty : HttpUtility.UrlEncode(comment.comment_content);

            // return result
            return result;
        }

        /// <summary>
        /// Prepare request parameters based on the input parameters.
        /// </summary>
        /// <param name="key">The input key.</param>
        /// <param name="domain">The input domain.</param>
        /// <returns>The prepared parameters if any.</returns>
        protected virtual NameValueCollection PreparePars(String key, String domain)
        {
            // check the input parameters
            if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(domain))
                return null;

            // initialize result
            NameValueCollection result = new NameValueCollection();

            // add required information
            result["key"] = key; // no need for encoding
            result["blog"] = HttpUtility.UrlEncode(domain);
            
            // return result
            return result;
        }

        /// <summary>
        /// Check the input data for valid content: "valid" string or "true" string.
        /// </summary>
        /// <param name="content">The input content.</param>
        /// <returns>True if the content is valid, false otherwise.</returns>
        protected virtual Boolean ExtractResult(String content)
        {
            // check the input parameters
            if (String.IsNullOrEmpty(content))
                return false;

            // check for valid content
            if (content.ToLower().Equals("valid") || content.ToLower().Equals("true"))
                return true;

            // return failure
            return false;
        }

        #endregion
    }
}
