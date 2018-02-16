namespace MvcForum.Core.ExtensionMethods
{
    using System;
    using System.Reflection;

    public static class VersionExtensions
    {
        #region Version

        /// <summary>
        ///     Gets the main version number (Used by installer)
        /// </summary>
        /// <returns></returns>
        public static decimal GetCurrentVersionNo()
        {
            //Installer for new versions and first startup
            // Get the current version
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Store the value for use in the app
            return Convert.ToDecimal($"{version.Major}.{version.Minor}");
        }

        /// <summary>
        ///     Get the full version number shown in the admin
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentVersionNoFull()
        {
            //Installer for new versions and first startup
            // Get the current version
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Store the value for use in the app
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        #endregion
    }
}