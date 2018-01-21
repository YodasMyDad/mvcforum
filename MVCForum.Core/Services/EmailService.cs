namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Web.Hosting;
    using ExtensionMethods;
    using Hangfire;
    using Interfaces.Services;
    using Models;
    using Models.Entities;
    using Utilities;

    public partial class EmailService : IEmailService
    {
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;

        public EmailService(ILoggingService loggingService, ISettingsService settingsService)
        {
            _loggingService = loggingService;
            _settingsService = settingsService;
        }

        public void ProcessMail(List<Email> emails)
        {
            try
            {
                // See if there are any
                if (emails != null && emails.Any())
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
                    if (string.IsNullOrWhiteSpace(smtp))
                    {
                        // Not logging as it makes the log file massive
                        //_loggingService.Error("There are no SMTP details in the settings, unable to send emails");
                        return;
                    }

                    // Set up the SMTP Client object and settings
                    var mySmtpClient = new SmtpClient(smtp);
                    if (!string.IsNullOrWhiteSpace(smtpUsername) && !string.IsNullOrWhiteSpace(smtpPassword))
                    {
                        mySmtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    }

                    if (smtpEnableSsl != null)
                    {
                        mySmtpClient.EnableSsl = (bool) smtpEnableSsl;
                    }

                    if (!string.IsNullOrWhiteSpace(smtpPort))
                    {
                        mySmtpClient.Port = Convert.ToInt32(smtpPort);
                    }

                    // Loop through email email create a mailmessage and send
                    foreach (var message in emails)
                    {
                        try
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
                        }
                        catch (Exception ex)
                        {
                            _loggingService.Error($"EXCEPTION sending mail to {message.EmailTo}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
        }

        /// <summary>
        ///     Returns the HTML email template with values replaced
        /// </summary>
        /// <param name="to"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public string EmailTemplate(string to, string content)
        {
            var settings = _settingsService.GetSettings();
            return EmailTemplate(to, content, settings);
        }

        public string EmailTemplate(string to, string content, Settings settings)
        {
            using (var sr = File.OpenText(HostingEnvironment.MapPath(@"~/Content/Emails/EmailNotification.htm")))
            {
                var sb = sr.ReadToEnd();
                sr.Close();
                sb = sb.Replace("#CONTENT#", content);
                sb = sb.Replace("#SITENAME#", settings.ForumName);
                sb = sb.Replace("#SITEURL#", settings.ForumUrl);
                if (!string.IsNullOrWhiteSpace(to))
                {
                    to = $"<p>{to},</p>";
                    sb = sb.Replace("#TO#", to);
                }

                return sb;
            }
        }

        public void SendMail(Email email, Settings settings)
        {
            SendMail(new List<Email> {email}, settings);
        }

        /// <summary>
        ///     Send a single email
        /// </summary>
        /// <param name="email"></param>
        public void SendMail(Email email)
        {
            SendMail(new List<Email> {email});
        }

        /// <summary>
        ///     Send multiple emails
        /// </summary>
        /// <param name="emails"></param>
        public void SendMail(List<Email> emails)
        {
            var settings = _settingsService.GetSettings();
            SendMail(emails, settings);
        }

        public void SendMail(List<Email> emails, Settings settings)
        {
            // Sort out the email body
            foreach (var email in emails)
            {
                // Sort local images in emails
                email.Body = StringUtils.AppendDomainToImageUrlInHtml(email.Body, settings.ForumUrl.TrimEnd('/'));
            }

            // Now batch add to hangfire, 25 emails at a time
            foreach (var emailList in emails.ChunkBy(25))
            {
                // Fire with hangfire
                BackgroundJob.Enqueue<EmailService>(x => x.ProcessMail(emailList));
            }
        }
    }
}