namespace SmartAndPro.ERP.Application.Services.Email;

public interface IEmailService
{
    Task<bool> SendBookingConfirmationAsync(string toEmail, string guestName, 
        string bookingReference, DateTime checkIn, DateTime checkOut, string hotelName, 
        decimal totalAmount, List<string> roomDetails);
    
    Task<bool> SendCancellationEmailAsync(string toEmail, string guestName, 
        string bookingReference, string reason);
    
    Task<bool> SendBookingCancellationEmailAsync(string toEmail, string guestName,
        string bookingReference, string hotelName, decimal refundAmount);
    
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false);
}
