﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    /// <summary>
    /// A class for logging errors to a text file. Works in Partial Trust.
    /// </summary>
    public partial class LoggingService : ILoggingService
    {
        private const string LogFileNameOnly = @"LogFile";
        private const string LogFileExtension = @".txt";
        private const string LogFileDirectory = @"~/App_Data";

        private const string DateTimeFormat = @"dd/MM/yyyy HH:mm:ss";
        private static readonly Object LogLock = new Object();
        private static string _logFileFolder;
        private static int _maxLogSize = 10000;
        private static string _logFileName;

        /// <summary>
        /// Constructor
        /// </summary>
        public LoggingService()
        {
            // If we have no http context current then assume testing mode i.e. log file in run folder
            //_logFileFolder = HttpContext.Current != null ? HttpContext.Current.Server.MapPath(LogFileDirectory) : @".";
            _logFileFolder = System.Web.Hosting.HostingEnvironment.MapPath(LogFileDirectory);            
            _logFileName = MakeLogFileName(false);
        }

        #region Private static methods

        /// <summary>
        /// Generate a full log file name
        /// </summary>
        /// <param name="isArchive">If this an archive file, make the usual file name but append a timestamp</param>
        /// <returns></returns>
        private static string MakeLogFileName(bool isArchive)
        {
            return !isArchive ? $"{_logFileFolder}//{LogFileNameOnly}{LogFileExtension}"
                : $"{_logFileFolder}//{LogFileNameOnly}_{DateTime.UtcNow.ToString("ddMMyyyy_hhmmss")}{LogFileExtension}";
        }

        /// <summary>
        /// Gets the file size, in medium trust
        /// </summary>
        /// <returns></returns>
        private static long Length()
        {
            // FileInfo not happy in medoum trust so just open the file
            using (var fs = File.OpenRead(_logFileName))
            {
                return fs.Length;
            }
        }

        /// <summary>
        /// Perform the write. Thread-safe.
        /// </summary>
        /// <param name="message"></param>
        private static void Write(string message)
        {
            if (message != "File does not exist.")
            {
                try
                {
                    // Note there is a lock here. This class is only suitable for error logging,
                    // not ANY form of trace logging...
                    lock (LogLock)
                    {
                        if (Length() >= _maxLogSize)
                        {
                            ArchiveLog();
                        }

                        using (var tw = TextWriter.Synchronized(File.AppendText(_logFileName)))
                        {
                            var callStack = new StackFrame(2, true); // Go back one stack frame to get module info

                            tw.WriteLine("{0} | {1} | {2} | {3} | {4} | {5}", DateTime.UtcNow.ToString(DateTimeFormat), callStack.GetMethod().Module.Name, callStack.GetMethod().Name, callStack.GetMethod().DeclaringType, callStack.GetFileLineNumber(), message);
                        }
                    }
                }
                catch
                {
                    // Not much to do if logging failed...
                } 
            }
        }

        /// <summary>
        /// Move file to archive
        /// </summary>
        private static void ArchiveLog()
        {
            // Move file
            File.Copy(_logFileName, MakeLogFileName(true));
            File.Delete(_logFileName);

            // Recreate file
            CheckFileExists(_maxLogSize);
        }

        /// <summary>
        /// Create file if it doesn't exist
        /// </summary>
        /// <param name="maxLogSize"></param>
        private static void CheckFileExists(int maxLogSize)
        {
            _maxLogSize = maxLogSize;

            if (!File.Exists(_logFileName))
            {
                using (File.Create(_logFileName))
                {
                    // Ensures is closed again after creation
                }
            }
        }

        /// <summary>
        /// Formats a log line into a readable line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static LogEntry LogLineFormatter(string line)
        {
            try
            {
                var lineSplit = line.Split('|');

                return new LogEntry
                              {
                                  Date = DateTime.ParseExact(lineSplit[0].Trim(), DateTimeFormat, CultureInfo.InvariantCulture),
                                  Module = lineSplit[1].Trim(),
                                  Method = lineSplit[2].Trim(),
                                  DeclaringType = lineSplit[3].Trim(),
                                  LineNumber = lineSplit[4].Trim(),
                                  ErrorMessage = lineSplit[5].Trim(),
                              };                
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a list of log entries from the log file
        /// </summary>
        /// <returns></returns>
        private static List<LogEntry> ReadLogFile()
        {
            // create empty log list
            var logs = new List<LogEntry>();

            // Read the file and display it line by line.
            using (var file = new StreamReader(_logFileName, Encoding.UTF8, true))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    var logline = LogLineFormatter(line);
                    if (logline != null)
                    {
                        logs.Add(logline);
                    }
                }
            }

            // Order and take a max of 1000 entries
            return logs.OrderByDescending(x => x.Date).Take(100).ToList();
        }

        #endregion

        /// <summary>
        /// Initialise the logging. Checks to see if file exists, so best 
        /// called ONCE from an application entry point to avoid threading issues
        /// </summary>
        public void Initialise(int maxLogSize)
        {
            CheckFileExists(maxLogSize);
        }

        /// <summary>
        /// Force creation of a new log file
        /// </summary>
        public void Recycle()
        {
            ArchiveLog();
        }

        public void ClearLogFiles()
        {
            ArchiveLog();
        }

        /// <summary>
        /// Logs an error based log with a message
        /// </summary>
        /// <param name="message"></param>
        public void Error(string message)
        {
            Write(message);
        }

        /// <summary>
        /// Logs an error based log with an exception
        /// </summary>
        /// <param name="ex"></param>
        public void Error(Exception ex)
        {
            const int maxExceptionDepth = 5;

            if (ex == null)
            {
                return;
            }

            var message = new StringBuilder(ex.Message);

            var inner = ex.InnerException;
            var depthCounter = 0;
            while (inner != null && depthCounter++ < maxExceptionDepth)
            {
                message.Append(" INNER EXCEPTION: ");
                message.Append(inner.Message);
                inner = inner.InnerException;
            }

            Write(message.ToString());
        }

        /// <summary>
        /// Returns all logs in the log file
        /// </summary>
        /// <returns></returns>
        public IList<LogEntry> ListLogFile()
        {
            return ReadLogFile();
        }
    }
}
