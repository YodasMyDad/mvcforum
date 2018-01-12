namespace MvcForum.Core.ExtensionMethods
{
    using System.Drawing;
    using System.Net;

    public static class StringExtensionMethods
    {
        /// <summary>
        /// Returns an Image from an exteneral url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Image GetImageFromExternalUrl(this string url)
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(url);

            using (var httpWebReponse = (HttpWebResponse) httpWebRequest.GetResponse())
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
    }
}