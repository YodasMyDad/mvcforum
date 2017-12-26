namespace MvcForum.Core.Utilities
{
    using System.Web.Hosting;
    using System.Xml;

    public static class InstallerHelper
    {
        public static void TouchWebConfig()
        {
            var webConfigPath = HostingEnvironment.MapPath("~/web.config");
            var xDoc = new XmlDocument();
            xDoc.Load(webConfigPath);
            xDoc.Save(webConfigPath);
        }

        public static string GetMainDatabaseFilePath(string appVersion)
        {
            return $"~/Installer/Db/{appVersion}/database.sql";
        }

        public static string GetUpdateDatabaseFilePath(string appVersion)
        {
            return $"~/Installer/Db/{appVersion}/Upgrade/upgrade.sql";
        }
    }

}