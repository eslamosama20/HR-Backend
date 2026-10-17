using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTermsButtonAndTimerInSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionTimeoutMinutes",
                table: "SystemSetting",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTermsInPrint2",
                table: "SystemSetting",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionTimeoutMinutes",
                table: "SystemSetting");

            migrationBuilder.DropColumn(
                name: "ShowTermsInPrint2",
                table: "SystemSetting");
        }
    }
}
