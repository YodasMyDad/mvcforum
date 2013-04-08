using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Utilities;

namespace MVCForum.Data.Repositories
{
    public class InstallerRepository : IInstallerRepository
    {
        private readonly MVCForumContext _context;
        public InstallerRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public InstallerResult CreateDbTables(string connectionStringOveride, string sqlFilePath, string currentVersion)
        {
            // Setup the installer result
            var insResult = new InstallerResult { Successful = true, Message = "Successfully created the database tables" };

            // Sort the connection string out
            string connString;
            try
            {
                // Set the connection string
                connString = connectionStringOveride ?? ConfigurationManager.ConnectionStrings["MVCForumContext"].ConnectionString;
            }
            catch (Exception ex)
            {
                insResult.Exception = ex;
                insResult.Message = "Error trying to get the connection string";
                insResult.Successful = false;
                return insResult;
            }

            // Get and open the SQL 
            string script;
            var filePath = string.Empty;
            try
            {
                filePath = sqlFilePath ?? InstallerHelper.GetMainDatabaseFilePath(currentVersion);
                var file = new FileInfo(HttpContext.Current.Server.MapPath(filePath));
                script = file.OpenText().ReadToEnd();
            }
            catch (Exception ex)
            {
                insResult.Exception = ex;
                insResult.Message = string.Format("Error trying to read the SQL file '{0}' create db", filePath);
                insResult.Successful = false;
                return insResult;
            }


            try
            {
                using (var conn = new SqlConnection(connString))
                {
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
                }
            }
            catch (Exception ex)
            {
                insResult.Exception = ex;
                insResult.Message = "Error trying to create the database tables. Check SQL file is not corrupted, also check you have correct permissions in SQL Server to create tables";
                insResult.Successful = false;
                return insResult;
            }

            return insResult;
        }

    }
}
