using MVCForum.SSOLogin.Models.Forms;
using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace MVCForum.SSOLogin.Controllers
{
    public class MainController : Controller
    {
        /// <summary>
        /// This method handles the login/authenticated/redirect logic for authenticating on this SSO Test Site
        /// Redirects while preserving the ENTIRE query string is a pain in MVC so I made one method handle it all without doing any redirects except redirecting to itself.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult DoWork(Login model)
        {
            string strWa = Request["wa"];
            if (Request.IsAuthenticated || (!string.IsNullOrEmpty(strWa) && strWa == "wsignout1.0"))
            {
                //If the current request is authenticated or if the current request is a signout request from a relying party the request will be passed off to Federation Request Processing
                //if the request is a sign in request the user will be redirect to the relying party signed in.  If it is a signout request this session will be killed and redirected back to the relying party signed out.
                FederatedPassiveSecurityTokenServiceOperations.ProcessRequest(System.Web.HttpContext.Current.Request, User as ClaimsPrincipal, WIF.ExampleSecurityTokenServiceConfiguration.Current.CreateSecurityTokenService(), System.Web.HttpContext.Current.Response);                
                return View("Authenticated"); //don't do any kind of return redirect here, as that will hijack the Response.Redirect that the above code triggered.
            }
            else
            {
                if (Request.HttpMethod == "POST")
                {
                    if (model != null)
                    {
                        if (ModelState.IsValid)
                        {
                            if (UserManager.Login(model.UserName, model.Password, model.RememberMe))
                            {
                                return Redirect(Request.Url.ToString());
                            }
                            else
                            {
                                if (model == null)
                                    model = new Login();
                                model.LoginError = "Invalid UserName or Password";
                            }
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    ModelState.Clear();
                    model = null;
                }
            }
            return View("Login", model);
        }    
    }
}