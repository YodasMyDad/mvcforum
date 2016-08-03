using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels.Mapping;
using MembershipUser = MVCForum.Domain.DomainModel.MembershipUser;
using System.Text;
using Microsoft.Reporting.WebForms;


namespace MVCForum.Website.Areas.Admin.Controllers
{
    public class ReportsController : BaseAdminController
    {

        public ReportsController(ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
        }

        // GET: Admin/Reports
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public FileContentResult MembershipCheck(string format = "pdf")
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {

                var allRegisteredUsers = MembershipService.GetAllRegistered();
                var allUsers = MembershipService.GetAllActive();

                    var allViewModelUsers = allRegisteredUsers
                        .Select(ViewModelMapping.RegistrationToSingleRegisteredMemberListViewModel)
                        .Join(allUsers, 
                            RegisteredUser => RegisteredUser.MembershipFirmId, 
                            User => User.MembershipFirm.Id,
                            (RegisteredUser , User) => ViewModelMapping.RegisterUserToRegisteredMemberListViewModel(RegisteredUser, User))
                        .OrderBy(x => x.RegMembershipFirmName)
                        .ToList();


                var localReport = new LocalReport();
                localReport.ReportPath = Server.MapPath("/content/reports/MembershipCheck.rdlc");

                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Name = "DataSet1";
                reportDataSource.Value = allViewModelUsers;
                localReport.DataSources.Add(reportDataSource);

                ////Pass parameter to RDLC
                //string _companyName = "I'm param";
                //string _info = "this is brabra...";
                //ReportParameter pCompanyName = new Microsoft.Reporting.WebForms.ReportParameter("CompanyName", _companyName);
                //ReportParameter pInfo = new Microsoft.Reporting.WebForms.ReportParameter("Info", _info);
                //localReport.SetParameters(new ReportParameter[] { pCompanyName, pInfo });

                //Setting file
                string reportType = format;
                string mimeType, encoding, fileNameExtension;
                string deviceInfo =
                "<DeviceInfo>" +
                    "  <OutputFormat>+format+</OutputFormat>" +
                    "  <PageWidth>8.5in</PageWidth>" +
                    "  <PageHeight>11in</PageHeight>" +
                    "  <MarginTop>0.25in</MarginTop>" +
                    "  <MarginLeft>0.25in</MarginLeft>" +
                    "  <MarginRight>0.25in</MarginRight>" +
                    "  <MarginBottom>0.25in</MarginBottom>" +
                "</DeviceInfo>";

                Warning[] warnings;
                string[] streams;
                byte[] renderedBytes;
                //Render the report            
                renderedBytes = localReport.Render(
                    reportType,
                    deviceInfo,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);
                return File(renderedBytes, mimeType);

            }
        }

        [Authorize]
        public FileContentResult LanyardInserts(string format = "pdf")
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var allUsers = MembershipService.GetAllRegistered();
                var allViewModelUsers = allUsers.Select(ViewModelMapping.RegistrationToSingleMemberListViewModel).OrderBy(x => x.MembershipFirmName).ToList();

                var localReport = new LocalReport();
                localReport.ReportPath = Server.MapPath("/content/reports/LanyardInserts.rdlc");

                ReportDataSource reportDataSource = new ReportDataSource();
                reportDataSource.Name = "DataSet1";
                reportDataSource.Value = allViewModelUsers;
                localReport.DataSources.Add(reportDataSource);

                ////Pass parameter to RDLC
                //string _companyName = "I'm param";
                //string _info = "this is brabra...";
                //ReportParameter pCompanyName = new Microsoft.Reporting.WebForms.ReportParameter("CompanyName", _companyName);
                //ReportParameter pInfo = new Microsoft.Reporting.WebForms.ReportParameter("Info", _info);
                //localReport.SetParameters(new ReportParameter[] { pCompanyName, pInfo });

                //Setting file
                string reportType = format;
                string mimeType, encoding, fileNameExtension;
                string deviceInfo =
                "<DeviceInfo>" +
                    "  <OutputFormat>+format+</OutputFormat>" +
                    "  <PageWidth>8.5in</PageWidth>" +
                    "  <PageHeight>11in</PageHeight>" +
                    "  <MarginTop>0.25in</MarginTop>" +
                    "  <MarginLeft>0.25in</MarginLeft>" +
                    "  <MarginRight>0.25in</MarginRight>" +
                    "  <MarginBottom>0.25in</MarginBottom>" +
                "</DeviceInfo>";

                Warning[] warnings;
                string[] streams;
                byte[] renderedBytes;
                //Render the report            
                renderedBytes = localReport.Render(
                    reportType,
                    deviceInfo,
                    out mimeType,
                    out encoding,
                    out fileNameExtension,
                    out streams,
                    out warnings);
                return File(renderedBytes, mimeType);

            }
        }

    }
}