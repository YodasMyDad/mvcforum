using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace MVCForum.OpenAuth
{
    public class TwitterClient
    {
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public string SecretToken { get; set; }
        private string CallBackUrl { get; set; }

        private static readonly ServiceProviderDescription ServiceDescription =
            new ServiceProviderDescription
            {
                RequestTokenEndpoint = new MessageReceivingEndpoint(
                                           "https://api.twitter.com/oauth/request_token",
                                           HttpDeliveryMethods.GetRequest |
                                           HttpDeliveryMethods.AuthorizationHeaderRequest),

                UserAuthorizationEndpoint = new MessageReceivingEndpoint(
                                          "https://api.twitter.com/oauth/authorize",
                                          HttpDeliveryMethods.GetRequest |
                                          HttpDeliveryMethods.AuthorizationHeaderRequest),

                AccessTokenEndpoint = new MessageReceivingEndpoint(
                                          "https://api.twitter.com/oauth/access_token",
                                          HttpDeliveryMethods.GetRequest |
                                          HttpDeliveryMethods.AuthorizationHeaderRequest),

                TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
            };

        readonly IConsumerTokenManager _tokenManager;

        public TwitterClient(IConsumerTokenManager tokenManager, string callBackUrl)
        {
            _tokenManager = tokenManager;
            CallBackUrl = callBackUrl;
        }

        public void StartAuthentication()
        {
            var request = HttpContext.Current.Request;
            using (var twitter = new WebConsumer(ServiceDescription, _tokenManager))
            {
                var callBackUrl = new Uri(request.Url.Scheme + "://" + request.Url.Authority + CallBackUrl);
                twitter.Channel.Send(
                    twitter.PrepareRequestUserAuthorization(callBackUrl, null, null)
                );
            }
        }

        public bool FinishAuthentication()
        {
            using (var twitter = new WebConsumer(ServiceDescription, _tokenManager))
            {
                var accessTokenResponse = twitter.ProcessUserAuthorization();
                if (accessTokenResponse != null)
                {
                    AccessToken = accessTokenResponse.AccessToken;
                    SecretToken = _tokenManager.GetTokenSecret(AccessToken);
                    UserName = accessTokenResponse.ExtraData["screen_name"];
                    return true;
                }
            }

            return false;
        }
    }
}
