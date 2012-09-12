using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace MVCForum.Website.Installer
{
    public static class InstallerHelper
    {
        public static bool CreateDatabaseTable()
        {
            try
            {
                // Get the app version to change to
                var appVersion = HttpContext.Current.Application["Version"].ToString();


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

                // Now update the web.config version
                // This should be enough to restart app
                Utilities.ConfigUtils.UpdateAppSetting("MVCForumVersion", appVersion);

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }


        }
    }
}