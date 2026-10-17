using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHeldOrdersAndPaymentEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "CheckoutIdempotencyRecords",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "HeldOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TerminalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    RequestedByUserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeldAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeldOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentProviderEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentProviderEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HeldOrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeldOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductUnitId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeldOrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HeldOrderLines_HeldOrders_HeldOrderId",
                        column: x => x.HeldOrderId,
                        principalTable: "HeldOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HeldOrderLines_HeldOrderId",
                table: "HeldOrderLines",
                column: "HeldOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentProviderEvents_Provider_EventId",
                table: "PaymentProviderEvents",
                columns: new[] { "Provider", "EventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeldOrderLines");

            migrationBuilder.DropTable(
                name: "PaymentProviderEvents");

            migrationBuilder.DropTable(
                name: "HeldOrders");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "CheckoutIdempotencyRecords");
        }
    }
}
