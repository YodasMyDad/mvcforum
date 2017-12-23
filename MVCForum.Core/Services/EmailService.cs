namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Web.Hosting;
    using Domain.DomainModel;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Utilities;

    public partial class EmailService : IEmailService
    {
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        private readonly MVCForumContext _context;

        public EmailService(ILoggingService loggingService, ISettingsService settingsService, IMVCForumContext context)
        {
            _loggingService = loggingService;
            _settingsService = settingsService;
            _context = context as MVCForumContext;
        }

        public Email Add(Email email)
        {
            return _context.Email.Add(email);
        }

        public void Delete(Email email)
        {
            _context.Email.Remove(email);
        }

        public List<Email> GetAll(int amountToTake)
        {
            return _context.Email.OrderBy(x => x.DateCreated).Take(amountToTake).ToList();
        }

        public void ProcessMail(int amountToSend)
        {
            try
            {
                // Get the amount of emails to send in this batch
                var emails = GetAll(amountToSend);

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
                    if (string.IsNullOrEmpty(smtp))
                    {
                        // Not logging as it makes the log file massive
                        //_loggingService.Error("There are no SMTP details in the settings, unable to send emails");
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
                        Delete(sentEmail);
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
                if (!string.IsNullOrEmpty(to))
                {
                    to = $"<p>{to},</p>";
                    sb = sb.Replace("#TO#", to);
                }

                return sb;
            }
        }

        public void SendMail(Email email, Settings settings)
        {
            SendMail(new List<Email> { email }, settings);
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
            var settings = _settingsService.GetSettings();
            SendMail(emails, settings);
        }

        public void SendMail(List<Email> emails, Settings settings)
        {
            // Add all the emails to the email table
            // They are sent every X seconds by the email sending task
            foreach (var email in emails)
            {

                // Sort local images in emails
                email.Body = StringUtils.AppendDomainToImageUrlInHtml(email.Body, settings.ForumUrl.TrimEnd('/'));
                Add(email);
            }
        }
    }
}
