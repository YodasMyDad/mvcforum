using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace MVCForum.Website.Controllers
{
    public class WIFSSOController : BaseController
    {
        #region Fields
        Modules.MVCForumWSFederationAuthenticationModule wsModule;
        private readonly IBannedEmailService _bannedEmailService;
        private readonly IBannedWordService _bannedWordService;
        #endregion

        #region Constructor
        public WIFSSOController(ILoggingService loggingService,
                                        IUnitOfWorkManager unitOfWorkManager,
                                        IMembershipService membershipService,
                                        ILocalizationService localizationService,
                                        IRoleService roleService,
                                        ISettingsService settingsService, IBannedEmailService bannedEmailService, IBannedWordService bannedWordService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            wsModule = (Modules.MVCForumWSFederationAuthenticationModule)FederatedAuthentication.WSFederationAuthenticationModule;
            if (wsModule == null)
                throw new InvalidOperationException("Unable to retrieve an instance of the WSFederationAuthenticationModule.  Either the module is missing, or it is not of type MVCForumWSFederationAuthenticationModule.");

            _bannedEmailService = bannedEmailService;
            _bannedWordService = bannedWordService;
        }
        #endregion

        #region Methods
        public ActionResult WIFSSOLogon(string returnUrl)
        {
            if (!Request.IsAuthenticated)
            {
                if (returnUrl == null)
                    returnUrl = Request.Url.ToString();
                else
                {
                    throw new NotImplementedException("TODO Handle returnUrl stuff");
                }
                return RedirectToIdentityProvider(returnUrl);
            }
            else
            {
                ClaimsPrincipal principal = HttpContext.User as ClaimsPrincipal;
                if (principal == null)
                    throw new InvalidOperationException("Unable to convert current user object into ClaimsPrincipal.");
                string userName = principal.Identity.Name;
                var emailClaim = principal.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault();
                var existingUser = MembershipService.GetUser(userName);
                if (existingUser == null)
                {
                    var newUserModel = new ViewModels.MemberAddViewModel
                    {
                        UserName = userName,
                        Email = emailClaim != null ? emailClaim.Value : null,
                        LoginType = LoginType.WindowsIdentityFoundation,
                        Password = StringUtils.RandomString(8),
                        Comment = "Imported from WIF SSO",
                        ReturnUrl = returnUrl
                    };
                    TempData[AppConstants.MemberRegisterViewModel] = newUserModel;
                    return RedirectToAction("SocialLoginValidator", "Members");
                }
                else
                {
                    //TODO UPDATE THE USER WITH NEW DATA!!
                    //TODO Redirect to Return Url
                    return RedirectToAction("Index", "Home", new { area = string.Empty });
                }
            }
        }
        public ActionResult RedirectToIdentityProvider(string returnUrl)
        {
            bool signInCancelled = false;
            var signInRequest = this.wsModule.GetIdentityProviderRedirectUrl("CFDFCA25-70D1-4A34-AE81-6ECA33E44117", returnUrl, false, out signInCancelled);
            if (!signInCancelled)
                return Redirect(signInRequest.RequestUrl);
            else
                return RedirectToAction("Index", "Home");
        }
        #endregion
    }
}