namespace SmartAndPro.ERP.Application.Services.SMS;

public interface ISmsService
{
    Task<bool> SendBookingConfirmationSmsAsync(string phoneNumber, string guestName, 
        string bookingReference, string hotelName, DateTime checkIn);
    
    Task<bool> SendCancellationSmsAsync(string phoneNumber, string guestName, 
        string bookingReference);
    
    Task<bool> SendBookingCancellationSmsAsync(string phoneNumber, string guestName,
        string bookingReference, decimal refundAmount);
    
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}
