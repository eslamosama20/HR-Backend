using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.PublicBooking;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.Email;
using SmartAndPro.ERP.Application.Services.SMS;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Hubs;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.Application.Common.Extensions;

namespace SmartAndPro.ERP.Application.Services.PublicBooking;

public class PublicBookingService
    : ServiceBase<Domains.Entities.PublicBooking.PublicBooking, PublicBookingPostDto, PublicBookingPutDto, PublicBookingResponseDto, PublicBookingFiltersDto>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly IEmailService? _emailService;
    private readonly ISmsService? _smsService;
    private readonly IHubContext<NotificationHub>? _hubContext;

    // Constructor for ServiceBaseFactory (most common - used by factory pattern)
    public PublicBookingService(
        RepositoriesFactory repositoriesFactory,
        AppSettings appSettings,
        IMapper mapper,
        ServiceBaseFactory serviceBaseFactory)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
        _emailService = null;
        _smsService = null;
        _hubContext = null;
    }

    // Constructor with all dependencies (for full admin functionality with DI)
    public PublicBookingService(
        RepositoriesFactory repositoriesFactory,
        AppSettings appSettings,
        IMapper mapper,
        ServiceBaseFactory serviceBaseFactory,
        IEmailService emailService,
        ISmsService smsService,
        IHubContext<NotificationHub> hubContext)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
        _serviceBaseFactory = serviceBaseFactory;
        _emailService = emailService;
        _smsService = smsService;
        _hubContext = hubContext;
    }

    public override async Task<OperationResponse<PublicBookingResponseDto>> AddAsync(PublicBookingPostDto bookingDto)
    {
        var bookingRepo = _repositoriesFactory.GetRepository<Domains.Entities.PublicBooking.PublicBooking>(true);
        var bookingRoomRepo = _repositoriesFactory.GetRepository<PublicBookingRoom>(true);
        var bookingPaidServiceRepo = _repositoriesFactory.GetRepository<PublicBookingPaidService>(true);
        var roomTypeService = _serviceBaseFactory.GetService<PublicRoomTypeService>();

        try
        {
            await bookingRepo.BeginTransactionAsync();

            // Validate availability for all rooms
            foreach (var room in bookingDto.Rooms)
            {
                var availabilityResult = await roomTypeService.GetAvailableRoomsAsync(
                    room.RoomTypeId,
                    bookingDto.CheckInDate,
                    bookingDto.CheckOutDate);

                if (availabilityResult.ErrorCode != ErrorCodes.Success || availabilityResult.Data < room.Quantity)
                {
                    await bookingRepo.RollbackTransactionAsync();
                    return new OperationResponse<PublicBookingResponseDto>(
                        ErrorCodes.GeneralFail,
                        null,
                        null,
                        false,
                        $"Not enough rooms available. Requested: {room.Quantity}, Available: {availabilityResult.Data}");
                }
            }

            // Create booking entity
            var booking = _mapper.Map<Domains.Entities.PublicBooking.PublicBooking>(bookingDto);
            booking.BookingReference = GenerateBookingReference();
            booking.Status = PublicBookingStatus.Pending;
            booking.PaymentStatus = PaymentStatus.Pending;
            booking.FinalAmount = bookingDto.TotalAmount - bookingDto.DiscountAmount;

            var addResult = await bookingRepo.Add(booking);
            if (addResult.ErrorCode != ErrorCodes.Success)
            {
                await bookingRepo.RollbackTransactionAsync();
                return new OperationResponse<PublicBookingResponseDto>(ErrorCodes.GeneralFail, null);
            }

            // Add booking rooms
            foreach (var roomDto in bookingDto.Rooms)
            {
                var bookingRoom = new PublicBookingRoom
                {
                    BookingId = booking.Id,
                    RoomTypeId = roomDto.RoomTypeId,
                    Quantity = roomDto.Quantity,
                    PricePerNight = roomDto.PricePerNight,
                    TotalPrice = roomDto.PricePerNight * roomDto.Quantity * 
                                (decimal)(bookingDto.CheckOutDate - bookingDto.CheckInDate).TotalDays
                };

                await bookingRoomRepo.Add(bookingRoom);

                // Decrease available quantity
                var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>(true);
                var roomTypeQuery = await roomTypeRepo.GetAll();
                var roomType = await roomTypeQuery.Data
                    .FirstOrDefaultAsync(rt => rt.Id == roomDto.RoomTypeId);
                if (roomType != null)
                {
                    roomType.AvailableQuantity -= roomDto.Quantity;
                    await roomTypeRepo.Update(roomType);
                }
            }

            // Add paid services if any
            if (bookingDto.PaidServices != null && bookingDto.PaidServices.Any())
            {
                foreach (var serviceDto in bookingDto.PaidServices)
                {
                    var bookingPaidService = new PublicBookingPaidService
                    {
                        PublicBookingId = booking.Id,
                        PublicHotelPaidServiceId = serviceDto.PublicHotelPaidServiceId,
                        PriceAtBooking = serviceDto.PriceAtBooking,
                        Quantity = serviceDto.Quantity,
                        TotalPrice = serviceDto.PriceAtBooking * serviceDto.Quantity,
                        CustomerNotes = serviceDto.CustomerNotes
                    };

                    await bookingPaidServiceRepo.Add(bookingPaidService);
                }
            }

            await bookingRepo.CommitTransactionAsync();

            // Get full booking details for email/SMS
            var response = await GetAsync(booking.Id);
            
            // Send confirmation email and SMS
            if (response.Data != null)
            {
                var roomDetails = response.Data.Rooms.Select(r => 
                    $"{r.RoomTypeName} - {r.Quantity} غرفة × {r.PricePerNight:N2} ر.س/ليلة"
                ).ToList();

                // Send email (if service available)
                if (_emailService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _emailService.SendBookingConfirmationAsync(
                            bookingDto.GuestEmail,
                            bookingDto.GuestName,
                            booking.BookingReference,
                            bookingDto.CheckInDate,
                            bookingDto.CheckOutDate,
                            response.Data.HotelName,
                            response.Data.FinalAmount,
                            roomDetails
                        );
                    });
                }

                // Send SMS (if service available)
                if (_smsService != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _smsService.SendBookingConfirmationSmsAsync(
                            bookingDto.GuestPhone,
                            bookingDto.GuestName,
                            booking.BookingReference,
                            response.Data.HotelName,
                            bookingDto.CheckInDate
                        );
                    });
                }

                // Create notification (if repository available)
                try
                {
                    var notificationRepo = _repositoriesFactory.GetRepository<SystemNotification>(true);
                    var notification = new SystemNotification
                    {
                        Title = $"حجز جديد - {booking.BookingReference}",
                        Message = $"تم استلام حجز جديد من {bookingDto.GuestName} للفندق {response.Data.HotelName}",
                        Type = "NewBooking",
                        BookingId = booking.Id,
                        Priority = "High",
                        ActionUrl = $"/admin/bookings/{booking.Id}"
                    };
                    await notificationRepo.Add(notification);
                }
                catch
                {
                    // Ignore notification errors - booking is already saved
                }

                // Send SignalR real-time notification (if hub available)
                if (_hubContext != null)
                {
                    _ = Task.Run(async () =>
                    {
                        await _hubContext.Clients.All.SendAsync("NewBooking", new
                        {
                            bookingReference = booking.BookingReference,
                            guestName = bookingDto.GuestName,
                            hotelName = response.Data.HotelName,
                            amount = response.Data.FinalAmount,
                            timestamp = DateTime.UtcNow
                        });
                    });
                }
            }

            // Return final response
            return response;
        }
        catch (Exception ex)
        {
            await bookingRepo.RollbackTransactionAsync();
            return new OperationResponse<PublicBookingResponseDto>(
                ErrorCodes.GeneralFail,
                null,
                null,
                false,
                ex.Message);
        }
    }

    public override async Task<OperationResponse<PublicBookingResponseDto>> GetAsync(Guid id)
    {
        var bookingRepo = _repositoriesFactory.GetRepository<Domains.Entities.PublicBooking.PublicBooking>();
        var bookingQuery = await bookingRepo.GetAll();

        var booking = await bookingQuery.Data
            .Include(b => b.Hotel)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.RoomType)
            .Include(b => b.BookingPaidServices)
                .ThenInclude(bps => bps.HotelPaidService)
                    .ThenInclude(hps => hps.PublicPaidService)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
        {
            return new OperationResponse<PublicBookingResponseDto>(ErrorCodes.NotFound, null);
        }

        var response = _mapper.Map<PublicBookingResponseDto>(booking);
        response.HotelName = booking.Hotel?.Name ?? "";
        response.NumberOfNights = (booking.CheckOutDate - booking.CheckInDate).Days;
        response.Rooms = booking.BookingRooms?.Select(br => new BookingRoomResponseDto
        {
            RoomTypeId = br.RoomTypeId,
            RoomTypeName = br.RoomType?.Name ?? "",
            Quantity = br.Quantity,
            PricePerNight = br.PricePerNight,
            TotalPrice = br.TotalPrice
        }).ToList() ?? new List<BookingRoomResponseDto>();

        // Map paid services
        response.PaidServices = booking.BookingPaidServices?.Select(bps => new PublicBookingPaidServiceDto
        {
            Id = bps.Id,
            PublicHotelPaidServiceId = bps.PublicHotelPaidServiceId,
            PriceAtBooking = bps.PriceAtBooking,
            Quantity = bps.Quantity,
            CustomerNotes = bps.CustomerNotes,
            Service = _mapper.Map<PublicPaidServiceResponseDto>(bps.HotelPaidService?.PublicPaidService)
        }).ToList() ?? new List<PublicBookingPaidServiceDto>();

        return new OperationResponse<PublicBookingResponseDto>(ErrorCodes.Success, response);
    }

    public override async Task<OperationResponse<List<PublicBookingResponseDto>>> GetAllAsync(
        PublicBookingFiltersDto filters,
        int? pageNumber = null,
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null)
    {
        var bookingRepo = _repositoriesFactory.GetRepository<Domains.Entities.PublicBooking.PublicBooking>();
        var query = await bookingRepo.GetAll();

        var bookingsQuery = query.Data
            .Include(b => b.Hotel)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.RoomType)
            .AsQueryable();

        // Apply SearchKeyword
        bookingsQuery = bookingsQuery.ApplySearchKeyword(filters.SearchKeyword);

        // Apply filters
        if (filters.HotelId.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.HotelId == filters.HotelId.Value);
        }

        if (!string.IsNullOrEmpty(filters.BookingReference))
        {
            bookingsQuery = bookingsQuery.Where(b => b.BookingReference.Contains(filters.BookingReference));
        }

        if (!string.IsNullOrEmpty(filters.GuestName))
        {
            bookingsQuery = bookingsQuery.Where(b => b.GuestName.Contains(filters.GuestName));
        }

        if (!string.IsNullOrEmpty(filters.GuestEmail))
        {
            bookingsQuery = bookingsQuery.Where(b => b.GuestEmail.Contains(filters.GuestEmail));
        }

        if (!string.IsNullOrEmpty(filters.GuestPhone))
        {
            bookingsQuery = bookingsQuery.Where(b => b.GuestPhone.Contains(filters.GuestPhone));
        }

        if (filters.Status.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.Status == filters.Status.Value);
        }

        if (filters.PaymentStatus.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.PaymentStatus == filters.PaymentStatus.Value);
        }

        if (filters.CheckInFrom.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CheckInDate >= filters.CheckInFrom.Value);
        }

        if (filters.CheckInTo.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CheckInDate <= filters.CheckInTo.Value);
        }

        if (filters.CheckOutFrom.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CheckOutDate >= filters.CheckOutFrom.Value);
        }

        if (filters.CheckOutTo.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CheckOutDate <= filters.CheckOutTo.Value);
        }

        if (filters.BookingDateFrom.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CreatedAt >= filters.BookingDateFrom.Value);
        }

        if (filters.BookingDateTo.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.CreatedAt <= filters.BookingDateTo.Value);
        }

        if (filters.MinAmount.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.FinalAmount >= filters.MinAmount.Value);
        }

        if (filters.MaxAmount.HasValue)
        {
            bookingsQuery = bookingsQuery.Where(b => b.FinalAmount <= filters.MaxAmount.Value);
        }

        // Pagination
        if (pageNumber.HasValue && pageSize.HasValue)
        {
            var skip = (pageNumber.Value - 1) * pageSize.Value;
            bookingsQuery = bookingsQuery
                .ApplySort(sortField, sortOrder)
                .Skip(skip)
                .Take(pageSize.Value);
        }
        else
        {
            bookingsQuery = bookingsQuery.ApplySort(sortField, sortOrder);
        }

        var bookings = await bookingsQuery.ToListAsync();
        var response = _mapper.Map<List<PublicBookingResponseDto>>(bookings);

        foreach (var booking in response)
        {
            var bookingEntity = bookings.First(b => b.Id == booking.Id);
            booking.HotelName = bookingEntity.Hotel?.Name ?? "";
            booking.NumberOfNights = (bookingEntity.CheckOutDate - bookingEntity.CheckInDate).Days;
            booking.Rooms = bookingEntity.BookingRooms?.Select(br => new BookingRoomResponseDto
            {
                RoomTypeId = br.RoomTypeId,
                RoomTypeName = br.RoomType?.Name ?? "",
                Quantity = br.Quantity,
                PricePerNight = br.PricePerNight,
                TotalPrice = br.TotalPrice
            }).ToList() ?? new List<BookingRoomResponseDto>();
        }

        return new OperationResponse<List<PublicBookingResponseDto>>(ErrorCodes.Success, response);
    }

    public async Task<OperationResponse<PublicBookingResponseDto>> ConfirmBookingAsync(Guid bookingId)
    {
        var bookingRepo = _repositoriesFactory.GetRepository<Domains.Entities.PublicBooking.PublicBooking>(true);
        var bookingQuery = await bookingRepo.GetAll();
        var booking = await bookingQuery.Data.FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return new OperationResponse<PublicBookingResponseDto>(ErrorCodes.NotFound, null);
        }

        booking.Status = PublicBookingStatus.Confirmed;
        booking.ConfirmedAt = DateTime.UtcNow;
        await bookingRepo.Update(booking);

        return await GetAsync(bookingId);
    }

    public async Task<OperationResponse<PublicBookingResponseDto>> CancelBookingAsync(Guid bookingId, string reason)
    {
        var bookingRepo = _repositoriesFactory.GetRepository<Domains.Entities.PublicBooking.PublicBooking>(true);
        var bookingRoomRepo = _repositoriesFactory.GetRepository<PublicBookingRoom>(true);
        var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>(true);

        try
        {
            await bookingRepo.BeginTransactionAsync();

            var bookingQuery = await bookingRepo.GetAll();
            var booking = await bookingQuery.Data
                .Include(b => b.BookingRooms)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                await bookingRepo.RollbackTransactionAsync();
                return new OperationResponse<PublicBookingResponseDto>(ErrorCodes.NotFound, null);
            }

            // Restore available quantity for all booked rooms
            foreach (var bookingRoom in booking.BookingRooms)
            {
                var roomTypeQuery = await roomTypeRepo.GetAll();
                var roomType = await roomTypeQuery.Data
                    .FirstOrDefaultAsync(rt => rt.Id == bookingRoom.RoomTypeId);
                if (roomType != null)
                {
                    roomType.AvailableQuantity += bookingRoom.Quantity;
                    await roomTypeRepo.Update(roomType);
                }
            }

            booking.Status = PublicBookingStatus.Cancelled;
            booking.CancellationReason = reason;
            booking.CancelledAt = DateTime.UtcNow;
            await bookingRepo.Update(booking);

            await bookingRepo.CommitTransactionAsync();

            // Send cancellation notifications (if services available)
            if (_emailService != null)
            {
                _ = Task.Run(async () =>
                {
                    await _emailService.SendCancellationEmailAsync(
                        booking.GuestEmail,
                        booking.GuestName,
                        booking.BookingReference,
                        reason
                    );
                });
            }

            if (_smsService != null)
            {
                _ = Task.Run(async () =>
                {
                    await _smsService.SendCancellationSmsAsync(
                        booking.GuestPhone,
                        booking.GuestName,
                        booking.BookingReference
                    );
                });
            }

            // Create notification
            var notificationRepo = _repositoriesFactory.GetRepository<SystemNotification>(true);
            var notification = new SystemNotification
            {
                Title = $"إلغاء حجز - {booking.BookingReference}",
                Message = $"تم إلغاء الحجز رقم {booking.BookingReference} للعميل {booking.GuestName}",
                Type = "CancelBooking",
                BookingId = booking.Id,
                Priority = "Medium",
                ActionUrl = $"/admin/bookings/{booking.Id}"
            };
            await notificationRepo.Add(notification);

            return await GetAsync(bookingId);
        }
        catch (Exception ex)
        {
            await bookingRepo.RollbackTransactionAsync();
            return new OperationResponse<PublicBookingResponseDto>(
                ErrorCodes.GeneralFail,
                null,
                null,
                false,
                ex.Message);
        }
    }

    public async Task<OperationResponse<PublicBookingResponseDto>> UpdatePaymentStatusAsync(
        Guid bookingId,
        PaymentStatus paymentStatus,
        string? transactionId = null,
        int? paymobOrderId = null)
    {
        var bookingRepo = _repositoriesFactory.GetRepository<Domains.Entities.PublicBooking.PublicBooking>(true);
        var bookingQuery = await bookingRepo.GetAll();
        var booking = await bookingQuery.Data.FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return new OperationResponse<PublicBookingResponseDto>(ErrorCodes.NotFound, null);
        }

        booking.PaymentStatus = paymentStatus;
        if (!string.IsNullOrEmpty(transactionId))
        {
            booking.PaymentTransactionId = transactionId;
        }

        if (paymobOrderId.HasValue)
        {
            booking.PaymobOrderId = paymobOrderId;
        }

        if (paymentStatus == PaymentStatus.Paid)
        {
            booking.Status = PublicBookingStatus.Confirmed;
            booking.ConfirmedAt = DateTime.UtcNow;
        }

        await bookingRepo.Update(booking);

        return await GetAsync(bookingId);
    }

    /// <summary>
    /// Cancel a booking and process refund if applicable
    /// </summary>
    public async Task<OperationResponse<PublicBookingResponseDto>> CancelBookingAsync(
        Guid bookingId,
        string cancellationReason,
        bool processRefund = true)
    {
        var bookingRepo = _repositoriesFactory.GetRepository<Domains.Entities.PublicBooking.PublicBooking>(true);
        var bookingQuery = await bookingRepo.GetAll();
        var booking = await bookingQuery.Data
            .Include(b => b.Hotel)
            .Include(b => b.BookingRooms)
                .ThenInclude(br => br.RoomType)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return new OperationResponse<PublicBookingResponseDto>(
                ErrorCodes.NotFound,
                null,
                null,
                false,
                "Booking not found");
        }

        // Check if already cancelled
        if (booking.Status == PublicBookingStatus.Cancelled)
        {
            return new OperationResponse<PublicBookingResponseDto>(
                ErrorCodes.GeneralFail,
                null,
                null,
                false,
                "Booking is already cancelled");
        }

        // Check if check-in has already occurred
        if (booking.CheckInDate < DateTime.UtcNow.Date)
        {
            return new OperationResponse<PublicBookingResponseDto>(
                ErrorCodes.GeneralFail,
                null,
                null,
                false,
                "Cannot cancel booking after check-in date has passed");
        }

        try
        {
            await bookingRepo.BeginTransactionAsync();

            // Update booking status
            booking.Status = PublicBookingStatus.Cancelled;
            booking.CancelledAt = DateTime.UtcNow;
            booking.CancellationReason = cancellationReason;

            // Process refund if payment was made
            if (processRefund && booking.PaymentStatus == PaymentStatus.Paid)
            {
                // Calculate refund based on cancellation policy
                var daysUntilCheckIn = (booking.CheckInDate - DateTime.UtcNow.Date).Days;
                decimal refundPercentage = 1.0m; // Default 100% refund

                // Example cancellation policy (customize as needed)
                if (daysUntilCheckIn < 1)
                {
                    refundPercentage = 0m; // No refund for same-day cancellation
                }
                else if (daysUntilCheckIn < 3)
                {
                    refundPercentage = 0.5m; // 50% refund within 3 days
                }
                else if (daysUntilCheckIn < 7)
                {
                    refundPercentage = 0.75m; // 75% refund within 7 days
                }
                // else 100% refund for 7+ days

                booking.RefundAmount = booking.TotalAmount * refundPercentage;
                booking.RefundStatus = refundPercentage > 0
                    ? RefundStatus.Pending
                    : RefundStatus.Rejected;

                // TODO: Integrate with payment gateway to process actual refund
                // For now, mark as pending and process manually or via payment service
                if (refundPercentage > 0)
                {
                    // Placeholder for payment gateway refund call
                    // await _paymentService.ProcessRefund(booking.PaymentTransactionId, booking.RefundAmount);
                    // booking.RefundStatus = RefundStatus.Completed;
                    // booking.RefundedAt = DateTime.UtcNow;
                    // booking.RefundTransactionId = refundResponse.TransactionId;
                }
            }
            else if (booking.PaymentStatus == PaymentStatus.Pending)
            {
                // No payment made, no refund needed
                booking.PaymentStatus = PaymentStatus.Failed; // Mark payment as failed
            }

            await bookingRepo.Update(booking);

            // Release room inventory back to available quantity
            if (booking.BookingRooms != null && booking.BookingRooms.Any())
            {
                var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>(true);
                var roomTypeQuery = await roomTypeRepo.GetAll();

                foreach (var bookingRoom in booking.BookingRooms)
                {
                    var roomType = await roomTypeQuery.Data
                        .FirstOrDefaultAsync(rt => rt.Id == bookingRoom.RoomTypeId);

                    if (roomType != null)
                    {
                        // Restore available quantity
                        roomType.AvailableQuantity += bookingRoom.Quantity;
                        await roomTypeRepo.Update(roomType);
                    }
                }
            }

            // Send cancellation notification via Email/SMS (if services available)
            if (_emailService != null)
            {
                _ = Task.Run(async () =>
                {
                    await _emailService.SendBookingCancellationEmailAsync(
                        booking.GuestEmail,
                        booking.GuestName,
                        booking.BookingReference,
                        booking.Hotel?.Name ?? "",
                        booking.RefundAmount ?? 0
                    );
                });
            }

            if (_smsService != null && !string.IsNullOrEmpty(booking.GuestPhone))
            {
                _ = Task.Run(async () =>
                {
                    await _smsService.SendBookingCancellationSmsAsync(
                        booking.GuestPhone,
                        booking.GuestName,
                        booking.BookingReference,
                        booking.RefundAmount ?? 0
                    );
                });
            }

            // Create system notification
            var notificationRepo = _repositoriesFactory.GetRepository<SystemNotification>(true);
            var notification = new SystemNotification
            {
                Title = $"إلغاء حجز - {booking.BookingReference}",
                Message = $"تم إلغاء الحجز {booking.BookingReference} من قبل {booking.GuestName}. المبلغ المسترد: {booking.RefundAmount:F2}",
                Type = "BookingCancellation",
                BookingId = booking.Id,
                Priority = "Medium",
                ActionUrl = $"/admin/bookings/{booking.Id}"
            };
            await notificationRepo.Add(notification);

            // Send SignalR real-time notification
            _ = Task.Run(async () =>
            {
                await _hubContext.Clients.All.SendAsync("BookingCancelled", new
                {
                    bookingReference = booking.BookingReference,
                    guestName = booking.GuestName,
                    hotelName = booking.Hotel?.Name ?? "",
                    refundAmount = booking.RefundAmount,
                    refundStatus = booking.RefundStatus?.ToString(),
                    timestamp = DateTime.UtcNow
                });
            });

            await bookingRepo.CommitTransactionAsync();

            // Return updated booking
            return await GetAsync(bookingId);
        }
        catch (Exception ex)
        {
            await bookingRepo.RollbackTransactionAsync();
            return new OperationResponse<PublicBookingResponseDto>(
                ErrorCodes.GeneralFail,
                null,
                null,
                false,
                $"Failed to cancel booking: {ex.Message}");
        }
    }

    private string GenerateBookingReference()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"BK{timestamp}{random}";
    }

    /// <summary>
    /// TEMPORARY: Reset room availability (for testing purposes)
    /// </summary>
    public async Task ResetRoomAvailabilityAsync(Guid roomTypeId, int quantity)
    {
        var roomTypeRepo = _repositoriesFactory.GetRepository<PublicRoomType>();
        var roomTypesResult = await roomTypeRepo.Filter(r => r.Id == roomTypeId);
        
        if (roomTypesResult.ErrorCode != ErrorCodes.Success || roomTypesResult.Data == null)
        {
            throw new Exception($"Room type with ID {roomTypeId} not found");
        }
        
        var roomType = roomTypesResult.Data.FirstOrDefault();
        if (roomType == null)
        {
            throw new Exception($"Room type with ID {roomTypeId} not found");
        }

        roomType.AvailableQuantity = quantity;
        await roomTypeRepo.Update(roomType);
    }
}
