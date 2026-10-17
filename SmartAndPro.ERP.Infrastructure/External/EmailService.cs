using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SmartAndPro.ERP.Infrastructure.External;

public static class EmailService
{
    public static async Task SendMail(string to, string subject, string body)
    {
        var email = new MimeMessage
        {
            Sender = MailboxAddress.Parse("smart.and.pro.erp@gmail.com"),
            Subject = subject
        };
        email.To.Add(MailboxAddress.Parse(to));

        var builder = new BodyBuilder();


        builder.HtmlBody = body;
        email.Body = builder.ToMessageBody();
        email.From.Add(new MailboxAddress("SMART ERP SmartAndPro", "smart.and.pro.erp@gmail.com"));

        using var smtp = new SmtpClient();
        smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        smtp.Authenticate("smart.and.pro.erp@gmail.com", "tuuldpsuzjzedyxm");
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }
}