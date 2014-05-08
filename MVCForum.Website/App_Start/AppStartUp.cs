using MVCForum.IOC;
using MVCForum.Website.App_Start;

[assembly: WebActivator.PreApplicationStartMethod(typeof(AppStartUp), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(AppStartUp), "Stop")]

namespace MVCForum.Website.App_Start
{
    public static class AppStartUp
    {
        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
           UnityMVC3.Start();
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
          // Nothing to do
        }
    }
}
