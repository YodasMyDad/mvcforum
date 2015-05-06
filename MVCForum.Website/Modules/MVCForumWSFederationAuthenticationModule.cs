using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Web;

namespace MVCForum.Website.Modules
{
    public class MVCForumWSFederationAuthenticationModule : WSFederationAuthenticationModule
    {
        /// <summary>
        /// Builds the SignInRequestMessage used to redirect to the WSFederation Issuer site.
        /// [Dev Note] This method was added in order to return the SignInRequestMessage instead of actually doing the redirect like RedirectToIdentityProvider does.  The
        /// reason it needs to be returned is for the MVC Controller context.  To keep the controller clean I didn't want WIF doing the redirect itself while in the controller
        /// pipeline.  This allows us to use "return Redirect(signInRequestMessage.RequestUrl);" to do the redirect ourselves.
        /// </summary>
        /// <param name="uniqueId">The WSFAM saves this value in the wctx parameter in the WS-Federation sign in request; however, the module does not use it when processing sign-in requests or sign-in responses. You can set it to any value. It does not have to be unique. For more information, see the CreateSignInRequest method.</param>
        /// <param name="returnUrl">The URL to which the module should return upon authentication.</param>
        /// <param name="persist">The WSFAM saves this value in the wctx parameter in the WS-Federation sign in request; however, the module does not use it when processing sign-in requests or sign-in responses. You can set it either true or</param>
        /// <param name="cancelled">An output parameter that indicates whether the SignIn Event's triggered a Cancellation of the sign in process.  If it comes out true it means code somewhere in the application subscribed to the event and wants authentication to be cancelled, handle appropriately.</param>
        /// <exception cref="https://msdn.microsoft.com/en-us/library/system.invalidoperationexception(v=vs.110).aspx">Issuer is null or an empty string.  Or Realm is null or an empty string. Or HttpContext.Current is null. Or HttpContext.Current.Response is null.</exception>
        /// <returns></returns>
        public SignInRequestMessage GetIdentityProviderRedirectUrl(string uniqueId, string returnUrl, bool persist, out bool cancelled)
        {
            this.VerifyProperties();
            HttpContext current = HttpContext.Current;
            if ((current == null) || (current.Response == null))
            {
                throw new InvalidOperationException("No HttpContext...");
            }
            RedirectingToIdentityProviderEventArgs e = new RedirectingToIdentityProviderEventArgs(this.CreateSignInRequest(uniqueId, returnUrl, persist));
            this.OnRedirectingToIdentityProvider(e);
            cancelled = e.Cancel;
            return e.SignInRequestMessage;
        }
    }
}