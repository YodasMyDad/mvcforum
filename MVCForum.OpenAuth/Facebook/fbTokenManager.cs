using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OAuth2;

namespace MVCForum.OpenAuth
{
    public class fbTokenManager : IClientAuthorizationTracker
    {
        public IAuthorizationState GetAuthorizationState(
          Uri callbackUrl, string clientState)
        {
            return new AuthorizationState
            {
                Callback = callbackUrl,
            };
        }
    }
}
