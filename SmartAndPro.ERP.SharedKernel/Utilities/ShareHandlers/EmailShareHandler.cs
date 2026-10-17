using Microsoft.Extensions.Options;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;



namespace SmartAndPro.ERP.SharedKernel.Utilities.ShareHandlers
{
    public class EmailShareHandler : ShareHandler
    {
        private readonly MailSettings _mailSettings;

        public EmailShareHandler(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value; 
        }
        public override string Name => "Email";

        public override Task Share(byte[] pdfBytes, string receiver)
        {
            return Share(pdfBytes, receiver, null, null, null);
        }

        public override Task Share(byte[] pdfBytes, string receiver, string? subject, string? body, string? attachmentName)
        {
            return Share(pdfBytes, receiver, subject, body, attachmentName, null);
        }

        public override async Task Share(byte[] pdfBytes, string receiver, string? subject, string? body, string? attachmentName, string? senderDisplayName)
        {
            if (_mailSettings == null || string.IsNullOrWhiteSpace(_mailSettings.Host) || string.IsNullOrWhiteSpace(_mailSettings.Email))
            {
                throw new InvalidOperationException("إعدادات البريد الإلكتروني غير مكتملة في النظام.");
            }

            try
            {
                using (var client = new SmtpClient(_mailSettings.Host, _mailSettings.Port > 0 ? _mailSettings.Port : 587))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_mailSettings.Email, _mailSettings.Password);
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 15000;

                    using (var message = new MailMessage())
                    {
                        var fromDisplayName = !string.IsNullOrWhiteSpace(senderDisplayName) 
                            ? senderDisplayName 
                            : (_mailSettings.DisplayName ?? "SMART ERP");

                        message.From = new MailAddress(_mailSettings.Email, fromDisplayName);
                        message.To.Add(receiver);
                        message.Subject = !string.IsNullOrWhiteSpace(subject) ? subject : "مستند - Smart ERP";
                        message.Body = !string.IsNullOrWhiteSpace(body) ? body : "مرفق مع هذه الرسالة الملف الخاص بكم.\n\nشكرًا لتواصلكم معنا.";
                        message.IsBodyHtml = !string.IsNullOrWhiteSpace(body) && (
                            body.Contains("<html", StringComparison.OrdinalIgnoreCase) || 
                            body.Contains("<!DOCTYPE", StringComparison.OrdinalIgnoreCase) || 
                            body.Contains("<div", StringComparison.OrdinalIgnoreCase) || 
                            body.Contains("<body", StringComparison.OrdinalIgnoreCase)
                        );

                        var attachName = !string.IsNullOrWhiteSpace(attachmentName) ? attachmentName : "Document.pdf";

                        using (var stream = new MemoryStream(pdfBytes))
                        {
                            message.Attachments.Add(new Attachment(stream, attachName, "application/pdf"));
                            await client.SendMailAsync(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailShareHandler] Error sending email: {ex.Message}");
                throw;
            }
        }
    }
}
