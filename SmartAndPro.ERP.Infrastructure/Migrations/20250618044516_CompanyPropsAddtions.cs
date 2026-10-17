using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanyPropsAddtions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankBranchName",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolicyParagraph",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SwiftCode",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnifiedNumber",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "CompanyDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "BankBranchName",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "PolicyParagraph",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "SwiftCode",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "UnifiedNumber",
                table: "CompanyDetails");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "CompanyDetails");
        }
    }
}
