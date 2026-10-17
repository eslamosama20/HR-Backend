using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;

namespace SmartAndPro.ERP.Application.Services.SMS;

/// <summary>
/// SMS service using Twilio for sending booking confirmations
/// </summary>
public class SmsService : ISmsService
{
    private readonly IConfiguration _configuration;
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _fromNumber;
    private readonly bool _isEnabled;
    private readonly IHttpClientFactory _httpClientFactory;

    public SmsService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _accountSid = configuration["SmsSettings:TwilioAccountSid"] ?? "";
        _authToken = configuration["SmsSettings:TwilioAuthToken"] ?? "";
        _fromNumber = configuration["SmsSettings:TwilioPhoneNumber"] ?? "";
        _isEnabled = bool.Parse(configuration["SmsSettings:Enabled"] ?? "false");
    }

    public async Task<bool> SendBookingConfirmationSmsAsync(string phoneNumber, string guestName, 
        string bookingReference, string hotelName, DateTime checkIn)
    {
        if (!_isEnabled) return true; // Skip if SMS is disabled

        var message = $"مرحباً {guestName}،\n" +
                     $"تم تأكيد حجزك رقم {bookingReference}\n" +
                     $"الفندق: {hotelName}\n" +
                     $"تاريخ الوصول: {checkIn:dd/MM/yyyy}\n" +
                     $"نتطلع لاستقبالك!";

        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendCancellationSmsAsync(string phoneNumber, string guestName, 
        string bookingReference)
    {
        if (!_isEnabled) return true; // Skip if SMS is disabled

        var message = $"عزيزي {guestName}،\n" +
                     $"تم إلغاء حجزك رقم {bookingReference}.\n" +
                     $"للاستفسار يرجى الاتصال بنا.";

        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendBookingCancellationSmsAsync(string phoneNumber, string guestName,
        string bookingReference, decimal refundAmount)
    {
        if (!_isEnabled) return true; // Skip if SMS is disabled

        var message = $"عزيزي {guestName}،\n" +
                     $"تم إلغاء حجزك رقم {bookingReference}.\n" +
                     $"المبلغ المسترد: {refundAmount:F2} ريال\n" +
                     $"سيتم المعالجة خلال 5-7 أيام عمل.";

        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        if (!_isEnabled)
        {
            Console.WriteLine($"SMS disabled. Would send to {phoneNumber}: {message}");
            return true;
        }

        try
        {
            // Format phone number (remove spaces, ensure + prefix)
            var formattedPhone = phoneNumber.Replace(" ", "").Replace("-", "");
            if (!formattedPhone.StartsWith("+"))
            {
                formattedPhone = "+" + formattedPhone;
            }

            // Twilio API endpoint
            var url = $"https://api.twilio.com/2010-04-01/Accounts/{_accountSid}/Messages.json";

            // Create form data
            var formData = new Dictionary<string, string>
            {
                { "To", formattedPhone },
                { "From", _fromNumber },
                { "Body", message }
            };

            var content = new FormUrlEncodedContent(formData);

            using var httpClient = _httpClientFactory.CreateClient();
            
            // Add basic auth header
            var authString = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{_accountSid}:{_authToken}"));
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);

            // Send request
            var response = await httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"SMS sent successfully to {formattedPhone}");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"SMS send failed: {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending SMS: {ex.Message}");
            return false;
        }
    }
}
