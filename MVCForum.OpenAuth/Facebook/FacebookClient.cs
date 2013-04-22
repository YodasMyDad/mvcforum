using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth2;

namespace MVCForum.OpenAuth.Facebook
{
    public class FacebookClient : WebServerClient
    {
        // Set the end points
        private static readonly AuthorizationServerDescription FacebookDescription
          = new AuthorizationServerDescription
          {
              TokenEndpoint = new Uri("https://graph.facebook.com/oauth/access_token"),
              AuthorizationEndpoint = new Uri("https://graph.facebook.com/oauth/authorize")
          };

        // Add any extra scopes you need i.e. Email authorisation = email, publish to users wall = publish_stream
        public IEnumerable<string> ScopeParameters = new List<string>
            {
                "email"
            }; 

        /// <summary>
        /// Initializes a new instance of 
        /// the <see cref="FacebookClient"/> class.
        /// </summary>
        public FacebookClient()
            : base(FacebookDescription)
        {
            this.AuthorizationTracker = new fbTokenManager();
        }
    }
}
