using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChannelManagerIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChannelConnections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    WebhookSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChannelPropertyId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApiEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConnectionCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelConnections_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChannelDeadLetterQueue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTable = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelDeadLetterQueue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelDeadLetterQueue_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChannelOutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    LockedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelOutboxEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelOutboxEvents_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChannelSyncLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseCode = table.Column<int>(type: "int", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelSyncLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelSyncLogs_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChannelWebhookQueue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalEventId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    LockedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelWebhookQueue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelWebhookQueue_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChannelRateMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ErpSeasonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelRateId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MarkupPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0.00m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelRateMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelRateMappings_ChannelConnections_ChannelConnectionId",
                        column: x => x.ChannelConnectionId,
                        principalTable: "ChannelConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelRateMappings_ReservationSeason_ErpSeasonId",
                        column: x => x.ErpSeasonId,
                        principalTable: "ReservationSeason",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChannelReservationMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ErpReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelReservationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OtaReservationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OtaName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OtaStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    RawPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelReservationMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelReservationMappings_ChannelConnections_ChannelConnectionId",
                        column: x => x.ChannelConnectionId,
                        principalTable: "ChannelConnections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChannelReservationMappings_Reservation_ErpReservationId",
                        column: x => x.ErpReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChannelRoomMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelConnectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ErpRoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelRoomId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MaxAllocationOverride = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelRoomMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelRoomMappings_ChannelConnections_ChannelConnectionId",
                        column: x => x.ChannelConnectionId,
                        principalTable: "ChannelConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChannelRoomMappings_RoomType_ErpRoomTypeId",
                        column: x => x.ErpRoomTypeId,
                        principalTable: "RoomType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChannelReservationPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelReservationMappingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentModel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false, defaultValue: 1.000000m),
                    OtaCommissionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.00m),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.00m),
                    ServiceCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.00m),
                    CityTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0.00m),
                    EncryptedVccCardNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EncryptedVccCvv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VccExpiryMonth = table.Column<int>(type: "int", nullable: true),
                    VccExpiryYear = table.Column<int>(type: "int", nullable: true),
                    VccActivationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    VccBalanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelReservationPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChannelReservationPayments_ChannelReservationMappings_ChannelReservationMappingId",
                        column: x => x.ChannelReservationMappingId,
                        principalTable: "ChannelReservationMappings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChannelConnections_HotelId",
                table: "ChannelConnections",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelDeadLetterQueue_HotelId",
                table: "ChannelDeadLetterQueue",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelOutboxEvents_HotelId",
                table: "ChannelOutboxEvents",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelOutboxEvents_IdempotencyKey",
                table: "ChannelOutboxEvents",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelOutboxEvents_Status",
                table: "ChannelOutboxEvents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRateMappings_ChannelConnectionId",
                table: "ChannelRateMappings",
                column: "ChannelConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRateMappings_ErpSeasonId",
                table: "ChannelRateMappings",
                column: "ErpSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelReservationMappings_ChannelConnectionId",
                table: "ChannelReservationMappings",
                column: "ChannelConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelReservationMappings_ChannelReservationId",
                table: "ChannelReservationMappings",
                column: "ChannelReservationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelReservationMappings_ErpReservationId",
                table: "ChannelReservationMappings",
                column: "ErpReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelReservationMappings_OtaReservationNumber",
                table: "ChannelReservationMappings",
                column: "OtaReservationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelReservationPayments_ChannelReservationMappingId",
                table: "ChannelReservationPayments",
                column: "ChannelReservationMappingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRoomMappings_ChannelConnectionId",
                table: "ChannelRoomMappings",
                column: "ChannelConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelRoomMappings_ErpRoomTypeId",
                table: "ChannelRoomMappings",
                column: "ErpRoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSyncLogs_CreatedAt",
                table: "ChannelSyncLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelSyncLogs_HotelId",
                table: "ChannelSyncLogs",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelWebhookQueue_ExternalEventId",
                table: "ChannelWebhookQueue",
                column: "ExternalEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChannelWebhookQueue_HotelId",
                table: "ChannelWebhookQueue",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelWebhookQueue_Status",
                table: "ChannelWebhookQueue",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChannelDeadLetterQueue");

            migrationBuilder.DropTable(
                name: "ChannelOutboxEvents");

            migrationBuilder.DropTable(
                name: "ChannelRateMappings");

            migrationBuilder.DropTable(
                name: "ChannelReservationPayments");

            migrationBuilder.DropTable(
                name: "ChannelRoomMappings");

            migrationBuilder.DropTable(
                name: "ChannelSyncLogs");

            migrationBuilder.DropTable(
                name: "ChannelWebhookQueue");

            migrationBuilder.DropTable(
                name: "ChannelReservationMappings");

            migrationBuilder.DropTable(
                name: "ChannelConnections");
        }
    }
}
