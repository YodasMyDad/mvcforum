using System;
using System.Net;
using System.Web.Mvc;
using System.Web.Security;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using Skybrud.Social.Microsoft;
using Skybrud.Social.Microsoft.OAuth;
using Skybrud.Social.Microsoft.Responses.Authentication;
using Skybrud.Social.Microsoft.Scopes;
using Skybrud.Social.Microsoft.WindowsLive.Scopes;

namespace MVCForum.Website.Controllers.OAuthControllers
{
    public class MicrosoftOAuthController : BaseController
    {

        // Create new app - https://account.live.com/developers/applications/create
        // List of existing app - https://account.live.com/developers/applications/index

        public MicrosoftOAuthController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService) :
            base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
        }

        public string ReturnUrl
        {
            get
            {
                return string.Concat(SettingsService.GetSettings().ForumUrl.TrimEnd('/'), Url.Action("MicrosoftLogin"));
            }
        }

        public string AuthCode
        {
            get { return Request.QueryString["code"]; }
        }

        public string AuthState
        {
            get { return Request.QueryString["state"]; }
        }

        public string AuthError
        {
            get { return Request.QueryString["error"]; }
        }

        public string AuthErrorDescription
        {
            get { return Request.QueryString["error_description"]; }
        }

        public ActionResult MicrosoftLogin()
        {
            var resultMessage = new GenericMessageViewModel();

            var input = new
            {
                Code = AuthCode,
                State = AuthState,
                Error = new
                {
                    HasError = !String.IsNullOrWhiteSpace(AuthError),
                    Text = AuthError,
                    ErrorDescription = AuthErrorDescription
                }
            };


            // Get the prevalue options
            if (string.IsNullOrEmpty(SiteConstants.Instance.MicrosoftAppId) ||
                string.IsNullOrEmpty(SiteConstants.Instance.MicrosoftAppSecret))
            {
                resultMessage.Message = "You need to add the Microsoft app credentials to the web.config";
                resultMessage.MessageType = GenericMessages.danger;
            }
            else
            {

                var client = new MicrosoftOAuthClient
                {
                    ClientId = SiteConstants.Instance.MicrosoftAppId,
                    ClientSecret = SiteConstants.Instance.MicrosoftAppSecret,
                    RedirectUri = ReturnUrl
                };

                // Session expired?
                if (input.State != null && Session["MVCForum_" + input.State] == null)
                {
                    resultMessage.Message = "Session Expired";
                    resultMessage.MessageType = GenericMessages.danger;
                }

                // Check whether an error response was received from Microsoft
                if (input.Error.HasError)
                {
                    Session.Remove("MVCForum_" + input.State);
                    resultMessage.Message = AuthErrorDescription;
                    resultMessage.MessageType = GenericMessages.danger;
                }

                // Redirect the user to the Microsoft login dialog
                if (string.IsNullOrWhiteSpace(input.Code))
                {

                    // Generate a new unique/random state
                    var state = Guid.NewGuid().ToString();

                    // Save the state in the current user session
                    Session["MVCForum_" + state] = "/";

                    // Construct the authorization URL
                    var url = client.GetAuthorizationUrl(state, WindowsLiveScopes.Emails + WindowsLiveScopes.Birthday);

                    // Redirect the user
                    return Redirect(url);
                }

                // Exchange the authorization code for an access token
                MicrosoftTokenResponse accessTokenResponse;
                try
                {
                    Session.Remove("MVCForum_" + input.State);
                    accessTokenResponse = client.GetAccessTokenFromAuthCode(input.Code);
                }
                catch (Exception ex)
                {
                    accessTokenResponse = null;
                    resultMessage.Message = string.Format("Unable to acquire access token<br/>{0}", ex.Message);
                    resultMessage.MessageType = GenericMessages.danger;
                }


                try
                {
                    if (string.IsNullOrEmpty(resultMessage.Message) || accessTokenResponse != null)
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

                        var getEmail = user.Emails != null && !string.IsNullOrWhiteSpace(user.Emails.Preferred);
                        if (!getEmail)
                        {
                            resultMessage.Message = LocalizationService.GetResourceString("Members.UnableToGetEmailAddress");
                            resultMessage.MessageType = GenericMessages.danger;
                            ShowMessage(resultMessage);
                            return RedirectToAction("LogOn", "Members");
                        }

                        using (UnitOfWorkManager.NewUnitOfWork())
                        {
                            var userExists = MembershipService.GetUserByEmail(user.Emails.Preferred);

                            if (userExists != null)
                            {
                                try
                                {
                                    // Users already exists, so log them in
                                    FormsAuthentication.SetAuthCookie(userExists.UserName, true);
                                    resultMessage.Message = LocalizationService.GetResourceString("Members.NowLoggedIn");
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
                                //    if (!string.IsNullOrEmpty(pictureUrl))
                                //    {
                                //        //viewModel.SocialProfileImageUrl = getImageUrl;
                                //    }
                                //}


                                // Store the viewModel in TempData - Which we'll use in the register logic
                                TempData[AppConstants.MemberRegisterViewModel] = viewModel;

                                return RedirectToAction("SocialLoginValidator", "Members");
                            }
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
                    resultMessage.Message = string.Format("Unable to get user information<br/>{0}", ex.Message);
                    resultMessage.MessageType = GenericMessages.danger;
                    LoggingService.Error(ex);
                }



            }


            ShowMessage(resultMessage);
            return RedirectToAction("LogOn", "Members");
        }
    }
}