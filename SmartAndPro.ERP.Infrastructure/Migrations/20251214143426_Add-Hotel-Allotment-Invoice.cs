using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelAllotmentInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HotelAllotmentInvoiceId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HotelAllotmentInvoiceId",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HotelAllotmentInvoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TotalNumberOfRooms = table.Column<int>(type: "int", nullable: false),
                    TotalNumberOfDays = table.Column<int>(type: "int", nullable: false),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsFrontal = table.Column<bool>(type: "bit", nullable: false),
                    TotalCostBeforeVAT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelAllotmentInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HotelAllotmentInvoice_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HotelAllotmentInvoice_MainAccount_MainAccountId",
                        column: x => x.MainAccountId,
                        principalTable: "MainAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HotelAllotmentInvoice_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "HotelAllotmentRoom",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelAllotmentInvoiceId = table.Column<int>(type: "int", nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfNights = table.Column<int>(type: "int", nullable: false),
                    NumberOfRooms = table.Column<int>(type: "int", nullable: false),
                    MealPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MealPlanCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ViewCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricePerDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomsPerDay = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotelAllotmentRoom", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HotelAllotmentRoom_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                        column: x => x.HotelAllotmentInvoiceId,
                        principalTable: "HotelAllotmentInvoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HotelAllotmentRoom_ReservationMeal_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "ReservationMeal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HotelAllotmentRoom_ReservationView_ViewId",
                        column: x => x.ViewId,
                        principalTable: "ReservationView",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HotelAllotmentRoom_RoomType_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "RoomType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentSchedule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VAT = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryNo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InvoiceType = table.Column<int>(type: "int", nullable: false),
                    HotelAllotmentInvoiceId = table.Column<int>(type: "int", nullable: true),
                    HotelAllotmentInvoiceId1 = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentSchedule_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                        column: x => x.HotelAllotmentInvoiceId,
                        principalTable: "HotelAllotmentInvoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentSchedule_HotelAllotmentInvoice_HotelAllotmentInvoiceId1",
                        column: x => x.HotelAllotmentInvoiceId1,
                        principalTable: "HotelAllotmentInvoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_HotelAllotmentInvoiceId",
                table: "Transaction",
                column: "HotelAllotmentInvoiceId",
                unique: true,
                filter: "[HotelAllotmentInvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_HotelAllotmentInvoiceId",
                table: "Attachments",
                column: "HotelAllotmentInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelAllotmentInvoice_HotelId",
                table: "HotelAllotmentInvoice",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelAllotmentInvoice_InvoiceNo",
                table: "HotelAllotmentInvoice",
                column: "InvoiceNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HotelAllotmentInvoice_MainAccountId",
                table: "HotelAllotmentInvoice",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelAllotmentInvoice_TransactionId",
                table: "HotelAllotmentInvoice",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HotelAllotmentRoom_HotelAllotmentInvoiceId",
                table: "HotelAllotmentRoom",
                column: "HotelAllotmentInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelAllotmentRoom_MealPlanId",
                table: "HotelAllotmentRoom",
                column: "MealPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelAllotmentRoom_RoomTypeId",
                table: "HotelAllotmentRoom",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HotelAllotmentRoom_ViewId",
                table: "HotelAllotmentRoom",
                column: "ViewId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_EntryNo",
                table: "PaymentSchedule",
                column: "EntryNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_HotelAllotmentInvoiceId",
                table: "PaymentSchedule",
                column: "HotelAllotmentInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_HotelAllotmentInvoiceId1",
                table: "PaymentSchedule",
                column: "HotelAllotmentInvoiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                table: "Attachments",
                column: "HotelAllotmentInvoiceId",
                principalTable: "HotelAllotmentInvoice",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                table: "Transaction",
                column: "HotelAllotmentInvoiceId",
                principalTable: "HotelAllotmentInvoice",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "HotelAllotmentRoom");

            migrationBuilder.DropTable(
                name: "PaymentSchedule");

            migrationBuilder.DropTable(
                name: "HotelAllotmentInvoice");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_HotelAllotmentInvoiceId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_HotelAllotmentInvoiceId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "HotelAllotmentInvoiceId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "HotelAllotmentInvoiceId",
                table: "Attachments");
        }
    }
}
