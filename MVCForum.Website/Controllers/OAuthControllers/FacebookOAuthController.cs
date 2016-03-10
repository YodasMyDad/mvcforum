using System;
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
using Skybrud.Social.Facebook;
using Skybrud.Social.Facebook.OAuth;
using Skybrud.Social.Facebook.Options.User;

namespace MVCForum.Website.Controllers.OAuthControllers
{
    // Facebook uses OAuth 2.0 for authentication and communication. In order for users to authenticate with the Facebook API, 
    // you must specify the ID, secret and redirect URI of your Facebook app. 
    // You can create a new app at the following URL: https://developers.facebook.com/

    public partial class FacebookOAuthController : BaseController
    {
        public FacebookOAuthController(ILoggingService loggingService, 
                                        IUnitOfWorkManager unitOfWorkManager, 
                                        IMembershipService membershipService, 
                                        ILocalizationService localizationService, 
                                        IRoleService roleService, 
                                        ISettingsService settingsService) : base(loggingService, 
                                                                                unitOfWorkManager, 
                                                                                membershipService, 
                                                                                localizationService, 
                                                                                roleService, 
                                                                                settingsService)
        {

        }

        public string ReturnUrl
        {
            get
            {
                return string.Concat(SettingsService.GetSettings().ForumUrl.TrimEnd('/'), Url.Action("FacebookLogin"));
            }
        }

        public string Callback { get; private set; }

        public string ContentTypeAlias { get; private set; }

        public string PropertyAlias { get; private set; }

        /// <summary>
        /// Gets the authorizing code from the query string (if specified).
        /// </summary>
        public string AuthCode
        {
            get { return Request.QueryString["code"]; }
        }

        public string AuthState
        {
            get { return Request.QueryString["state"]; }
        }

        public string AuthErrorReason
        {
            get { return Request.QueryString["error_reason"]; }
        }

        public string AuthError
        {
            get { return Request.QueryString["error"]; }
        }

        public string AuthErrorDescription
        {
            get { return Request.QueryString["error_description"]; }
        }

        public ActionResult FacebookLogin()
        {
            var resultMessage = new GenericMessageViewModel();

            Callback = Request.QueryString["callback"];
            ContentTypeAlias = Request.QueryString["contentTypeAlias"];
            PropertyAlias = Request.QueryString["propertyAlias"];

            if (AuthState != null)
            {
                var stateValue = Session["MVCForum_" + AuthState] as string[];
                if (stateValue != null && stateValue.Length == 3)
                {
                    Callback = stateValue[0];
                    ContentTypeAlias = stateValue[1];
                    PropertyAlias = stateValue[2];
                }
            }

            // Get the prevalue options
            if (string.IsNullOrEmpty(SiteConstants.Instance.FacebookAppId) ||
                string.IsNullOrEmpty(SiteConstants.Instance.FacebookAppSecret))
            {
                resultMessage.Message = "You need to add the Facebook app credentials";
                resultMessage.MessageType = GenericMessages.danger;
            }
            else
            {

                // Settings valid move on
                // Configure the OAuth client based on the options of the prevalue options
                var client = new FacebookOAuthClient
                {
                    AppId = SiteConstants.Instance.FacebookAppId,
                    AppSecret = SiteConstants.Instance.FacebookAppSecret,
                    RedirectUri = ReturnUrl
                };

                // Session expired?
                if (AuthState != null && Session["MVCForum_" + AuthState] == null)
                {
                    resultMessage.Message = "Session Expired";
                    resultMessage.MessageType = GenericMessages.danger;
                }

                // Check whether an error response was received from Facebook
                if (AuthError != null)
                {
                    Session.Remove("MVCForum_" + AuthState);
                    resultMessage.Message = AuthErrorDescription;
                    resultMessage.MessageType = GenericMessages.danger;
                }

                // Redirect the user to the Facebook login dialog
                if (AuthCode == null)
                {
                    // Generate a new unique/random state
                    var state = Guid.NewGuid().ToString();

                    // Save the state in the current user session
                    Session["MVCForum_" + state] = new[] { Callback, ContentTypeAlias, PropertyAlias };

                    // Construct the authorization URL
                    var url = client.GetAuthorizationUrl(state, "public_profile", "email"); //"user_friends"

                    // Redirect the user
                    return Redirect(url);
                }

                // Exchange the authorization code for a user access token
                var userAccessToken = string.Empty;
                try
                {
                    userAccessToken = client.GetAccessTokenFromAuthCode(AuthCode);
                }
                catch (Exception ex)
                {
                    resultMessage.Message = string.Format("Unable to acquire access token<br/>{0}", ex.Message);
                    resultMessage.MessageType = GenericMessages.danger;
                }

                try
                {
                    if (string.IsNullOrEmpty(resultMessage.Message))
                    {
                        // Initialize the Facebook service (no calls are made here)
                        var service = FacebookService.CreateFromAccessToken(userAccessToken);

                        // Declare the options for the call to the API
                        var options = new FacebookGetUserOptions
                        {
                            Identifier = "me",
                            Fields = new[] { "id", "name", "email", "first_name", "last_name", "gender" }
                        };

                        var user = service.Users.GetUser(options);

                        // Try to get the email - Some FB accounts have protected passwords
                        var email = user.Body.Email;
                        if (string.IsNullOrEmpty(email))
                        {
                            resultMessage.Message = LocalizationService.GetResourceString("Members.UnableToGetEmailAddress");
                            resultMessage.MessageType = GenericMessages.danger;
                            ShowMessage(resultMessage);
                            return RedirectToAction("LogOn", "Members");
                        }

                        // First see if this user has registered already - Use email address
                        using (UnitOfWorkManager.NewUnitOfWork())
                        {
                            var userExists = MembershipService.GetUserByEmail(email);

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
                                    Email = email,
                                    LoginType = LoginType.Facebook,
                                    Password = StringUtils.RandomString(8),
                                    UserName = user.Body.Name,
                                    UserAccessToken = userAccessToken
                                };

                                // Get the image and save it
                                var getImageUrl = string.Format("http://graph.facebook.com/{0}/picture?type=square", user.Body.Id);
                                viewModel.SocialProfileImageUrl = getImageUrl;

                                //Large size photo https://graph.facebook.com/{facebookId}/picture?type=large
                                //Medium size photo https://graph.facebook.com/{facebookId}/picture?type=normal
                                //Small size photo https://graph.facebook.com/{facebookId}/picture?type=small
                                //Square photo https://graph.facebook.com/{facebookId}/picture?type=square

                                // Store the viewModel in TempData - Which we'll use in the register logic
                                TempData[AppConstants.MemberRegisterViewModel] = viewModel;

                                return RedirectToAction("SocialLoginValidator", "Members");
                            }
                        }
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