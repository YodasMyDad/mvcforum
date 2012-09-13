using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace MVCForum.Website.Installer
{
    public static class InstallerHelper
    {
        public static InstallerResult InstallMainForumTables()
        {
            // Setup the installer result
            var insResult = new InstallerResult {Result = true};
            string appVersion;

            try
            {
                // Get the app version to change to
                appVersion = HttpContext.Current.Application["Version"].ToString();
            }
            catch
            {
                // Error return the error
                insResult.Result = false;
                insResult.ResultMessage = "Unable to obtain MVC Forum version";
                return insResult;
            }

            try
            {
                // Now create the database tables
                var dbScriptPath = string.Format("~/Installer/Db/{0}/database.sql", appVersion);
                var connString = ConfigurationManager.ConnectionStrings["MVCForumContext"].ConnectionString;

                var file = new FileInfo(HttpContext.Current.Server.MapPath(dbScriptPath));
                var script = file.OpenText().ReadToEnd();

                var conn = new SqlConnection(connString);

                // split script on GO command
                IEnumerable<string> commandStrings = Regex.Split(script, "^\\s*GO\\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                conn.Open();
                foreach (var commandString in commandStrings)
                {
                    if (commandString.Trim() != "")
                    {
                        new SqlCommand(commandString, conn).ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
            catch (System.Exception)
            {
                insResult.Result = false;
                insResult.ResultMessage = "Error creating the database tables, check your web.config connection string is correct and the database user has the correct permissions";
                return insResult;
            }

            if(Utilities.ConfigUtils.UpdateAppSetting("MVCForumVersion", appVersion) == false)
            {
                insResult.Result = false;
                insResult.ResultMessage = string.Format("Error updating the {0} version number in the web.config, try updating it manually to {1} and restarting the site", "MVCForumContext", appVersion);
                return insResult;
            }
      
            insResult.Result = true;
            insResult.ResultMessage = "Congratulations, MVC Forum has installed successfully";
            return insResult;
        }

        public static void TouchWebConfig()
        {
            var webConfigPath = HttpContext.Current.Server.MapPath("~/web.config");
            var xDoc = new XmlDocument();
            xDoc.Load(webConfigPath);
            xDoc.Save(webConfigPath);
        }
    }

    public class InstallerResult
    {
        public string ResultMessage { get; set; }
        public bool Result { get; set; }
    }
}