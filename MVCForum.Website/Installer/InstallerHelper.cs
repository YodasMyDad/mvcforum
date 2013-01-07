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

        #region Main table install

        public static InstallerResult RunSql(string filePath)
        {
            // Setup the installer result
            var insResult = new InstallerResult { Result = true };

            try
            {
                // Now create the database tables
                var dbScriptPath = filePath;
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
                insResult.ResultMessage = "Error creating/updating the database, check your web.config connection string is correct and the database user has the correct permissions";
                return insResult;
            }

            return insResult;
        } 

        #endregion


        #region Helpers

        public static void TouchWebConfig()
        {
            var webConfigPath = HttpContext.Current.Server.MapPath("~/web.config");
            var xDoc = new XmlDocument();
            xDoc.Load(webConfigPath);
            xDoc.Save(webConfigPath);
        }

        public static string GetMainDatabaseFilePath(string appVersion)
        {
            return string.Format("~/Installer/Db/{0}/database.sql", appVersion);
        }

        public static string GetUpdateDatabaseFilePath(string appVersion)
        {
            return string.Format("~/Installer/Db/{0}/Upgrade/upgrade.sql", appVersion);
        }

        #endregion

    }

    public class InstallerResult
    {
        public string ResultMessage { get; set; }
        public bool Result { get; set; }
    }
}