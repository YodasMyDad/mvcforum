#region Using

using System;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Web.Caching;
using System.Collections.Specialized;
using System.Collections.Generic;

namespace MVCForum.Website.Application
{

    #endregion

    /// <summary>
    /// Summary description for OpenID
    /// </summary>
    public static class OpenID
    {

        /// <summary>
        /// Perform redirection to the OpenID provider based on the specified identity.
        /// </summary>
        /// <param name="identity">The identity or OpenID URL.</param>
        /// <param name="requiredParameters">The required parameters. Can be null or string.empty.</param>
        /// <param name="optionalParameters">The optional parameters. Can be null or string.empty.</param>
        public static bool Login(string identity, string requiredParameters, string optionalParameters)
        {
            try
            {
                var dic = GetIdentityServer(identity);
                var server = dic["openid.server"];
                var delgate = dic["openid.delegate"] ?? identity;

                if (!string.IsNullOrEmpty(server))
                {
                    string redirectUrl = CreateRedirectUrl(requiredParameters, optionalParameters, delgate, identity);

                    // Add the provided data to session so it can be tracked after authentication
                    var data = new OpenIdData(identity);
                    HttpContext.Current.Session["openid"] = data;

                    HttpContext.Current.Response.Redirect(server + redirectUrl, true);
                }
            }
            catch (Exception)
            { }

            return false;
        }

        /// <summary>
        /// Authenticates the request from the OpenID provider.
        /// </summary>
        public static OpenIdData Authenticate()
        {
            var data = (OpenIdData)HttpContext.Current.Session["openid"];

            // Make sure the client has been through the Login method
            if (data == null)
                return new OpenIdData(string.Empty);

            var query = HttpContext.Current.Request.QueryString;

            // Make sure the incoming request's identity matches the one stored in session
            if (query["openid.claimed_id"] != data.Identity)
                return data;

            data.IsSuccess = query["openid.mode"] == "id_res";

            foreach (string name in query.Keys)
            {
                if (name.StartsWith("openid.sreg."))
                    data.Parameters.Add(name.Replace("openid.sreg.", string.Empty), query[name]);
            }

            HttpContext.Current.Session.Remove("openid");
            return data;
        }

        /// <summary>
        /// Gets a value indicating whether the request comes from an OpenID provider.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this is an OpenID request; otherwise, <c>false</c>.
        /// </value>
        public static bool IsOpenIdRequest
        {
            get
            {
                // All OpenID request must use the GET method
                if (!HttpContext.Current.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    return false;

                return HttpContext.Current.Request.QueryString["openid.mode"] != null;
            }
        }

        #region Private methods

        private static readonly Regex REGEX_LINK = new Regex(@"<link[^>]*/?>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex REGEX_HREF = new Regex("href\\s*=\\s*(?:\"(?<1>[^\"]*)\"|(?<1>\\S+))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Crawls the identity URL to find the auto-discovery link headers.
        /// </summary>
        public static StringDictionary GetIdentityServer(string identity)
        {
            using (var client = new WebClient())
            {
                var html = client.DownloadString(identity);
                var col = new StringDictionary();

                foreach (Match match in REGEX_LINK.Matches(html))
                {
                    AssignValue(match, col, "openid.server");
                    AssignValue(match, col, "openid.delegate");
                }

                return col;
            }
        }

        private static void AssignValue(Match linkMatch, StringDictionary col, string name)
        {
            if (linkMatch.Value.IndexOf(name, StringComparison.Ordinal) > 0)
            {
                var hrefMatch = REGEX_HREF.Match(linkMatch.Value);
                if (hrefMatch.Success)
                {
                    if (!col.ContainsKey(name))
                        col.Add(name, hrefMatch.Groups[1].Value);
                }
            }
        }

        /// <summary>
        /// Creates the URL to the OpenID provider with all parameters.
        /// </summary>
        private static string CreateRedirectUrl(string requiredParameters, string optionalParameters, string delgate, string identity)
        {
            var sb = new StringBuilder();
            sb.Append("?openid.ns=" + HttpUtility.UrlEncode("http://specs.openid.net/auth/2.0"));
            sb.Append("&openid.mode=checkid_setup");
            sb.Append("&openid.identity=" + HttpUtility.UrlEncode(delgate));
            sb.Append("&openid.claimed_id=" + HttpUtility.UrlEncode(identity));
            sb.Append("&openid.return_to=" + HttpUtility.UrlEncode(HttpContext.Current.Request.Url.ToString()));

            if (!string.IsNullOrEmpty(requiredParameters) || !string.IsNullOrEmpty(optionalParameters))
            {
                sb.Append("&openid.ns.sreg=" + HttpUtility.UrlEncode("http://openid.net/extensions/sreg/1.1"));

                if (!string.IsNullOrEmpty(requiredParameters))
                    sb.Append("&openid.sreg.required=" + HttpUtility.UrlEncode(requiredParameters));

                if (!string.IsNullOrEmpty(optionalParameters))
                    sb.Append("&openid.sreg.optional=" + HttpUtility.UrlEncode(optionalParameters));
            }

            return sb.ToString();
        }

        #endregion

    }

    /// <summary>
    /// The data store used for keeping state between OpenID requests.
    /// </summary>
    public class OpenIdData
    {

        public OpenIdData(string identity)
        {
            Identity = identity;
        }

        public bool IsSuccess;
        public string Identity;
        public NameValueCollection Parameters = new NameValueCollection();
    }
}



  //protected void Page_Load(object sender, EventArgs e)
  //{
  //  if (OpenID.IsOpenIdRequest)
  //  {
  //    OpenIdData data = OpenID.Authenticate();
  //    if (data.IsSuccess)
  //    {
  //      StringBuilder sb = new StringBuilder();
  //      sb.AppendFormat("<b>OpenID: {0}</b><br />", data.Identity);
  //      sb.AppendFormat("email: {0}<br />", data.Parameters["email"]);
  //      sb.AppendFormat("fullname: {0}<br />", data.Parameters["fullname"]);
  //      sb.AppendFormat("country: {0}<br />", data.Parameters["country"]);
  //      sb.AppendFormat("language: {0}<br />", data.Parameters["language"]);

  //      Response.Write(sb.ToString());
  //    }
  //  }
  //}

  //void btnLogon_Click(object sender, EventArgs e)
  //{
  //  bool success = OpenID.Login(txtOpenId.Text, "email,fullname", "country,language");

  //  if (!success)
  //  {
  //    Response.Write("The OpenID is not valid");
  //  }
  //}