using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Web;

namespace MVCForum.SSOLogin.WIF
{
    public class ExampleSecurityTokenService : SecurityTokenService
    {
        private const string SIGNING_CERTIFICATE_NAME = "CN=TokenSigningCert";
        private const string ENCRYPTING_CERTIFICATE_NAME = "CN=TokenSigningCert";
        private SigningCredentials _signingCreds;
        private EncryptingCredentials _encryptingCreds;

        public ExampleSecurityTokenService(SecurityTokenServiceConfiguration configuration)
            : base(configuration)
        {
            // Setup the certificate our STS is going to use to sign the issued tokens
            _signingCreds = new X509SigningCredentials(CertificateUtil.GetCertificate(StoreName.TrustedPeople, StoreLocation.LocalMachine, SIGNING_CERTIFICATE_NAME));

            // Note: In this sample app only a si   ngle RP identity is shown, which is localhost, and the certificate of that RP is 
            // populated as _encryptingCreds
            // If you have multiple RPs for the STS you would select the certificate that is specific to 
            // the RP that requests the token and then use that for _encryptingCreds
            _encryptingCreds = new X509EncryptingCredentials(CertificateUtil.GetCertificate(StoreName.TrustedPeople, StoreLocation.LocalMachine, ENCRYPTING_CERTIFICATE_NAME));
        }

        protected override Scope GetScope(System.Security.Claims.ClaimsPrincipal principal, System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request)
        {
            // Validate the AppliesTo address

            // Create the scope using the request AppliesTo address and the RP identity
            Scope scope = new Scope(request.AppliesTo.Uri.AbsoluteUri, _signingCreds);

            if (Uri.IsWellFormedUriString(request.ReplyTo, UriKind.Absolute))
            {
                if (request.AppliesTo.Uri.Host != new Uri(request.ReplyTo).Host)
                    scope.ReplyToAddress = request.AppliesTo.Uri.AbsoluteUri;
                else
                    scope.ReplyToAddress = request.ReplyTo;
            }
            else
            {
                Uri resultUri = null;
                if (Uri.TryCreate(request.AppliesTo.Uri, request.ReplyTo, out resultUri))
                    scope.ReplyToAddress = resultUri.AbsoluteUri;
                else
                    scope.ReplyToAddress = request.AppliesTo.Uri.ToString();
            }

            // Note: In this sample app only a single RP identity is shown, which is localhost, and the certificate of that RP is 
            // populated as _encryptingCreds
            // If you have multiple RPs for the STS you would select the certificate that is specific to 
            // the RP that requests the token and then use that for _encryptingCreds
            scope.EncryptingCredentials = _encryptingCreds;

            return scope;
        }

        protected override System.Security.Claims.ClaimsIdentity GetOutputClaimsIdentity(System.Security.Claims.ClaimsPrincipal principal, System.IdentityModel.Protocols.WSTrust.RequestSecurityToken request, Scope scope)
        {
            ClaimsIdentity claims = new ClaimsIdentity();
            claims.AddClaims(principal.Claims);
            //add more claims here
            return claims;
        }
        
        /// <summary>
        /// Validates the appliesTo and throws an exception if the appliesTo is null or appliesTo contains some unexpected address.  
        /// </summary>
        void ValidateAppliesTo(EndpointReference appliesTo)
        {   
            if (appliesTo == null)
            {
                throw new InvalidRequestException("The appliesTo is null.");
            }

            bool allowed = true;

            //NOTE: You should not technically accept any relying party here.  You should have allowed urls stored somewhere, like a custom web.config section
            //and only set allowed to true if the appliesTo.Url matches one of the urls.  As is, any site anywhere in the world can authenticate this SSO Login site, which may/may not be desired.

            if (!allowed)
                throw new InvalidRequestException(String.Format("The relying party address is not valid. make sure the audience urls are configured."));
        }
    }
}