using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentScheduales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentSchedule_HotelAllotmentInvoice_HotelAllotmentInvoiceId1",
                table: "PaymentSchedule");

            migrationBuilder.DropIndex(
                name: "IX_PaymentSchedule_HotelAllotmentInvoiceId1",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "HotelAllotmentInvoiceId1",
                table: "PaymentSchedule");

            migrationBuilder.AddColumn<string>(
                name: "PaymentDay",
                table: "PaymentSchedule",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentDay",
                table: "PaymentSchedule");

            migrationBuilder.AddColumn<int>(
                name: "HotelAllotmentInvoiceId1",
                table: "PaymentSchedule",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_HotelAllotmentInvoiceId1",
                table: "PaymentSchedule",
                column: "HotelAllotmentInvoiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentSchedule_HotelAllotmentInvoice_HotelAllotmentInvoiceId1",
                table: "PaymentSchedule",
                column: "HotelAllotmentInvoiceId1",
                principalTable: "HotelAllotmentInvoice",
                principalColumn: "Id");
        }
    }
}
