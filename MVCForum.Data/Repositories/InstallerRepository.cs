using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
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

        /// <summary>
        /// Creates database tables and reports back with an installer result
        /// </summary>
        /// <param name="connectionStringOveride">If you don't want to run this sql against the default DB then pass in a different connection string here</param>
        /// <param name="sqlFilePath">Overide what SQL you want to run, pass in the file path here i.e. /myfolder/myscript.sql</param>
        /// <param name="currentVersion">The current app version</param>
        /// <returns></returns>
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
                filePath = string.IsNullOrEmpty(sqlFilePath) ? HttpContext.Current.Server.MapPath(InstallerHelper.GetMainDatabaseFilePath(currentVersion)) : sqlFilePath;
                var file = new FileInfo(filePath);
                script = file.OpenText().ReadToEnd();
            }
            catch (Exception ex)
            {
                insResult.Exception = ex;
                insResult.Message = string.Format("Error trying to read the SQL file '{0}' create db", filePath);
                insResult.Successful = false;
                return insResult;
            }



            using (var conn = new SqlConnection(connString))
            {
                // split script on GO command
                IEnumerable<string> commandStrings = Regex.Split(script, "^\\s*GO\\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

                conn.Open();
                foreach (var commandString in commandStrings)
                {
                    if (commandString.Trim() != "")
                    {
                        try
                        {
                            new SqlCommand(commandString, conn).ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            //NOTE: Surpress errors where tables already exist, and just carry on
                            if (!ex.Message.Contains("There is already an object named") &&
                                !ex.Message.Contains("Column already has a DEFAULT bound to it"))
                            {
                                insResult.Exception = ex;
                                insResult.Message = "Error trying to create the database tables. Check you have correct permissions in SQL Server";
                                insResult.Successful = false;
                                return insResult;   
                            }
                        }

                    }
                }
            }

            return insResult;
        }

    }
}
