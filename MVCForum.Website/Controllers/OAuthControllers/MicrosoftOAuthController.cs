namespace MvcForum.Web.Controllers.OAuthControllers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Security;
    using Core;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Enums;
    using Core.Utilities;
    using Skybrud.Social.Microsoft;
    using Skybrud.Social.Microsoft.OAuth;
    using Skybrud.Social.Microsoft.Responses.Authentication;
    using Skybrud.Social.Microsoft.WindowsLive.Scopes;
    using ViewModels;
    using ViewModels.Member;

    public partial class MicrosoftOAuthController : BaseController
    {
        // Create new app - https://account.live.com/developers/applications/create
        // List of existing app - https://account.live.com/developers/applications/index

        public MicrosoftOAuthController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            ICacheService cacheService, IMvcForumContext context) :
            base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
        }

        public string ReturnUrl => string.Concat(SettingsService.GetSettings().ForumUrl.TrimEnd('/'),
            Url.Action("MicrosoftLogin"));

        public string AuthCode => Request.QueryString["code"];

        public string AuthState => Request.QueryString["state"];

        public string AuthError => Request.QueryString["error"];

        public string AuthErrorDescription => Request.QueryString["error_description"];

        public virtual ActionResult MicrosoftLogin()
        {
            var resultMessage = new GenericMessageViewModel();

            var input = new
            {
                Code = AuthCode,
                State = AuthState,
                Error = new
                {
                    HasError = !string.IsNullOrWhiteSpace(AuthError),
                    Text = AuthError,
                    ErrorDescription = AuthErrorDescription
                }
            };


            // Get the prevalue options
            if (string.IsNullOrWhiteSpace(ForumConfiguration.Instance.MicrosoftAppId) ||
                string.IsNullOrWhiteSpace(ForumConfiguration.Instance.MicrosoftAppSecret))
            {
                resultMessage.Message = "You need to add the Microsoft app credentials to the web.config";
                resultMessage.MessageType = GenericMessages.danger;
            }
            else
            {
                var client = new MicrosoftOAuthClient
                {
                    ClientId = ForumConfiguration.Instance.MicrosoftAppId,
                    ClientSecret = ForumConfiguration.Instance.MicrosoftAppSecret,
                    RedirectUri = ReturnUrl
                };

                // Session expired?
                if (input.State != null && Session[$"MvcForum_{input.State}"] == null)
                {
                    resultMessage.Message = "Session Expired";
                    resultMessage.MessageType = GenericMessages.danger;
                }

                // Check whether an error response was received from Microsoft
                if (input.Error.HasError)
                {
                    Session.Remove($"MvcForum_{input.State}");
                    resultMessage.Message = AuthErrorDescription;
                    resultMessage.MessageType = GenericMessages.danger;
                }

                // Redirect the user to the Microsoft login dialog
                if (string.IsNullOrWhiteSpace(input.Code))
                {
                    // Generate a new unique/random state
                    var state = Guid.NewGuid().ToString();

                    // Save the state in the current user session
                    Session[$"MvcForum_{state}"] = "/";

                    // Construct the authorization URL
                    var url = client.GetAuthorizationUrl(state, WindowsLiveScopes.Emails + WindowsLiveScopes.Birthday);

                    // Redirect the user
                    return Redirect(url);
                }

                // Exchange the authorization code for an access token
                MicrosoftTokenResponse accessTokenResponse;
                try
                {
                    Session.Remove($"MvcForum_{input.State}");
                    accessTokenResponse = client.GetAccessTokenFromAuthCode(input.Code);
                }
                catch (Exception ex)
                {
                    accessTokenResponse = null;
                    resultMessage.Message = $"Unable to acquire access token<br/>{ex.Message}";
                    resultMessage.MessageType = GenericMessages.danger;
                }


                try
                {
                    if (string.IsNullOrWhiteSpace(resultMessage.Message) || accessTokenResponse != null)
                    {
                        //MicrosoftScope debug = accessTokenResponse.Body.Scope.Items;

                        //accessTokenResponse.Body.AccessToken
                        //foreach (MicrosoftScope scope in accessTokenResponse.Body.Scope.Items) {
                        //    scope
                        //}
                        //accessTokenResponse.Response.Body

                        // Initialize a new MicrosoftService so we can make calls to the API
                        var service = MicrosoftService.CreateFromAccessToken(accessTokenResponse.Body.AccessToken);

                        // Make the call to the Windows Live API / endpoint
                        var response = service.WindowsLive.GetSelf();

                        // Get a reference to the response body
                        var user = response.Body;

                        var getEmail = !string.IsNullOrWhiteSpace(user.Emails?.Preferred);
                        if (!getEmail)
                        {
                            resultMessage.Message =
                                LocalizationService.GetResourceString("Members.UnableToGetEmailAddress");
                            resultMessage.MessageType = GenericMessages.danger;
                            ShowMessage(resultMessage);
                            return RedirectToAction("LogOn", "Members");
                        }


                        var userExists = MembershipService.GetUserByEmail(user.Emails.Preferred);
                        if (userExists != null)
                        {
                            try
                            {
                                // Users already exists, so log them in
                                FormsAuthentication.SetAuthCookie(userExists.UserName, true);
                                resultMessage.Message =
                                    LocalizationService.GetResourceString("Members.NowLoggedIn");
                                resultMessage.MessageType = GenericMessages.success;
                                ShowMessage(resultMessage);
                                return RedirectToAction("Index", "Home");
                            }
                            catch (Exception ex)
                            {
                                LoggingService.Error(ex);
                            }
                        }
                        else
                        {
                            // Not registered already so register them
                            var viewModel = new MemberAddViewModel
                            {
                                Email = user.Emails.Preferred,
                                LoginType = LoginType.Microsoft,
                                Password = StringUtils.RandomString(8),
                                UserName = user.Name,
                                UserAccessToken = accessTokenResponse.Body.AccessToken,
                                SocialProfileImageUrl = $"https://apis.live.net/v5.0/{user.Id}/picture"
                            };

                            //var uri = string.Concat("https://apis.live.net/v5.0/me?access_token=",viewModel.UserAccessToken);
                            //using (var dl = new WebClient())
                            //{
                            //    var profile = JObject.Parse(dl.DownloadString(uri));
                            //    var pictureUrl = ;
                            //    if (!string.IsNullOrWhiteSpace(pictureUrl))
                            //    {
                            //        //viewModel.SocialProfileImageUrl = getImageUrl;
                            //    }
                            //}


                            // Store the viewModel in TempData - Which we'll use in the register logic
                            TempData[Constants.MemberRegisterViewModel] = viewModel;

                            return RedirectToAction("SocialLoginValidator", "Members");
                        }
                    }
                    else
                    {
                        resultMessage.MessageType = GenericMessages.danger;
                        ShowMessage(resultMessage);
                        return RedirectToAction("LogOn", "Members");
                    }
                }
                catch (Exception ex)
                {
                    resultMessage.Message = $"Unable to get user information<br/>{ex.Message}";
                    resultMessage.MessageType = GenericMessages.danger;
                    LoggingService.Error(ex);
                }
            }


            ShowMessage(resultMessage);
            return RedirectToAction("LogOn", "Members");
        }
    }
}