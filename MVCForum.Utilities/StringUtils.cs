using System;
using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Security.Application;

namespace MVCForum.Utilities
{
    public static class StringUtils
    {

        #region Extension Methods
            /// <summary>
            /// Checks whether the string is Null Or Empty
            /// </summary>
            /// <param name="theInput"></param>
            /// <returns></returns>
            public static bool IsNullEmpty(this string theInput)
            {
                return string.IsNullOrEmpty(theInput);
            }
            
            /// <summary>
            /// Converts the string to Int32
            /// </summary>
            /// <param name="theInput"></param>
            /// <returns></returns>
            public static int ToInt32(this string theInput)
            {
                return !string.IsNullOrEmpty(theInput) ? Convert.ToInt32(theInput) : 0;
            }
            
            /// <summary>
            /// Removes all line breaks from a string
            /// </summary>
            /// <param name="lines"></param>
            /// <returns></returns>
            public static string RemoveLineBreaks(this string lines)
            {
                return lines.Replace("\r\n", "")
                            .Replace("\r", "")
                            .Replace("\n", "");
            }
            
            /// <summary>
            /// Removes all line breaks from a string and replaces them with specified replacement
            /// </summary>
            /// <param name="lines"></param>
            /// <param name="replacement"></param>
            /// <returns></returns>
            public static string ReplaceLineBreaks(this string lines, string replacement)
            {
                return lines.Replace(Environment.NewLine, replacement);
            }
            
            /// <summary>
            /// Does a case insensitive contains
            /// </summary>
            /// <param name="source"></param>
            /// <param name="value"></param>
            /// <returns></returns>
            public static bool ContainsCaseInsensitive(this string source, string value)
            {
                var results = source.IndexOf(value, StringComparison.CurrentCultureIgnoreCase);
                return results != -1;
            }
        #endregion

        #region Social Helpers
        public static string GetGravatarImage(string email, int size)
        {
            return IsValidEmail(email) ? string.Format("http://www.gravatar.com/avatar/{0}?s={1}", md5HashString(email), size) : "";
        }
        #endregion

        public static string md5HashString(string toHash)
        {
            // Create a new instance of the MD5CryptoServiceProvider object.
            var md5Hasher = MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(toHash));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();  // Return the hexadecimal string.
        }

        /// <summary>
        /// Returns the number of occurances of one string within another
        /// </summary>
        /// <param name="text"></param>
        /// <param name="stringToFind"></param>
        /// <returns></returns>
        public static int NumberOfOccurrences(string text, string stringToFind)
        {
            if (text == null || stringToFind == null)
            {
                return 0;
            }

            var reg = new Regex(stringToFind, RegexOptions.IgnoreCase);

            return reg.Matches(text).Count;
        }

        /// <summary>
        /// Checks to see if the string passed in is a valid email address
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        public static bool IsValidEmail(string strIn)
        {
            if (strIn.IsNullEmpty())
            {
                return false;
            }

            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn,
                   @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$");
        }


        /// <summary>
        /// Strips numeric charators from a string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string StripNonNumerics(string source)
        {
            var digitRegex = new Regex(@"[^\d]");
            return digitRegex.Replace(source, "");
        }

        /// <summary>
        /// Downloads a web page and returns the HTML as a string
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpWebResponse DownloadWebPage(string url)
        {
            var ub = new UriBuilder(url);
            var request = (HttpWebRequest)WebRequest.Create(ub.Uri);
            request.Proxy = null;
            return (HttpWebResponse)request.GetResponse();
        }

        /// <summary>
        /// Checks to see if the object is numeric or not
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static bool IsNumeric(object expression)
        {
            double retNum;
            var isNum = Double.TryParse(Convert.ToString(expression), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        /// <summary>
        /// reverses a string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringReverse(string str)
        {
            var len = str.Length;
            var arr = new char[len];
            for (var i = 0; i < len; i++)
            {
                arr[i] = str[len - 1 - i];
            }
            return new string(arr);
        }

        /// <summary>
        /// Returns a capitalised version of words in the string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CapitalizeWords(string value)
        {
            if (value == null)
                return null;
            if (value.Length == 0)
                return value;

            var result = new StringBuilder(value);
            result[0] = char.ToUpper(result[0]);
            for (var i = 1; i < result.Length; ++i)
            {
                if (char.IsWhiteSpace(result[i - 1]))
                    result[i] = char.ToUpper(result[i]);
                else
                    result[i] = char.ToLower(result[i]);
            }
            return result.ToString();
        }


        /// <summary>
        /// Returns the amount of individual words in a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int CountWordsInString(string text)
        {
            if (string.IsNullOrEmpty(text))
            { return 0; }
            var tmpStr = text.Replace("\t", " ").Trim();
            tmpStr = tmpStr.Replace("\n", " ");
            tmpStr = tmpStr.Replace("\r", " ");
            while (tmpStr.IndexOf("  ") != -1)
                tmpStr = tmpStr.Replace("  ", " ");
            return tmpStr.Split(' ').Length;
        }

        /// <summary>
        /// Returns a specified amount of words from a string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="wordAmount"></param>
        /// <returns></returns>
        public static string ReturnAmountWordsFromString(string text, int wordAmount)
        {
            string tmpStr;
            string[] stringArray;
            var tmpStrReturn = "";
            tmpStr = text.Replace("\t", " ").Trim();
            tmpStr = tmpStr.Replace("\n", " ");
            tmpStr = tmpStr.Replace("\r", " ");

            while (tmpStr.IndexOf("  ") != -1)
            {
                tmpStr = tmpStr.Replace("  ", " ");
            }
            stringArray = tmpStr.Split(' ');

            if (stringArray.Length < wordAmount)
            {
                wordAmount = stringArray.Length;
            }
            for (int i = 0; i < wordAmount; i++)
            {
                tmpStrReturn += stringArray[i] + " ";
            }
            return tmpStrReturn;
        }

        /// <summary>
        /// Returns XSS safe HTML using XSS library
        /// </summary>
        /// <param name="html"></param>
        /// <param name="keepLineBreaks"> </param>
        /// <returns></returns>
        public static string GetSafeHtml(string html, bool keepLineBreaks = false)
        {
            if (string.IsNullOrEmpty(html)) return html;

            if (keepLineBreaks)
            {

                html = Microsoft.Security.Application.Encoder.HtmlEncode(SwapLineBreaks(html));
                return SwapLineBreaksBack(html);
            }

            return Microsoft.Security.Application.Encoder.HtmlEncode(html);
        }


        private const string _nlReplace = "--newline--";
        private static string SwapLineBreaks(string lines)
        {
            return lines.Replace(Environment.NewLine, _nlReplace);
        }
        private static string SwapLineBreaksBack(string lines)
        {
            return lines.Replace(_nlReplace, Environment.NewLine);
        }

        /// <summary>
        /// Url Encodes a string using the XSS library
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UrlEncode(string input)
        {
            return Microsoft.Security.Application.Encoder.UrlEncode(input);
        }

        /// <summary>
        /// Uses regex to strip HTML from a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripHtmlFromString(string input)
        {
            input = Regex.Replace(input, @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", string.Empty, RegexOptions.Singleline);
            input = Regex.Replace(input, @"\[[^]]+\]", "");
            return input;
        }

        /// <summary>
        /// Returns safe plain text using XSS library
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SafePlainText(string input)
        {
            if(!string.IsNullOrEmpty(input))
            {
                input = StripHtmlFromString(input);
                input = GetSafeHtml(input);
            }
            return input;
        }

        /// <summary>
        /// Returns a HTML link
        /// </summary>
        /// <param name="href"></param>
        /// <param name="anchortext"></param>
        /// <param name="openinnewwindow"></param>
        /// <returns></returns>
        public static string ReturnHtmlLink(string href, string anchortext, bool openinnewwindow = false)
        {
            return string.Format(openinnewwindow ? "<a rel='nofollow' target='_blank' href=\"{0}\">{1}</a>" : "<a rel='nofollow' href=\"{0}\">{1}</a>", href, anchortext);
        }

        /// <summary>
        /// Returns a HTML image tag
        /// </summary>
        /// <param name="url"></param>
        /// <param name="alt"></param>
        /// <returns></returns>
        public static string ReturnImageHtml(string url, string alt)
        {
            return string.Format("<img src=\"{0}\" alt=\"{1}\" />", url, alt);
        }

        /// <summary>
        /// Strips all non alpha/numeric charators from a string
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        public static string StripNonAlphaNumeric(string strInput, string replaceWith)
        {
            strInput = Regex.Replace(strInput, "[^\\w]", replaceWith);
            strInput = strInput.Replace("--", "-");
            if(strInput.EndsWith("-"))
            {
                strInput = strInput.Substring(0, (strInput.Length - 1));
            }
            return strInput;
        }

        /// <summary>
        /// Creates a URL freindly string, good for SEO
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        public static string CreateUrl(string strInput, string replaceWith)
        {
            return StripNonAlphaNumeric(strInput, replaceWith).ToLower();
        }

        /// <summary>
        /// Returns UK formatted amount from int
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static string FormatCurrency(int? amount)
        {
            return amount != null ? string.Format("{0:C}", amount) : "n/a";
        }

        /// <summary>
        /// A method to convert basic BBCode to HTML
        /// </summary>
        /// <param name="str">A string formatted in BBCode</param>
        /// <returns>The HTML representation of the BBCode string</returns>
        public static string ConvertBbCodeToHtml(string str)
        {
            // As this is a BBEditor we need to replace line breaks
            // or they won't show in the front end
            str = ReplaceLineBreaks(str, "<br>");

            // format the bold tags: [b][/b]
            // becomes: <strong></strong>
            var exp = new Regex(@"\[b\](.+?)\[/b\]");
            str = exp.Replace(str, "<strong>$1</strong>");

            // format the italic tags: [i][/i]
            // becomes: <em></em>
            exp = new Regex(@"\[i\](.+?)\[/i\]");
            str = exp.Replace(str, "<em>$1</em>");

            // format the underline tags: [u][/u]
            // becomes: <u></u>
            exp = new Regex(@"\[u\](.+?)\[/u\]");
            str = exp.Replace(str, "<u>$1</u>");

            // format the underline tags: [ul][/ul]
            // becomes: <ul></ul>
            exp = new Regex(@"\[ul\](.+?)\[/ul\]");
            str = exp.Replace(str, "<ul>$1</ul>");

            // format the underline tags: [ol][/ol]
            // becomes: <ol></ol>
            exp = new Regex(@"\[ol\](.+?)\[/ol\]");
            str = exp.Replace(str, "<ol>$1</ol>");

            // format the underline tags: [li][/li]
            // becomes: <li></li>
            exp = new Regex(@"\[li\](.+?)\[/li\]");
            str = exp.Replace(str, "<li>$1</li>");

            // format the code tags: [code][/code]
            // becomes: <pre></pre>
            exp = new Regex(@"\[code\](.+?)\[/code\]");
            str = exp.Replace(str, "<pre>$1</pre>");

            // format the code tags: [quote][/quote]
            // becomes: <blockquote></blockquote>
            exp = new Regex(@"\[quote\](.+?)\[/quote\]");
            str = exp.Replace(str, "<blockquote>$1</blockquote>");

            // format the strike tags: [s][/s]
            // becomes: <strike></strike>
            exp = new Regex(@"\[s\](.+?)\[/s\]");
            str = exp.Replace(str, "<strike>$1</strike>");

            //### Before this replace links without http ###
            str.Replace("[url=www.", "[url=http://www.");
            // format the url tags: [url=www.website.com]my site[/url]
            // becomes: <a href="www.website.com">my site</a>
            exp = new Regex(@"\[url\=([^\]]+)\]([^\]]+)\[/url\]");
            str = exp.Replace(str, "<a rel=\"nofollow\" href=\"$1\">$2</a>");

            // format the img tags: [img]www.website.com/img/image.jpeg[/img]
            // becomes: <img src="www.website.com/img/image.jpeg" />
            exp = new Regex(@"\[img\]([^\]]+)\[/img\]");
            str = exp.Replace(str, "<img src=\"$1\" />");

            // format img tags with alt: [img=www.website.com/img/image.jpeg]this is the alt text[/img]
            // becomes: <img src="www.website.com/img/image.jpeg" alt="this is the alt text" />
            exp = new Regex(@"\[img\=([^\]]+)\]([^\]]+)\[/img\]");
            str = exp.Replace(str, "<img src=\"$1\" alt=\"$2\" />");

            // format the size tags: [size=1.2][/size]
            // becomes: <span style="font-size:1.2em;"></span>
            exp = new Regex(@"\[size\=([^\]]+)\]([^\]]+)\[/size\]");
            str = exp.Replace(str, "<span style=\"font-size:$1em;\">$2</span>");

            // YouTube Insert Video, just add the video ID and it inserts video into post
            exp = new Regex(@"\[youtube\]([^\]]+)\[/youtube\]");
            str = exp.Replace(str, "<iframe title=\"YouTube video player\" width=\"500\" height=\"281\" src=\"http://www.youtube.com/embed/$1\" frameborder=\"0\" allowfullscreen></iframe>");

            // YouTube Insert Video, just add the video ID and it inserts video into post
            exp = new Regex(@"\[vimeo\]([^\]]+)\[/vimeo\]");
            str = exp.Replace(str, "<iframe src=\"http://player.vimeo.com/video/$1?portrait=0\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe>");

            return str;
        }

        public static string CheckLinkHasHttp(string url)
        {
            return !url.Contains("http://") ? string.Concat("http://", url) : url;
        }

        private static readonly Random _rng = new Random();
        private const string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string RandomString(int size)
        {
            var buffer = new char[size];
            for (var i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }
            return new string(buffer);
        }
    }
}
