using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace SmartAndPro.ERP.Application.Services.Email;

/// <summary>
/// Email service for sending booking confirmations and notifications
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _username;
    private readonly string _password;
    private readonly bool _enableSsl;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _fromEmail = configuration["Email:FromEmail"] ?? "";
        _fromName = configuration["Email:FromName"] ?? "Smart Hotel Booking";
        _username = configuration["Email:Username"] ?? "";
        _password = configuration["Email:Password"] ?? "";
        _enableSsl = bool.Parse(configuration["Email:EnableSsl"] ?? "true");
    }

    public async Task<bool> SendBookingConfirmationAsync(string toEmail, string guestName, 
        string bookingReference, DateTime checkIn, DateTime checkOut, string hotelName, 
        decimal totalAmount, List<string> roomDetails)
    {
        try
        {
            var subject = $"تأكيد حجزك - {bookingReference} | Booking Confirmation - {bookingReference}";
            var body = GenerateBookingConfirmationHtml(guestName, bookingReference, checkIn, 
                checkOut, hotelName, totalAmount, roomDetails);

            return await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending booking confirmation email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendCancellationEmailAsync(string toEmail, string guestName, 
        string bookingReference, string reason)
    {
        try
        {
            var subject = $"إلغاء الحجز - {bookingReference} | Booking Cancellation - {bookingReference}";
            var body = GenerateCancellationHtml(guestName, bookingReference, reason);

            return await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending cancellation email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendBookingCancellationEmailAsync(string toEmail, string guestName,
        string bookingReference, string hotelName, decimal refundAmount)
    {
        try
        {
            var subject = $"تأكيد إلغاء الحجز - {bookingReference} | Booking Cancellation Confirmation";
            var body = GenerateBookingCancellationWithRefundHtml(guestName, bookingReference, 
                hotelName, refundAmount);

            return await SendEmailAsync(toEmail, subject, body, isHtml: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending booking cancellation email: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, 
        bool isHtml = false)
    {
        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_fromEmail, _fromName);
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;

            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
            smtpClient.Credentials = new NetworkCredential(_username, _password);
            smtpClient.EnableSsl = _enableSsl;

            await smtpClient.SendMailAsync(message);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending email: {ex.Message}");
            return false;
        }
    }

    private string GenerateBookingConfirmationHtml(string guestName, string bookingReference, 
        DateTime checkIn, DateTime checkOut, string hotelName, decimal totalAmount, 
        List<string> roomDetails)
    {
        var rooms = string.Join("", roomDetails.Select(r => $"<li style='margin: 5px 0;'>{r}</li>"));
        
        return $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>تأكيد الحجز</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='margin: 0; font-size: 28px;'>تأكيد الحجز</h1>
        <p style='margin: 10px 0 0; font-size: 16px;'>Booking Confirmation</p>
    </div>
    
    <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <p style='font-size: 18px; margin-top: 0;'>عزيزي/عزيزتي <strong>{guestName}</strong>,</p>
        <p style='font-size: 16px;'>شكراً لحجزك معنا! نؤكد استلام حجزك بالتفاصيل التالية:</p>
        
        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #667eea;'>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>رقم الحجز:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: left; color: #667eea; font-weight: bold;'>{bookingReference}</td>
                </tr>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>الفندق:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: left;'>{hotelName}</td>
                </tr>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>تاريخ الوصول:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: left;'>{checkIn:dd/MM/yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>تاريخ المغادرة:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: left;'>{checkOut:dd/MM/yyyy}</td>
                </tr>
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #eee;'><strong>عدد الليالي:</strong></td>
                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: left;'>{(checkOut - checkIn).Days} ليلة</td>
                </tr>
                <tr>
                    <td style='padding: 10px;'><strong>المبلغ الإجمالي:</strong></td>
                    <td style='padding: 10px; text-align: left; color: #28a745; font-size: 20px; font-weight: bold;'>{totalAmount:N2} ر.س</td>
                </tr>
            </table>
        </div>

        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h3 style='color: #667eea; margin-top: 0;'>تفاصيل الغرف:</h3>
            <ul style='list-style: none; padding: 0;'>{rooms}</ul>
        </div>

        <div style='background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 8px; margin: 20px 0;'>
            <p style='margin: 0; color: #856404;'><strong>ملاحظة:</strong> يرجى إحضار نسخة من هذا البريد الإلكتروني أو رقم الحجز عند تسجيل الوصول.</p>
        </div>

        <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 2px solid #eee;'>
            <p style='color: #666; font-size: 14px; margin: 5px 0;'>نتطلع لاستقبالك!</p>
            <p style='color: #666; font-size: 14px; margin: 5px 0;'>We look forward to welcoming you!</p>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #999; font-size: 12px;'>
        <p>© 2025 Smart Hotel Booking System. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    private string GenerateCancellationHtml(string guestName, string bookingReference, string reason)
    {
        return $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>إلغاء الحجز</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='margin: 0; font-size: 28px;'>إلغاء الحجز</h1>
        <p style='margin: 10px 0 0; font-size: 16px;'>Booking Cancellation</p>
    </div>
    
    <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <p style='font-size: 18px; margin-top: 0;'>عزيزي/عزيزتي <strong>{guestName}</strong>,</p>
        <p style='font-size: 16px;'>نأسف لإبلاغك بأنه تم إلغاء حجزك رقم <strong style='color: #f5576c;'>{bookingReference}</strong>.</p>
        
        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #f5576c;'>
            <p style='margin: 0;'><strong>سبب الإلغاء:</strong></p>
            <p style='margin: 10px 0 0; color: #666;'>{reason}</p>
        </div>

        <div style='background: #d1ecf1; border: 1px solid #bee5eb; padding: 15px; border-radius: 8px; margin: 20px 0;'>
            <p style='margin: 0; color: #0c5460;'>إذا كان لديك أي استفسارات، يرجى الاتصال بنا.</p>
        </div>

        <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 2px solid #eee;'>
            <p style='color: #666; font-size: 14px;'>نأمل أن نخدمك في المستقبل القريب</p>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #999; font-size: 12px;'>
        <p>© 2025 Smart Hotel Booking System. All rights reserved.</p>
    </div>
</body>
</html>";
    }

    private string GenerateBookingCancellationWithRefundHtml(string guestName, string bookingReference,
        string hotelName, decimal refundAmount)
    {
        return $@"
<!DOCTYPE html>
<html dir='rtl' lang='ar'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>تأكيد إلغاء الحجز</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0;'>
        <h1 style='margin: 0; font-size: 28px;'>تأكيد إلغاء الحجز</h1>
        <p style='margin: 10px 0 0; font-size: 16px;'>Booking Cancellation Confirmed</p>
    </div>
    
    <div style='background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px;'>
        <p style='font-size: 18px; margin-top: 0;'>عزيزي/عزيزتي <strong>{guestName}</strong>,</p>
        <p style='font-size: 16px;'>تم إلغاء حجزك بنجاح.</p>
        
        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
            <div style='display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #eee;'>
                <span style='color: #666;'>Booking Reference | رقم الحجز</span>
                <strong style='color: #667eea;'>{bookingReference}</strong>
            </div>
            <div style='display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #eee;'>
                <span style='color: #666;'>Hotel | الفندق</span>
                <strong>{hotelName}</strong>
            </div>
            <div style='display: flex; justify-content: space-between; padding: 10px 0; border-bottom: 1px solid #eee;'>
                <span style='color: #666;'>Refund Amount | المبلغ المسترد</span>
                <strong style='color: #28a745; font-size: 18px;'>{refundAmount:F2} SAR</strong>
            </div>
            <div style='display: flex; justify-content: space-between; padding: 10px 0;'>
                <span style='color: #666;'>Status | الحالة</span>
                <strong style='color: #ffc107;'>Pending | قيد المعالجة</strong>
            </div>
        </div>

        <div style='background: #d4edda; border: 1px solid #c3e6cb; padding: 15px; border-radius: 8px; margin: 20px 0;'>
            <p style='margin: 0; color: #155724;'>
                <strong>ملاحظة:</strong> سيتم معالجة المبلغ المسترد خلال 5-7 أيام عمل ويتم إيداعه في نفس طريقة الدفع المستخدمة.
            </p>
            <p style='margin: 10px 0 0; color: #155724; font-size: 14px;'>
                <strong>Note:</strong> The refund will be processed within 5-7 business days and credited to your original payment method.
            </p>
        </div>

        <div style='background: #fff3cd; border: 1px solid #ffc107; padding: 15px; border-radius: 8px; margin: 20px 0;'>
            <p style='margin: 0; color: #856404;'>
                إذا كان لديك أي استفسارات بخصوص المبلغ المسترد، يرجى الاتصال بخدمة العملاء.
            </p>
        </div>

        <div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 2px solid #eee;'>
            <p style='color: #666; font-size: 14px; margin: 5px 0;'>نأمل أن نخدمك في المستقبل القريب</p>
            <p style='color: #666; font-size: 14px; margin: 5px 0;'>We hope to serve you again in the near future</p>
        </div>
    </div>
    
    <div style='text-align: center; padding: 20px; color: #999; font-size: 12px;'>
        <p>© 2025 Smart Hotel Booking System. All rights reserved.</p>
    </div>
</body>
</html>";
    }
}
