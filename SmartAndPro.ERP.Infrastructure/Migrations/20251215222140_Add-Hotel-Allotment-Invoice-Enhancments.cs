using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelAllotmentInvoiceEnhancments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentSchedule_EntryNo",
                table: "PaymentSchedule");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceType",
                table: "PaymentSchedule",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "EntryNo",
                table: "PaymentSchedule",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "InvoiceType",
                table: "PaymentSchedule",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EntryNo",
                table: "PaymentSchedule",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_EntryNo",
                table: "PaymentSchedule",
                column: "EntryNo",
                unique: true,
                filter: "[EntryNo] IS NOT NULL");
        }
    }
}
