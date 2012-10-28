using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        public EmailService(ILoggingService loggingService, ISettingsService settingsService)
        {
            _loggingService = loggingService;
            _settingsService = settingsService;
        }

        /// <summary>
        /// Returns the HTML email template with values replaced
        /// </summary>
        /// <param name="to"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public string EmailTemplate(string to, string content)
        {
            using (var sr = File.OpenText(HttpContext.Current.Server.MapPath(@"~/Content/Emails/EmailNotification.htm")))
            {
                var sb = sr.ReadToEnd();
                sr.Close();
                sb = sb.Replace("#CONTENT#", content);
                
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
        /// <param name="email"></param>
        /// NOTE: This implementation has a max send of 100 to stop mail server black listing
        public void SendMail(List<Email> email)
        {
            try
            {
                if (email != null && email.Count > 0)
                {

                    var smtp = _settingsService.GetSettings().SMTP;
                    var smtpUsername = _settingsService.GetSettings().SMTPUsername;
                    var smtpPassword = _settingsService.GetSettings().SMTPPassword;
                    var smtpPort = _settingsService.GetSettings().SMTPPort;

                    if (string.IsNullOrEmpty(smtp)) return;

                    var mySmtpClient = new SmtpClient(smtp);
                    if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
                    {
                        mySmtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    }

                    if(smtpPort != null)
                    {
                        mySmtpClient.Port = (int)smtpPort;
                    }

                    if (email.Count == 1)
                    {
                        var defaultEmail = email.FirstOrDefault();
                        if (defaultEmail != null)
                        {
                            var msg = new MailMessage
                            {
                                IsBodyHtml = true,
                                Body = defaultEmail.Body,
                                From = new MailAddress(defaultEmail.EmailFrom),
                                Subject = defaultEmail.Subject
                            };
                            msg.To.Add(defaultEmail.EmailTo);
                            mySmtpClient.Send(msg);
                        }
                    }
                    else
                    {
                        var count = 1;
                        foreach (var message in email)
                        {
                            // Throw exception if over 100, they need to use custom
                            // mail provider such as campaign monitor
                            if(count > 100)
                            {
                                _loggingService.Error(@"Unable to send more emails, over 100 limit - 
                                If you need to send more in one go, create a new email service with a dedicated provider");
                                break;
                            }

                            var msg = new MailMessage
                                          {
                                              IsBodyHtml = true,
                                              Body = message.Body,
                                              From = new MailAddress(message.EmailFrom),
                                              Subject = message.Subject
                                          };
                            msg.To.Add(message.EmailTo);
                            mySmtpClient.Send(msg);

                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error(ex);
            }
        }
    }
}
