using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorReservationDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultReservationUncheckedBankIds",
                table: "SystemSetting",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultReservationUncheckedPolicyIds",
                table: "SystemSetting",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "ShowCompanySignatureInReservationPrint",
                table: "SystemSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCustomerSignatureInReservationPrint",
                table: "SystemSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowStampInReservationPrint",
                table: "SystemSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultReservationUncheckedBankIds",
                table: "SystemSetting");

            migrationBuilder.DropColumn(
                name: "DefaultReservationUncheckedPolicyIds",
                table: "SystemSetting");

            migrationBuilder.DropColumn(
                name: "ShowCompanySignatureInReservationPrint",
                table: "SystemSetting");

            migrationBuilder.DropColumn(
                name: "ShowCustomerSignatureInReservationPrint",
                table: "SystemSetting");

            migrationBuilder.DropColumn(
                name: "ShowStampInReservationPrint",
                table: "SystemSetting");
        }
    }
}
