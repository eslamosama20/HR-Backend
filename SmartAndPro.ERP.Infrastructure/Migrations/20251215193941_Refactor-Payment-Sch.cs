using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPaymentSch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentSchedule_EntryNo",
                table: "PaymentSchedule");

            migrationBuilder.AlterColumn<string>(
                name: "EntryNo",
                table: "PaymentSchedule",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_EntryNo",
                table: "PaymentSchedule",
                column: "EntryNo",
                unique: true,
                filter: "[EntryNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentSchedule_EntryNo",
                table: "PaymentSchedule");

            migrationBuilder.AlterColumn<string>(
                name: "EntryNo",
                table: "PaymentSchedule",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_EntryNo",
                table: "PaymentSchedule",
                column: "EntryNo",
                unique: true);
        }
    }
}
