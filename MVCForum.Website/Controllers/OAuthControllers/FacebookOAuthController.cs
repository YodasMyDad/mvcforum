using System;
using System.Web.Mvc;
using System.Web.Security;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using MVCForum.Website.ViewModels.OAuth;
using Skybrud.Social.Facebook;
using Skybrud.Social.Facebook.OAuth;
using Skybrud.Social.Json;

namespace MVCForum.Website.Controllers.OAuthControllers
{
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
                return string.Concat(SettingsService.GetSettings().ForumUrl, Url.Action("FacebookLogin"));
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
            if (string.IsNullOrEmpty(SiteConstants.FacebookAppId) ||
                string.IsNullOrEmpty(SiteConstants.FacebookAppSecret))
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
                    AppId = SiteConstants.FacebookAppId,
                    AppSecret = SiteConstants.FacebookAppSecret,
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

                        // Hack to get email
                        // Get the raw string and parse it
                        // we use this to get items manually including the email
                        var response = service.Methods.Raw.Me();
                        var obj = JsonObject.ParseJson(response);

                        // Make a call to the Facebook API to get information about the user
                        var me = service.Methods.Me();

                        // Get debug information about the access token
                        //var debug = service.Methods.DebugToken(userAccessToken);

                        // Set the callback data
                        var data = new FacebookOAuthData
                        {
                            Id = me.Id,
                            Name = me.Name ?? me.UserName,
                            AccessToken = userAccessToken,
                            //ExpiresAt = debug.ExpiresAt == null ? default(DateTime) : debug.ExpiresAt.Value,
                            //Scope = debug.Scopes
                        };

                        // Try to get the email - Some FB accounts have protected passwords
                        var email = obj.GetString("email");
                        if (!string.IsNullOrEmpty(email))
                        {
                            data.Email = email;
                        }

                        // First see if this user has registered already - Use email address
                        using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                        {
                            // TODO - Ignore if no email - Have to check PropMemberFacebookAccessToken has a value
                            // TODO - and the me.UserName is there to match existing logged in accounts
                            var userExists = MembershipService.GetUserByEmail(data.Email);

                            if (userExists != null)
                            {
                                try
                                {
                                    // Users already exists, so log them in
                                    FormsAuthentication.SetAuthCookie(userExists.UserName, true);
                                    resultMessage.Message = LocalizationService.GetResourceString("Members.NowLoggedIn");
                                    resultMessage.MessageType = GenericMessages.success;
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
                                    Email = data.Email,
                                    LoginType = LoginType.Facebook,
                                    Password = StringUtils.RandomString(8),
                                    UserName = data.Name,
                                    UserAccessToken = userAccessToken
                                };

                                // Get the image and save it
                                var getImageUrl = string.Format("http://graph.facebook.com/{0}/picture?type=square", me.Id);
                                viewModel.SocialProfileImageUrl = getImageUrl;

                                //Large size photo https://graph.facebook.com/{facebookId}/picture?type=large
                                //Medium size photo https://graph.facebook.com/{facebookId}/picture?type=normal
                                //Small size photo https://graph.facebook.com/{facebookId}/picture?type=small
                                //Square photo https://graph.facebook.com/{facebookId}/picture?type=square

                                return RedirectToAction("MemberRegisterLogic", "Members", new { userModel = viewModel, unitOfWork });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    resultMessage.Message = string.Format("Unable to get user information<br/>{0}", ex.Message);
                    resultMessage.MessageType = GenericMessages.danger;
                }

            }

            ShowMessage(resultMessage);
            return RedirectToAction("LogOn", "Members");
        }
    }
}