namespace MvcForum.Web
{
    using System;
    using System.Web;

    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
        }

        /// <summary>
        /// Dispose of anything we have put in the 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_EndRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            var context = application.Context;

            foreach (var v in context.Items.Values)
            {
                if (v is IDisposable disposableItem)
                {
                    disposableItem.Dispose();
                }
            }
        }
    }
}