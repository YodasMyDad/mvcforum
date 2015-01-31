using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Web.Hosting;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class EmailService : IEmailService
    {
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        private readonly IEmailRepository _emailRepository;

        public EmailService(ILoggingService loggingService, ISettingsService settingsService, IEmailRepository emailRepository)
        {
            _loggingService = loggingService;
            _settingsService = settingsService;
            _emailRepository = emailRepository;
        }

        public void ProcessMail(int amountToSend)
        {
            try
            {
                // Get the amount of emails to send in this batch
                var emails = _emailRepository.GetAll(amountToSend);

                // See if there are any
                if (emails != null && emails.Count > 0)
                {
                    // Get the mails settings
                    var settings = _settingsService.GetSettings(false);
                    var smtp = settings.SMTP;
                    var smtpUsername = settings.SMTPUsername;
                    var smtpPassword = settings.SMTPPassword;
                    var smtpPort = settings.SMTPPort;
                    var smtpEnableSsl = settings.SMTPEnableSSL;
                    var fromEmail = settings.NotificationReplyEmail;

                    // If no SMTP settings then log it
                    if (string.IsNullOrEmpty(smtp))
                    {
                        _loggingService.Error("There are no SMTP details in the settings, unable to send emails");
                        return;
                    }

                    // Set up the SMTP Client object and settings
                    var mySmtpClient = new SmtpClient(smtp);
                    if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                    {
                        mySmtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    }

                    if (smtpEnableSsl != null)
                    {
                        mySmtpClient.EnableSsl = (bool)smtpEnableSsl;
                    }

                    if (!string.IsNullOrEmpty(smtpPort))
                    {
                        mySmtpClient.Port = Convert.ToInt32(smtpPort);
                    }

                    // List to store the emails to delete after they are sent
                    var emailsToDelete = new List<Email>();

                    // Loop through email email create a mailmessage and send it
                    foreach (var message in emails)
                    {
                        var msg = new MailMessage
                        {
                            IsBodyHtml = true,
                            Body = message.Body,
                            From = new MailAddress(fromEmail),
                            Subject = message.Subject
                        };
                        msg.To.Add(message.EmailTo);
                        mySmtpClient.Send(msg);

                        emailsToDelete.Add(message);
                    }

                    // Loop through the sent emails and delete them
                    foreach (var sentEmail in emailsToDelete)
                    {
                        _emailRepository.Delete(sentEmail);
                    }
                   
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
        }

        /// <summary>
        /// Returns the HTML email template with values replaced
        /// </summary>
        /// <param name="to"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public string EmailTemplate(string to, string content)
        {
            using (var sr = File.OpenText(HostingEnvironment.MapPath(@"~/Content/Emails/EmailNotification.htm")))
            {
                var sb = sr.ReadToEnd();
                sr.Close();
                sb = sb.Replace("#CONTENT#", content);
                sb = sb.Replace("#SITENAME#", _settingsService.GetSettings().ForumName);
                sb = sb.Replace("#SITEURL#", _settingsService.GetSettings().ForumUrl);
                if(!string.IsNullOrEmpty(to))
                {
                    to = string.Format("<p>{0},</p>", to);
                    sb = sb.Replace("#TO#", to);
                }

                return sb;
            }
        }

        /// <summary>
        /// Send a single email
        /// </summary>
        /// <param name="email"></param>
        public void SendMail(Email email)
        {
            SendMail(new List<Email> { email });
        }

        /// <summary>
        /// Send multiple emails
        /// </summary>
        /// <param name="emails"></param>
        public void SendMail(List<Email> emails)
        {
            // Add all the emails to the email table
            // They are sent every X seconds by the email sending task
            foreach (var email in emails)
            {
                _emailRepository.Add(email);
            }
        }
    }
}
