using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetOpenAuth.OpenId;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using DotNetOpenAuth.OpenId.Extensions.SimpleRegistration;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace MVCForum.OpenAuth
{
    public class OpenAuthHelpers
    {
        #region Open ID Helpers

        public static IAuthenticationResponse CheckOpenIdResponse()
        {
            var openId = new OpenIdRelyingParty();
            return openId.GetResponse();
        }

        public static IAuthenticationRequest GetRedirectActionRequest(Identifier provider)
        {
            var openid = new OpenIdRelyingParty();
            var request = openid.CreateRequest(Identifier.Parse(WellKnownProviders.Google));

            var fr = new FetchRequest();
            fr.Attributes.AddRequired(WellKnownAttributes.Contact.Email);
            fr.Attributes.AddRequired(WellKnownAttributes.Name.First);
            fr.Attributes.AddRequired(WellKnownAttributes.Name.Last);
            request.AddExtension(fr);

            // Require some additional data
            request.AddExtension(new ClaimsRequest
            {
                Email = DemandLevel.Require,
                FullName = DemandLevel.Require,
                Nickname = DemandLevel.Require
            });

            return request;
        } 

        #endregion
    }
}
