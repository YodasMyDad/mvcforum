namespace MvcForum.Core.ExtensionMethods
{
    using System;
    using System.Drawing;
    using System.Net;
    using System.Text;
    using Utilities;

    public static class StringExtensionMethods
    {
        /// <summary>
        ///     Returns an Image from an exteneral url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Image GetImageFromExternalUrl(this string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            using (var httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (var stream = httpWebReponse.GetResponseStream())
                {
                    if (stream != null)
                    {
                        return Image.FromStream(stream);
                    }
                }
            }
            return null;
        }

        /// <summary>
        ///     Turns a string to an array
        /// </summary>
        /// <param name="value"></param>
        /// <param name="charactorToSplit"></param>
        /// <param name="lowerCase"></param>
        /// <returns></returns>
        public static string[] ToArray(this string value, char charactorToSplit = ',', bool lowerCase = false)
        {
            value = value.ToLower().Trim()
                .TrimStart(',').TrimEnd(',');

            if (lowerCase)
            {
                value = value.ToLower();
            }

            return value.Split(',');
        }

        /// <summary>
        /// Appends a unique identifier to a string followed by a hyphen
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string AppendUniqueIdentifier(this string value, int length = 8)
        {
            return $"{GetBase36(length).ToLower()}-{value}";
        }

        /// <summary>
        /// Creates a new file name which has hyphens and a unique identifier
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="appendUniqueIdentifier"></param>
        /// <returns></returns>
        public static string CreateFilename(this string filename, bool appendUniqueIdentifier = true)
        {
            filename = filename.Trim(' ').Replace("_", "-").Replace(" ", "-").ToLower();
            return appendUniqueIdentifier ? filename.AppendUniqueIdentifier() : filename;
        }

        /// <summary>
        /// Formats post content
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public static string ConvertPostContent(this string post)
        {
            if (!string.IsNullOrWhiteSpace(post))
            {
                // Convert any BBCode
                //NOTE: Decided to remove BB code
                //post = StringUtils.ConvertBbCodeToHtml(post, false);

                // If using the PageDown/MarkDown Editor uncomment this line
                post = StringUtils.ConvertMarkDown(post);

                // Allow video embeds
                post = StringUtils.EmbedVideosInPosts(post);

                // Add Google prettify code snippets
                post = post.Replace("<pre>", "<pre class='prettyprint'>");
            }

            return post;
        }

        #region Private Methods

        private static readonly char[] Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly Random Random = new Random();

        private static string GetBase62(int length)
        {
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                sb.Append(Base62Chars[Random.Next(62)]);

            return sb.ToString();
        }

        private static string GetBase36(int length)
        {
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
                sb.Append(Base62Chars[Random.Next(36)]);

            return sb.ToString();
        } 
        #endregion

    }
}