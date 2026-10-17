using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingSystemAndEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicHotels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AddressAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MainImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalImages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amenities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckInTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CheckOutTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Policies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    ErpHotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicHotels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicBookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GuestEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CountryCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    GuestNationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumberOfAdults = table.Column<int>(type: "int", nullable: false),
                    NumberOfChildren = table.Column<int>(type: "int", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfNights = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SpecialRequests = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaymentTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RefundStatus = table.Column<int>(type: "int", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ErpReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicBookings_PublicHotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "PublicHotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicRoomTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DescriptionAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SizeInSqMeters = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxGuests = table.Column<int>(type: "int", nullable: false),
                    NumberOfBeds = table.Column<int>(type: "int", nullable: false),
                    BedType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeekendPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MainImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalImages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amenities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalRooms = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    ErpRoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicRoomTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicRoomTypes_PublicHotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "PublicHotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HotelReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    AdminResponse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdminResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HotelReviews_PublicBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "PublicBookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HotelReviews_PublicHotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "PublicHotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    GatewayTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CardLast4 = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CardBrand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerIpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_PublicBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "PublicBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicBookingRooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    PricePerNight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicBookingRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicBookingRooms_PublicBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "PublicBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PublicBookingRooms_PublicRoomTypes_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "PublicRoomTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PublicRoomPricings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeasonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SeasonNameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WeekdayPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WeekendPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinimumNights = table.Column<int>(type: "int", nullable: true),
                    EarlyBookingDiscount = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    EarlyBookingDays = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicRoomPricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicRoomPricings_PublicRoomTypes_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "PublicRoomTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomTypeSearchLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    NumberOfRooms = table.Column<int>(type: "int", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AvailabilityFound = table.Column<bool>(type: "bit", nullable: false),
                    AvailableRoomsCount = table.Column<int>(type: "int", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConvertedToBooking = table.Column<bool>(type: "bit", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTypeSearchLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomTypeSearchLogs_PublicBookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "PublicBookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RoomTypeSearchLogs_PublicHotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "PublicHotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomTypeSearchLogs_PublicRoomTypes_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "PublicRoomTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_HotelReviews_BookingId",
                table: "HotelReviews",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelReviews_HotelId",
                table: "HotelReviews",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_BookingId",
                table: "PaymentTransactions",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicBookingRooms_BookingId",
                table: "PublicBookingRooms",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicBookingRooms_RoomTypeId",
                table: "PublicBookingRooms",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicBookings_HotelId",
                table: "PublicBookings",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicRoomPricings_RoomTypeId",
                table: "PublicRoomPricings",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicRoomTypes_HotelId",
                table: "PublicRoomTypes",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeSearchLogs_BookingId",
                table: "RoomTypeSearchLogs",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeSearchLogs_HotelId",
                table: "RoomTypeSearchLogs",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTypeSearchLogs_RoomTypeId",
                table: "RoomTypeSearchLogs",
                column: "RoomTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HotelReviews");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PublicBookingRooms");

            migrationBuilder.DropTable(
                name: "PublicRoomPricings");

            migrationBuilder.DropTable(
                name: "RoomTypeSearchLogs");

            migrationBuilder.DropTable(
                name: "SystemNotifications");

            migrationBuilder.DropTable(
                name: "PublicBookings");

            migrationBuilder.DropTable(
                name: "PublicRoomTypes");

            migrationBuilder.DropTable(
                name: "PublicHotels");
        }
    }
}
