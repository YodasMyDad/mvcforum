using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth2;

namespace MVCForum.OpenAuth.GooglePlus
{
    public class GooglePlusClient : WebServerClient
    {
        // Set the end points
        private static readonly AuthorizationServerDescription GooglePlusDescription
          = new AuthorizationServerDescription
          {
              TokenEndpoint = new Uri("https://www.googleapis.com/oauth2/v3/token"), 
              AuthorizationEndpoint = new Uri("https://accounts.google.com/o/oauth2/auth") 
          };

        // Add any extra scopes you need i.e. Email authorisation = email, publish to users wall = publish_stream
        public IEnumerable<string> ScopeParameters = new List<string>
            {
                "email"
            }; 

        /// <summary>
        /// Initializes a new instance of 
        /// the <see cref="GooglePlusClient"/> class.
        /// </summary>
        public GooglePlusClient()
            : base(GooglePlusDescription)
        {
            this.AuthorizationTracker = new gpTokenManager();
        }
    }
}
