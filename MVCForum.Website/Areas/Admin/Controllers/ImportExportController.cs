namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using Application.CustomActionResults;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.General;
    using Newtonsoft.Json;
    using Web.ViewModels.Admin;

    [Authorize(Roles = Constants.AdminRoleName)]
    public class ImportExportController : BaseAdminController
    {
        private readonly ILocalizationService _localizationService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="membershipService"> </param>
        /// <param name="localizationService"></param>
        /// <param name="settingsService"> </param>
        /// <param name="context"></param>
        public ImportExportController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
            _localizationService = localizationService;
        }

        #region Private methods

        /// <summary>
        ///     Convert an import report into JSON data
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        private static object ToJson(CsvReport report)
        {
            var json = new
            {
                HasErrors = report.Errors.Any(),
                HasWarnings = report.Warnings.Any(),
                Warnings = JsonConvert.SerializeObject(report.Warnings.ExtractMessages()),
                Errors = JsonConvert.SerializeObject(report.Errors.ExtractMessages())
            };

            return json;
        }

        #endregion

        /// <summary>
        ///     We get here via the admin default layout (_AdminLayout). The returned view is displayed by
        ///     the @RenderBody in that layout
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        ///     Export a language in csv format
        /// </summary>
        /// <returns></returns>
        public CsvFileResult ExportUsers()
        {
            return new CsvFileResult {FileDownloadName = "MvcForumUsers.csv", Body = MembershipService.ToCsv()};
        }

        /// <summary>
        ///     Post of data for users import (file info)
        /// </summary>
        /// <param name="file">The name-value pairs for the language content</param>
        /// <returns></returns>
        [HttpPost]
        public WrappedJsonResult ImportUsers(HttpPostedFileBase file)
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
                    using (var streamReader = new StreamReader(file.InputStream, Encoding.UTF8, true))
                    {
                        while (streamReader.Peek() >= 0)
                        {
                            allLines.Add(streamReader.ReadLine());
                        }
                    }

                    // Read the CSV file and generate a language
                    report = MembershipService.FromCsv(allLines);
                    Context.SaveChanges();
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
                Context.RollBack();
                report.Errors.Add(new CsvErrorWarning
                {
                    ErrorWarningType = CsvErrorWarningType.GeneralError,
                    Message = $"Unable to import users: {ex.Message}"
                });
            }

            return new WrappedJsonResult {Data = ToJson(report)};
        }

        /// <summary>
        ///     Export a language in csv format
        /// </summary>
        /// <param name="languageCulture"></param>
        /// <returns></returns>
        public CsvFileResult ExportLanguage(string languageCulture)
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

        /// <summary>
        ///     Post of data for language import (file info)
        /// </summary>
        /// <param name="languageCulture">This defines the name etc of the imported language</param>
        /// <param name="file">The name-value pairs for the language content</param>
        /// <returns></returns>
        [HttpPost]
        public WrappedJsonResult ImportLanguage(string languageCulture, HttpPostedFileBase file)
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
                    using (var streamReader = new StreamReader(file.InputStream, Encoding.UTF8, true))
                    {
                        while (streamReader.Peek() >= 0)
                        {
                            allLines.Add(streamReader.ReadLine());
                        }
                    }

                    // Read the CSV file and generate a language
                    report = _localizationService.FromCsv(languageCulture, allLines);
                    Context.SaveChanges();
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
                Context.RollBack();
                report.Errors.Add(new CsvErrorWarning
                {
                    ErrorWarningType = CsvErrorWarningType.GeneralError,
                    Message = $"Unable to import language: {ex.Message}"
                });
            }

            return new WrappedJsonResult {Data = ToJson(report)};
        }

        public ActionResult Languages()
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

        public ActionResult Members()
        {
            return View();
        }
    }
}