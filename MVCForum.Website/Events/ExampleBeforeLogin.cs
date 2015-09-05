using System;
using System.Web;
using System.Web.Security;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Events;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Website.Application;

namespace MVCForum.Website.Events
{
    // In this example I am adding an event to intercept when someone tries to login
    // The example below would be for a single sign on solution - Where you verify the user against a seperate 
    // database and log them in.
    public class ExampleBeforeLogin : IEventHandler 
    {
        // Register the events here
        public void RegisterHandlers(EventManager theEventManager)
        {
            // TODO - Uncomment this line below to fire the method
            //theEventManager.BeforeLogin += BeforeLogin;
        }

        // Method that's fired when the event is raised
        private void BeforeLogin(object sender, LoginEventArgs e)
        {
            // Firstly, I'm going to cancel the event (Optional)
            e.Cancel = true;

            // Sender is the MembersController            
            //var membersController = sender as MembersController;

            // Here I would go off to a webservice, API or custom code and check the username and password is correct
            // against the other database. if it is log them in
            //TODO - Go validate e.UserName and e.Password                                    
            
            // If ok check the user exists and log the user in using the details below
            FormsAuthentication.SetAuthCookie(e.UserName, e.RememberMe);

            // Get membership service - you must create the member in MVCForum if they don't exist
            // Or you'll get an error when they have been redirected to the home page and logged in
            // this is pretty simple to do once we have the member service
            var memberService = ServiceFactory.Get<IMembershipService>();
            var user = memberService.GetUser(e.UserName);
            if (user == null)
            {
                // Create new member here
            }
          
            // Commit any changes you have made
            try
            {
                e.UnitOfWork.Commit();
                
                // Redirect to the home page (Or wherever)
                HttpContext.Current.Response.Redirect("~/");
            }
            catch (Exception)
            {
                e.UnitOfWork.Rollback();
            }

        }
    }
}