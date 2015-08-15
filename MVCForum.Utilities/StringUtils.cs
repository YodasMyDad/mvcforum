using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
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

        // Gets the full url including 
        public static string ReturnCurrentDomain()
        {
            var r = HttpContext.Current.Request;
            var builder = new UriBuilder(r.Url.Scheme, r.Url.Host, r.Url.Port);
            return builder.Uri.ToString().TrimEnd('/');
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
            return IsValidEmail(email) ? string.Format("{0}://www.gravatar.com/avatar/{1}?s={2}", HttpContext.Current.Request.Url.Scheme, md5HashString(email), size) : "";
        }
        #endregion

        #region Validation

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

        #endregion

        #region Misc

        /// <summary>
        /// Create a salt for the password hash (just makes it a bit more complex)
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string CreateSalt(int size)
        {
            // Generate a cryptographic random number.
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[size];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        /// <summary>
        /// Generate a hash for a password, adding a salt value
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string GenerateSaltedHash(string plainText, string salt)
        {
            // http://stackoverflow.com/questions/2138429/hash-and-salt-passwords-in-c-sharp

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            // Combine the two lists
            var plainTextWithSaltBytes = new List<byte>(plainTextBytes.Length + saltBytes.Length);
            plainTextWithSaltBytes.AddRange(plainTextBytes);
            plainTextWithSaltBytes.AddRange(saltBytes);

            // Produce 256-bit hashed value i.e. 32 bytes
            HashAlgorithm algorithm = new SHA256Managed();
            var byteHash = algorithm.ComputeHash(plainTextWithSaltBytes.ToArray());
            return Convert.ToBase64String(byteHash);
        }

        public static string PostForm(string url, string poststring)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);

            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/x-www-form-urlencoded";

            var bytedata = Encoding.UTF8.GetBytes(poststring);
            httpRequest.ContentLength = bytedata.Length;

            var requestStream = httpRequest.GetRequestStream();
            requestStream.Write(bytedata, 0, bytedata.Length);
            requestStream.Close();

            var httpWebResponse = (HttpWebResponse)httpRequest.GetResponse();
            var responseStream = httpWebResponse.GetResponseStream();

            var sb = new StringBuilder();

            if (responseStream != null)
            {
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        sb.Append(line);
                    }
                }
            }

            return sb.ToString();

        }


        #endregion

        #region Conversion

        /// <summary>
        /// Converts a csv list of string guids into a real list of guids
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static List<Guid> CsvIdConverter(string csv)
        {
            return csv.TrimStart(',').TrimEnd(',').Split(',').Select(Guid.Parse).ToList();
        }


        #endregion

        #region Numeric Helpers
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
        #endregion

        #region String content helpers

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
            {
                return 0;
            }
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
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

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
        /// Returns a string to do a related question/search lookup
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        public static string ReturnSearchString(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return searchTerm;
            }

            // Lower case
            searchTerm = searchTerm.ToLower();

            // Firstly strip non alpha numeric charactors out
            searchTerm = Regex.Replace(searchTerm, @"[^\w\.@\- ]", "");

            // Now strip common words out and retun the final result
            return string.Join(" ", searchTerm.Split().Where(w => !CommonWords().Contains(w)).ToArray());
        }

        /// <summary>
        /// Returns a list of the most common english words
        /// TODO: Need to put this in something so people can add other language lists of common words
        /// </summary>
        /// <returns></returns>
        public static IList<string> CommonWords()
        {
            return new List<string>
                {
                    "the", "be",  "to",  
                    "of",  
                    "and",
                    "a",
                    "in",   
                    "that",  
                    "have",
                    "i",
                    "it",   
                    "for",
                    "not",
                    "on",
                    "with",
                    "he",
                    "as",
                    "you",
                    "do",
                    "at",
                    "this",
                    "but",
                    "his",
                    "by",
                    "from",
                    "they",
                    "we",
                    "say",
                    "her",
                    "she",
                    "or",
                    "an",
                    "will",
                    "my",
                    "one",
                    "all",
                    "would",
                    "there",
                    "their",
                    "what",
                    "so",
                    "up",
                    "out",
                    "if",
                    "about",
                    "who",
                    "get",
                    "which",
                    "go",
                    "me",
                    "when",
                    "make",
                    "can",
                    "like",
                    "time",
                    "no",
                    "just",
                    "him",
                    "know",
                    "take",
                    "people",
                    "into",
                    "year",
                    "your",
                    "good",
                    "some",
                    "could",
                    "them",
                    "see",
                    "other",
                    "than",
                    "then",
                    "now",
                    "look",
                    "only",
                    "come",
                    "its",
                    "over",
                    "think",
                    "also",
                    "back",
                    "after",
                    "use",
                    "two",
                    "how",
                    "our",
                    "work",
                    "first",
                    "well",
                    "way",
                    "even",
                    "new",
                    "want",
                    "because",
                    "any",
                    "these",
                    "give",
                    "day",
                    "most",
                    "cant",
                    "us"
                };
        }

        #endregion

        #region Sanitising

        /// <summary>
        /// Strips all non alpha/numeric charators from a string
        /// </summary>
        /// <param name="strInput"></param>
        /// <param name="replaceWith"></param>
        /// <returns></returns>
        public static string StripNonAlphaNumeric(string strInput, string replaceWith)
        {
            strInput = Regex.Replace(strInput, "[^\\w]", replaceWith);
            strInput = strInput.Replace(string.Concat(replaceWith, replaceWith, replaceWith), replaceWith)
                                .Replace(string.Concat(replaceWith, replaceWith), replaceWith)
                                .TrimStart(Convert.ToChar(replaceWith))
                                .TrimEnd(Convert.ToChar(replaceWith));
            return strInput;
        }

        /// <summary>
        /// Get the current users IP address
        /// </summary>
        /// <returns></returns>
        public static string GetUsersIpAddress()
        {
            var context = HttpContext.Current;
            var serverName = context.Request.ServerVariables["SERVER_NAME"];
            if (serverName.ToLower().Contains("localhost"))
            {
                return serverName;
            }
            var ipList = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            return !string.IsNullOrEmpty(ipList) ? ipList.Split(',')[0] : context.Request.ServerVariables["REMOTE_ADDR"];
        }

        /// <summary>
        /// Used to pass all string input in the system  - Strips all nasties from a string/html
        /// </summary>
        /// <param name="html"></param>
        /// <param name="useXssSantiser"></param>
        /// <returns></returns>
        public static string GetSafeHtml(string html, bool useXssSantiser = false)
        {
            // Scrub html
            html = ScrubHtml(html, useXssSantiser);

            // remove unwanted html
            html = RemoveUnwantedTags(html);

            return html;
        }


        /// <summary>
        /// Takes in HTML and returns santized Html/string
        /// </summary>
        /// <param name="html"></param>
        /// <param name="useXssSantiser"></param>
        /// <returns></returns>
        public static string ScrubHtml(string html, bool useXssSantiser = false)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            // The reason we have this option, is using the santiser with the MarkDown editor 
            // causes problems with line breaks.
            if (useXssSantiser)
            {
                return Sanitizer.GetSafeHtmlFragment(html);
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            //Remove potentially harmful elements
            var nc = doc.DocumentNode.SelectNodes("//script|//link|//iframe|//frameset|//frame|//applet|//object|//embed");
            if (nc != null)
            {
                foreach (var node in nc)
                {
                    node.ParentNode.RemoveChild(node, false);

                }
            }

            //remove hrefs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
            if (nc != null)
            {

                foreach (var node in nc)
                {
                    node.SetAttributeValue("href", "#");
                }
            }

            //remove img with refs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
            if (nc != null)
            {
                foreach (var node in nc)
                {
                    node.SetAttributeValue("src", "#");
                }
            }

            //remove on<Event> handlers from all tags
            nc = doc.DocumentNode.SelectNodes("//*[@onclick or @onmouseover or @onfocus or @onblur or @onmouseout or @ondoubleclick or @onload or @onunload or @onerror]");
            if (nc != null)
            {
                foreach (var node in nc)
                {
                    node.Attributes.Remove("onFocus");
                    node.Attributes.Remove("onBlur");
                    node.Attributes.Remove("onClick");
                    node.Attributes.Remove("onMouseOver");
                    node.Attributes.Remove("onMouseOut");
                    node.Attributes.Remove("onDoubleClick");
                    node.Attributes.Remove("onLoad");
                    node.Attributes.Remove("onUnload");
                    node.Attributes.Remove("onError");
                }
            }

            // remove any style attributes that contain the word expression (IE evaluates this as script)
            nc = doc.DocumentNode.SelectNodes("//*[contains(translate(@style, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'expression')]");
            if (nc != null)
            {
                foreach (var node in nc)
                {
                    node.Attributes.Remove("stYle");
                }
            }

            return doc.DocumentNode.WriteTo();
        }

        public static string RemoveUnwantedTags(string html)
        {

            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var unwantedTagNames = new List<string>
            {
                "div",
                "font"
            };

            var htmlDoc = new HtmlDocument();

            // load html
            htmlDoc.LoadHtml(html);

            var tags = (from tag in htmlDoc.DocumentNode.Descendants()
                        where unwantedTagNames.Contains(tag.Name)
                        select tag).Reverse();


            // find formatting tags
            foreach (var item in tags)
            {
                if (item.PreviousSibling == null)
                {
                    // Prepend children to parent node in reverse order
                    foreach (var node in item.ChildNodes.Reverse())
                    {
                        item.ParentNode.PrependChild(node);
                    }
                }
                else
                {
                    // Insert children after previous sibling
                    foreach (var node in item.ChildNodes)
                    {
                        item.ParentNode.InsertAfter(node, item.PreviousSibling);
                    }
                }

                // remove from tree
                item.Remove();
            }

            // return transformed doc
            return htmlDoc.DocumentNode.WriteContentTo().Trim();
        }

        /// <summary>
        /// Url Encodes a string using the XSS library
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UrlEncode(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return Microsoft.Security.Application.Encoder.UrlEncode(input);
            }
            return input;
        }

        /// <summary>
        /// Decode a url
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UrlDecode(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return HttpUtility.UrlDecode(input);
            }
            return input;
        }

        /// <summary>
        /// decode a chunk of html or url
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string HtmlDecode(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                return HttpUtility.HtmlDecode(input);
            }
            return input;
        }

        /// <summary>
        /// Uses regex to strip HTML from a string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StripHtmlFromString(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = Regex.Replace(input, @"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", string.Empty, RegexOptions.Singleline);
                input = Regex.Replace(input, @"\[[^]]+\]", "");
            }
            return input;
        }

        /// <summary>
        /// Returns safe plain text using XSS library
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SafePlainText(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                input = StripHtmlFromString(input);
                input = GetSafeHtml(input, true);
            }
            return input;
        }
        #endregion

        #region Html Element Helpers
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

        public static string CheckLinkHasHttp(string url)
        {
            return !url.Contains("http://") ? string.Concat("http://", url) : url;
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
        #endregion

        #region Urls / Webpages
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

        public static string CreateUrl(string strInput, string replaceWith)
        {
            var str = RemoveAccents(strInput).ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= 50 ? str.Length : 50).Trim();
            str = Regex.Replace(str, @"\s", replaceWith); // hyphens or whatever
            return str;
        }

        public static string RemoveAccents(string text)
        {
            var bytes = Encoding.GetEncoding("Cyrillic").GetBytes(text);
            return Encoding.ASCII.GetString(bytes);
        }

        ///// <summary>
        ///// Creates a URL freindly string, good for SEO
        ///// </summary>
        ///// <param name="strInput"></param>
        ///// <param name="replaceWith"></param>
        ///// <returns></returns>
        //public static string CreateUrl(string strInput, string replaceWith)
        //{
        //    // Doing this to stop the urls getting encoded
        //    var url = RemoveAccents(strInput);
        //    return StripNonAlphaNumeric(url, replaceWith).ToLower();
        //}

        //public static string RemoveAccents(string input)
        //{
        //    // Replace accented characters for the closest ones:
        //    //var from = "ÂÃÄÀÁÅÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝàáâãäåçèéêëìíîïðñòóôõöøùúûüýÿ".ToCharArray();
        //    //var to = "AAAAAACEEEEIIIIDNOOOOOOUUUUYaaaaaaceeeeiiiidnoooooouuuuyy".ToCharArray();
        //    //for (var i = 0; i < from.Length; i++)
        //    //{
        //    //    input = input.Replace(from[i], to[i]);
        //    //}

        //    //// Thorn http://en.wikipedia.org/wiki/%C3%9E
        //    //input = input.Replace("Þ", "TH");
        //    //input = input.Replace("þ", "th");

        //    //// Eszett http://en.wikipedia.org/wiki/%C3%9F
        //    //input = input.Replace("ß", "ss");

        //    //// AE http://en.wikipedia.org/wiki/%C3%86
        //    //input = input.Replace("Æ", "AE");
        //    //input = input.Replace("æ", "ae");

        //    //return input;


        //    var stFormD = input.Normalize(NormalizationForm.FormD);
        //    var sb = new StringBuilder();

        //    foreach (var t in stFormD)
        //    {
        //        var uc = CharUnicodeInfo.GetUnicodeCategory(t);
        //        if (uc != UnicodeCategory.NonSpacingMark)
        //        {
        //            sb.Append(t);
        //        }
        //    }

        //    return (sb.ToString().Normalize(NormalizationForm.FormC));

        //}

        #endregion

        #region Rich Text Formatting

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
        /// Converts markdown into HTML
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ConvertMarkDown(string str)
        {
            var md = new MarkdownSharp.Markdown { AutoHyperlink = true, LinkEmails = false };
            return md.Transform(str);
        }

        public static string EmbedVideosInPosts(string str)
        {
            #region OLD PARSING BB TAGS
            //// YouTube Insert Video, just add the video ID and it inserts video into post
            //var exp = new Regex(@"\[youtube\]([^\]]+)\[/youtube\]");
            //str = exp.Replace(str, "<div class=\"video-container\"><iframe title=\"YouTube video player\" width=\"500\" height=\"281\" src=\"http://www.youtube.com/embed/$1\" frameborder=\"0\" allowfullscreen></iframe></div>");

            //// YouTube Insert Video, just add the video ID and it inserts video into post
            //exp = new Regex(@"\[vimeo\]([^\]]+)\[/vimeo\]");
            //str = exp.Replace(str, "<div class=\"video-container\"><iframe src=\"http://player.vimeo.com/video/$1?portrait=0\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe></div>");

            //// YouTube Screenr Video, just add the video ID and it inserts video into post
            //exp = new Regex(@"\[screenr\]([^\]]+)\[/screenr\]");
            //str = exp.Replace(str, "<div class=\"video-container\"><iframe src=\"http://www.screenr.com/embed/$1\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe></div>");

            //// Add tweets

            //return str; 
            #endregion


            if (str.IndexOf("youtube.com", StringComparison.CurrentCultureIgnoreCase) >= 0 || str.IndexOf("youtu.be", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                const string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:(?:(?:youtube.com\/watch\?[^?]*v=|youtu.be\/)([\w\-]+))(?:[^\s?]+)?)";
                const string replacement = "<div class=\"video-container\" itemscope itemtype=\"http://schema.org/VideoObject\"><meta itemprop=\"embedURL\" content=\"http://www.youtube.com/embed/$1\"><iframe title='YouTube video player' width='500' height='281' src='http://www.youtube.com/embed/$1' frameborder='0' allowfullscreen='1'></iframe></div>";

                var rgx = new Regex(pattern);
                str = rgx.Replace(str, replacement);
            }

            if (str.IndexOf("vimeo.com", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                const string pattern = @"(?:https?:\/\/)?vimeo\.com/(?:.*#|.*/videos/)?([0-9]+)";
                const string replacement = "<div class=\"video-container\" itemscope itemtype=\"http://schema.org/VideoObject\"><meta itemprop=\"embedURL\" content=\"http://player.vimeo.com/video/$1?portrait=0\"><iframe src=\"http://player.vimeo.com/video/$1?portrait=0\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe></div>";

                var rgx = new Regex(pattern);
                str = rgx.Replace(str, replacement);
            }

            if (str.IndexOf("screenr", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                const string pattern = @"(?:https?:\/\/)?(?:www\.)screenr\.com/([a-zA-Z0-9]+)";
                const string replacement = "<div class=\"video-container\" itemscope itemtype=\"http://schema.org/VideoObject\"><meta itemprop=\"embedURL\" content=\"http://www.screenr.com/embed/$1\"><iframe src=\"http://www.screenr.com/embed/$1\" width=\"500\" height=\"281\" frameborder=\"0\"></iframe></div>";

                var rgx = new Regex(pattern);
                str = rgx.Replace(str, replacement);
            }

            if (str.IndexOf("instagr", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                var reg = new Regex(@"http://instagr\.?am(?:\.com)?/\S*", RegexOptions.Compiled);
                var idRegex = new Regex(@"(?<=p/).*?(?=/)", RegexOptions.Compiled);
                var result = new StringBuilder();
                using (var reader = new StringReader(str))
                {
                    while (reader.Peek() > 0)
                    {
                        var line = reader.ReadLine();
                        if (line != null && reg.IsMatch(line))
                        {
                            var url = reg.Match(line).ToString();

                            // Find links 

                            result.AppendLine(reg.Replace(line, string.Format("<p><img src=\"http://instagram.com/p/{0}/media/?size=l\" class=\"img-responsive\" /></p>", idRegex.Match(url))));
                        }
                        else
                        {
                            result.AppendLine(line);
                        }

                    }
                }

                str = result.ToString();
            }

            return str;
        }

        /// <summary>
        /// A method to convert basic BBCode to HTML
        /// </summary>
        /// <param name="str">A string formatted in BBCode</param>
        /// <param name="replaceLineBreaks">Whether or not to replace line breaks with br's</param>
        /// <returns>The HTML representation of the BBCode string</returns>
        public static string ConvertBbCodeToHtml(string str, bool replaceLineBreaks = true)
        {
            if (replaceLineBreaks)
            {
                // As this is a BBEditor we need to replace line breaks
                // or they won't show in the front end
                str = ReplaceLineBreaks(str, "<br>");
            }

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

            return str;
        }
        #endregion

    }
}
