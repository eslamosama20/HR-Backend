using Microsoft.AspNetCore.SignalR;

namespace SmartAndPro.ERP.Infrastructure.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications
/// Handles broadcasting booking notifications to admin users
/// </summary>
public class NotificationHub : Hub
{
    /// <summary>
    /// Send notification to all connected admin clients
    /// </summary>
    public async Task SendNotificationToAll(string title, string message, string type, string priority)
    {
        await Clients.All.SendAsync("ReceiveNotification", new
        {
            title,
            message,
            type,
            priority,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send notification to specific user
    /// </summary>
    public async Task SendNotificationToUser(string userId, string title, string message, string type, string priority)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", new
        {
            title,
            message,
            type,
            priority,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send new booking notification to admins
    /// </summary>
    public async Task SendBookingNotification(string bookingReference, string guestName, string hotelName, decimal amount)
    {
        await Clients.All.SendAsync("NewBooking", new
        {
            bookingReference,
            guestName,
            hotelName,
            amount,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send booking cancellation notification
    /// </summary>
    public async Task SendCancellationNotification(string bookingReference, string reason)
    {
        await Clients.All.SendAsync("BookingCancelled", new
        {
            bookingReference,
            reason,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send low inventory alert
    /// </summary>
    public async Task SendLowInventoryAlert(string roomTypeName, int remainingQuantity)
    {
        await Clients.All.SendAsync("LowInventory", new
        {
            roomTypeName,
            remainingQuantity,
            timestamp = DateTime.UtcNow
        });
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
    }
}
