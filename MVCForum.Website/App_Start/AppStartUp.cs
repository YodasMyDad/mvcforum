using MVCForum.IOC;
using MVCForum.Website;

[assembly: WebActivator.PreApplicationStartMethod(typeof(AppStartUp), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(AppStartUp), "Stop")]

namespace MVCForum.Website
{
    public static class AppStartUp
    {
        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
           UnityHelper.Start();
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
