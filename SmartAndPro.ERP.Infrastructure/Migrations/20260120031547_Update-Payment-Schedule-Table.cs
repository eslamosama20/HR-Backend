using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentScheduleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentSchedule_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                table: "PaymentSchedule");

            migrationBuilder.DropIndex(
                name: "IX_PaymentSchedule_HotelAllotmentInvoiceId",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "HotelAllotmentInvoiceId",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "PaymentDay",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "InvoiceType",
                table: "HotelAllotmentInvoice");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceType",
                table: "PaymentSchedule",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceId",
                table: "PaymentSchedule",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "PaymentSchedule",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_Invoice",
                table: "PaymentSchedule",
                columns: new[] { "InvoiceType", "InvoiceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentSchedule_Invoice",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "PaymentSchedule");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceType",
                table: "PaymentSchedule",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "HotelAllotmentInvoiceId",
                table: "PaymentSchedule",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentDay",
                table: "PaymentSchedule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceType",
                table: "HotelAllotmentInvoice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_HotelAllotmentInvoiceId",
                table: "PaymentSchedule",
                column: "HotelAllotmentInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentSchedule_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                table: "PaymentSchedule",
                column: "HotelAllotmentInvoiceId",
                principalTable: "HotelAllotmentInvoice",
                principalColumn: "Id");
        }
    }
}
