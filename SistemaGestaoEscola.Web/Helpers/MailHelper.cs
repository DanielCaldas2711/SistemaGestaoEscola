using SistemaGestaoEscola.Web.Helpers.Interfaces;
using SistemaGestaoEscola.Web.Models;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;

namespace SistemaGestaoEscola.Web.Helpers
{
    public class MailHelper : IMailHelper
    {
        
        private readonly MailSettings _mailSettings;

        public MailHelper(IOptions<MailSettings> options)
        {
            _mailSettings = options.Value;
        }

        public Response SendEmail(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_mailSettings.NameFrom, _mailSettings.From));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            var bodybuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodybuilder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Connect(_mailSettings.Smtp, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(_mailSettings.From, _mailSettings.Password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.ToString()

                };
            }

            return new Response
            {
                IsSuccess = true,
            };
        }
    }
}
