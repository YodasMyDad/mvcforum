﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public partial class ImportExportController : BaseAdminController
    {
        private readonly ILocalizationService _localizationService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"> </param>
        /// <param name="localizationService"></param>
        /// <param name="settingsService"> </param>
        /// <param name="loggingService"> </param>
        public ImportExportController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, 
            IMembershipService membershipService, ILocalizationService localizationService, ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _localizationService = localizationService;
        }

        #region Private methods

        /// <summary>
        /// Convert an import report into JSON data
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        private static object ToJSON(CsvReport report)
        {
            var oSerializer = new JavaScriptSerializer();
            var json = new
            {
                HasErrors = report.Errors.Any(),
                HasWarnings = report.Warnings.Any(),
                Warnings = oSerializer.Serialize(report.Warnings.ExtractMessages()),
                Errors = oSerializer.Serialize(report.Errors.ExtractMessages()),
            };

            return json;
        }


        #endregion

        /// <summary>
        /// We get here via the admin default layout (_AdminLayout). The returned view is displayed by
        /// the @RenderBody in that layout
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Export a language in csv format
        /// </summary>
        /// <returns></returns>
        public CsvFileResult ExportUsers()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                return new CsvFileResult { FileDownloadName = "MVCForumUsers.csv", Body = MembershipService.ToCsv() };
            }
        }

        /// <summary>
        /// Post of data for users import (file info)
        /// </summary>
        /// <param name="file">The name-value pairs for the language content</param>
        /// <returns></returns>
        [HttpPost]
        public WrappedJsonResult ImportUsers(HttpPostedFileBase file)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var report = new CsvReport();

                //http://www.dustinhorne.com/post/2011/11/16/AJAX-File-Uploads-with-jQuery-and-MVC-3.aspx
                try
                {
                    // Verify that the user selected a file
                    if (file != null && file.ContentLength > 0)
                    {
                        // Unpack the data
                        var allLines = new List<string>();
                        using (var streamReader = new StreamReader(file.InputStream, System.Text.Encoding.UTF8, true))
                        {
                            while (streamReader.Peek() >= 0)
                            {
                                allLines.Add(streamReader.ReadLine());
                            }
                        }

                        // Read the CSV file and generate a language
                        report = MembershipService.FromCsv(allLines);
                        unitOfWork.Commit();
                    }
                    else
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.BadDataFormat,
                            Message = "File does not contain any users."
                        });
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    report.Errors.Add(new CsvErrorWarning
                    {
                        ErrorWarningType = CsvErrorWarningType.GeneralError,
                        Message = string.Format("Unable to import users: {0}", ex.Message)
                    });
                }

                return new WrappedJsonResult { Data = ToJSON(report) };
            }
        }

        /// <summary>
        /// Export a language in csv format
        /// </summary>
        /// <param name="languageCulture"></param>
        /// <returns></returns>
        public CsvFileResult ExportLanguage(string languageCulture)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var csv = new CsvFileResult();

                var language = _localizationService.GetLanguageByLanguageCulture(languageCulture);

                if (language != null)
                {
                    csv.FileDownloadName = languageCulture + ".csv";
                    csv.Body = _localizationService.ToCsv(language);
                }
                else
                {
                    csv.Body = "No such language";
                    LoggingService.Error("No such language when trying to export language");
                }

                return csv; 
            }
        }

        /// <summary>
        /// Post of data for language import (file info)
        /// </summary>
        /// <param name="languageCulture">This defines the name etc of the imported language</param>
        /// <param name="file">The name-value pairs for the language content</param>
        /// <returns></returns>
        [HttpPost]
        public WrappedJsonResult ImportLanguage(string languageCulture, HttpPostedFileBase file)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var report = new CsvReport();

                //http://www.dustinhorne.com/post/2011/11/16/AJAX-File-Uploads-with-jQuery-and-MVC-3.aspx
                try
                {
                    // Verify that the user selected a file
                    if (file != null && file.ContentLength > 0)
                    {
                        // Unpack the data
                        var allLines = new List<string>();
                        using (var streamReader = new StreamReader(file.InputStream, System.Text.Encoding.UTF8, true))
                        {
                            while (streamReader.Peek() >= 0)
                            {
                                allLines.Add(streamReader.ReadLine());
                            }
                        }

                        // Read the CSV file and generate a language
                        report = _localizationService.FromCsv(languageCulture, allLines);
                        unitOfWork.Commit();
                    }
                    else
                    {
                        report.Errors.Add(new CsvErrorWarning
                        {
                            ErrorWarningType = CsvErrorWarningType.BadDataFormat,
                            Message = "File does not contain a language."
                        });
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    report.Errors.Add(new CsvErrorWarning
                    {
                        ErrorWarningType = CsvErrorWarningType.GeneralError,
                        Message = string.Format("Unable to import language: {0}", ex.Message)
                    });
                }

                return new WrappedJsonResult { Data = ToJSON(report) }; 
            }
        }

        public ActionResult Languages()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var importExportViewModel = new LanguagesHomeViewModel();

                // For languages we need a list of export languages and import languages
                var languageImportExportViewModel = new LanguageImportExportViewModel
                {
                    ExportLanguages = _localizationService.LanguagesInDb,
                    ImportLanguages = _localizationService.LanguagesAll
                };
                importExportViewModel.LanguageViewModel = languageImportExportViewModel;

                return View(importExportViewModel);
            }
        }

        public ActionResult Members()
        {
            return View();
        }

        #region GenerateLanguageFile

        /// <summary>
        /// Appends missing lines from one csv file in "~/Installer" folder to another 
        /// OR creates a copy of source file if the destination file does not exist.
        /// </summary>
        public ActionResult GenerateLanguageFile(string from = "en-GB", string to = "ru-RU")
        {
            var fromFile = GetLanguageFileAsDictionary($"~/Installer/{from}.csv");
            if (fromFile == null)
            {
                return Content(string.Empty);
            }

            var toFile = GetLanguageFileAsDictionary($"~/Installer/{to}.csv");

            var result = toFile == null
                ? fromFile
                : fromFile.ToDictionary(i => i.Key, i => toFile.ContainsKey(i.Key) ? toFile[i.Key] : i.Value);

            System.IO.File.WriteAllLines(Request.MapPath($"~/Installer/{to}.csv"),
                result.Select(i => $"{i.Key},{i.Value}").ToArray(), Encoding.UTF8);

            return Content(string.Empty);
        }

        private Dictionary<string, string> GetLanguageFileAsDictionary(string fileName)
        {
            var mappedFileName = Request.MapPath(fileName);

            if (!System.IO.File.Exists(mappedFileName))
            {
                return null;
            }

            return
                System.IO.File.ReadLines(mappedFileName)
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Where(l => l.Contains(","))
                    .Select(GetKeyValue)
                    .ToList()
                    .ToDictionary(l => l[0], l => l[1]);
        }

        private static string[] GetKeyValue(string line)
        {
            var i = line.IndexOf(",", StringComparison.Ordinal);
            return new[]
            {
                line.Substring(0, i),
                line.Substring(i + 1)
            };
        }

        #endregion
    }
}
