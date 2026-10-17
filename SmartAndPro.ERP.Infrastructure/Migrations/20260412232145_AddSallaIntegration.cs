using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSallaIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SallaOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SallaOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    FulfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SallaCustomerId = table.Column<long>(type: "bigint", nullable: true),
                    ShippingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippingDistrict = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippingStreet = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShippingLat = table.Column<double>(type: "float", nullable: true),
                    ShippingLng = table.Column<double>(type: "float", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SallaPaymentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SaleInvoiceId = table.Column<int>(type: "int", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    SyncError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SallaCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SallaOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SallaProductMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SallaProductId = table.Column<long>(type: "bigint", nullable: false),
                    SallaSku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SallaProductName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SallaPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProductUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductUnitName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SallaProductMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SallaSyncLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestPayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SallaResourceId = table.Column<long>(type: "bigint", nullable: true),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SallaSyncLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SallaOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SallaOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SallaProductId = table.Column<long>(type: "bigint", nullable: false),
                    SallaSku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OptionsSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProductUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SallaOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SallaOrderItems_SallaOrders_SallaOrderId",
                        column: x => x.SallaOrderId,
                        principalTable: "SallaOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SallaOrderItems_ProductUnitId",
                table: "SallaOrderItems",
                column: "ProductUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SallaOrderItems_SallaOrderId",
                table: "SallaOrderItems",
                column: "SallaOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SallaOrderItems_SallaProductId",
                table: "SallaOrderItems",
                column: "SallaProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SallaOrders_ReferenceNumber",
                table: "SallaOrders",
                column: "ReferenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SallaOrders_SaleInvoiceId",
                table: "SallaOrders",
                column: "SaleInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SallaOrders_SallaCreatedAt",
                table: "SallaOrders",
                column: "SallaCreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SallaOrders_SallaOrderId",
                table: "SallaOrders",
                column: "SallaOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SallaOrders_Status",
                table: "SallaOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SallaOrders_SyncStatus",
                table: "SallaOrders",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SallaProductMappings_ProductUnitId",
                table: "SallaProductMappings",
                column: "ProductUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SallaProductMappings_SallaProductId",
                table: "SallaProductMappings",
                column: "SallaProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SallaSyncLogs_CreatedAt",
                table: "SallaSyncLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SallaSyncLogs_EventType",
                table: "SallaSyncLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_SallaSyncLogs_SallaResourceId",
                table: "SallaSyncLogs",
                column: "SallaResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_SallaSyncLogs_Status",
                table: "SallaSyncLogs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SallaOrderItems");

            migrationBuilder.DropTable(
                name: "SallaProductMappings");

            migrationBuilder.DropTable(
                name: "SallaSyncLogs");

            migrationBuilder.DropTable(
                name: "SallaOrders");
        }
    }
}
